using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    internal class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly SignInManager<SIOUser> _signInManager;

        public LogoutCommandHandler(IEventPublisher eventPublisher, IIdentityServerInteractionService interaction, IPersistedGrantService persistedGrantService, SignInManager<SIOUser> signInManager)
        {
            if (eventPublisher == null)
                throw new ArgumentNullException(nameof(eventPublisher));
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (persistedGrantService == null)
                throw new ArgumentNullException(nameof(persistedGrantService));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));

            _eventPublisher = eventPublisher;
            _interaction = interaction;
            _persistedGrantService = persistedGrantService;
            _signInManager = signInManager;
        }

        public async Task ExecuteAsync(LogoutCommand command)
        {
            await _signInManager.SignOutAsync();

            var logout = await _interaction.GetLogoutContextAsync(command.LogoutId);

            await _persistedGrantService.RemoveAllGrantsAsync(command.AggregateId.ToString(), logout?.ClientId);

            var userLoggedOutEvent = new UserLoggedOut(command.AggregateId, Guid.NewGuid(), command.AggregateId.ToString());
            userLoggedOutEvent.UpdateFrom(command);

            await _eventPublisher.PublishAsync(userLoggedOutEvent);
        }
    }
}
