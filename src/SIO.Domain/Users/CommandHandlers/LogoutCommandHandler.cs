using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    internal class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IEventManager _eventManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(IEventManager eventManager,
            IIdentityServerInteractionService interaction,
            IPersistedGrantService persistedGrantService,
            SignInManager<SIOUser> signInManager,
            ILogger<LogoutCommandHandler> logger)
        {
            if (eventManager == null)
                throw new ArgumentNullException(nameof(eventManager));
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (persistedGrantService == null)
                throw new ArgumentNullException(nameof(persistedGrantService));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _eventManager = eventManager;
            _interaction = interaction;
            _persistedGrantService = persistedGrantService;
            _signInManager = signInManager;
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
            await _eventManager.ProcessAsync(StreamId.New(), new UserLoggedOut(command.Subject));
        }
    }
}
