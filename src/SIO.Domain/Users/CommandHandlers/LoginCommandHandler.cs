using System;
using System.Threading;
using System.Threading.Tasks;
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
    internal class LoginCommandHandler : ICommandHandler<LoginCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IAggregateRepository<SIOStoreDbContext> _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(UserManager<SIOUser> userManager,
            SignInManager<SIOUser> signInManager,
            IAggregateRepository<SIOStoreDbContext> aggregateRepository,
            IAggregateFactory aggregateFactory,
            ILogger<LoginCommandHandler> logger)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (aggregateRepository == null)
                throw new ArgumentNullException(nameof(aggregateRepository));
            if (aggregateFactory == null)
                throw new ArgumentNullException(nameof(aggregateFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _userManager = userManager;
            _signInManager = signInManager;
            _aggregateRepository = aggregateRepository;
            _aggregateFactory = aggregateFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(LoginCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var user = await _userManager.FindByIdAsync(command.Subject);

            if (user == null)
                throw new UserDoesntExistException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserIsLockedOutException();

            var aggregate = await _aggregateRepository.GetAsync<User, UserState>(command.Subject, cancellationToken);
            var expectedVersion = aggregate.Version;

            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.UpdateAsync(user);
                aggregate.RequestToken(token);
                await _aggregateRepository.SaveAsync(aggregate, command, expectedVersion, cancellationToken);

                throw new UserNotVerifiedException();
            }

            if (user.IsArchived)
                throw new UserIsArchivedException();

            var validLogin = await _userManager.CheckPasswordAsync(user, command.Password);            

            if (!validLogin)
            {
                await _userManager.AccessFailedAsync(user);
                throw new IncorrectPasswordException();
            }

            aggregate.Login();

            await _signInManager.SignInAsync(user, false);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.UpdateAsync(user);            
            await _aggregateRepository.SaveAsync(aggregate, command, expectedVersion, cancellationToken);
        }
    }
}
