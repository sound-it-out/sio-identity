using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    internal class LogoutCommandHandler : ICommandHandler<LogoutCommand>
    {
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly SignInManager<SIOUser> _signInManager;

        public LogoutCommandHandler(IEventBusPublisher eventBusPublisher, IIdentityServerInteractionService interaction, IPersistedGrantService persistedGrantService, SignInManager<SIOUser> signInManager)
        {
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (persistedGrantService == null)
                throw new ArgumentNullException(nameof(persistedGrantService));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));

            _eventBusPublisher = eventBusPublisher;
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

            await _eventBusPublisher.PublishAsync(userLoggedOutEvent);
        }
    }
}
