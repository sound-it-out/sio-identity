using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Aggregates;
using SIO.Domain.Users.Commands;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    internal class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IAggregateRepository<SIOStoreDbContext> _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(IIdentityServerInteractionService interaction,
            IPersistedGrantService persistedGrantService,
            SignInManager<SIOUser> signInManager,
            IAggregateRepository<SIOStoreDbContext> aggregateRepository,
            IAggregateFactory aggregateFactory,
            ILogger<LogoutCommandHandler> logger)
        {
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (persistedGrantService == null)
                throw new ArgumentNullException(nameof(persistedGrantService));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (aggregateRepository == null)
                throw new ArgumentNullException(nameof(aggregateRepository));
            if (aggregateFactory == null)
                throw new ArgumentNullException(nameof(aggregateFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _interaction = interaction;
            _persistedGrantService = persistedGrantService;
            _signInManager = signInManager;
            _aggregateRepository = aggregateRepository;
            _aggregateFactory = aggregateFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(LogoutCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(LogoutCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await _signInManager.SignOutAsync();

            var logout = await _interaction.GetLogoutContextAsync(command.LogoutId);

            await _persistedGrantService.RemoveAllGrantsAsync(command.Subject, logout?.ClientId);

            var aggregate = await _aggregateRepository.GetAsync<User, UserState>(command.Subject, cancellationToken);
            var expectedVersion = aggregate.Version;

            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            aggregate.Logout();

            await _aggregateRepository.SaveAsync(aggregate, command, expectedVersion, cancellationToken);
        }
    }
}
