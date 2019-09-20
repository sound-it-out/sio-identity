using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    internal class VerifyUserCommandHandler : ICommandHandler<VerifyUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBus _eventBus;

        public VerifyUserCommandHandler(UserManager<SIOUser> userManager, IEventBus eventBus)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (eventBus == null)
                throw new ArgumentNullException(nameof(eventBus));

            _userManager = userManager;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(VerifyUserCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var user = await _userManager.FindByIdAsync(command.AggregateId.ToString());

            var userVerifiedResult = await _userManager.ConfirmEmailAsync(user, command.Token);

            if (!userVerifiedResult.Succeeded)
                throw new UserCommandException($"Failed to verify user: {command.AggregateId} {Environment.NewLine}Errors: {string.Join(Environment.NewLine, userVerifiedResult.Errors.Select(e => $"code: {e.Code}, description: {e.Description}"))}");

            await _eventBus.PublishAsync(new UserVerified(
                aggregateId: command.AggregateId,
                version: 1
            ));
        }
    }
}
