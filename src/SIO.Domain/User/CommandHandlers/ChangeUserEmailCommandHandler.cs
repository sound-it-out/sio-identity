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
    internal class ChangeUserEmailCommandHandler : ICommandHandler<ChangeUserEmailCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBus _eventBus;

        public ChangeUserEmailCommandHandler(UserManager<SIOUser> userManager, IEventBus eventBus)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (eventBus == null)
                throw new ArgumentNullException(nameof(eventBus));

            _userManager = userManager;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(ChangeUserEmailCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if ((await _userManager.FindByEmailAsync(command.Email)) != null)
                throw new UserCommandException($"{command.Email} is already in use");

            var user = await _userManager.FindByIdAsync(command.AggregateId.ToString());

            var emailChangeResult = await _userManager.SetEmailAsync(user, command.Email);

            if (!emailChangeResult.Succeeded)
                throw new UserCommandException($"Failed to chnage email for user: {command.AggregateId} {Environment.NewLine}Errors: {string.Join(Environment.NewLine, emailChangeResult.Errors.Select(e => $"code: {e.Code}, description: {e.Description}"))}");

            await _eventBus.PublishAsync(new UserEmailChanged(
                aggregateId: command.AggregateId,
                version: 1,
                email: command.Email
            ));
        }
    }
}
