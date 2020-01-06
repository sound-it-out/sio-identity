using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBusPublisher _eventBusPublisher;

        public ForgotPasswordCommandHandler(UserManager<SIOUser> userManager, IEventBusPublisher eventBusPublisher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));

            _userManager = userManager;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task ExecuteAsync(ForgotPasswordCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.AggregateId.ToString());

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    throw new UserNotVerifiedException();
                }

                if (user.IsArchived)
                {
                    throw new UserIsArchivedException();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.UpdateAsync(user);

                var userPasswordTokenGeneratedEvent = new UserPasswordTokenGenerated(command.AggregateId, command.CorrelationId, command.AggregateId.ToString(), token);
                userPasswordTokenGeneratedEvent.UpdateFrom(command);

                await _eventBusPublisher.PublishAsync(userPasswordTokenGeneratedEvent);                
            }
            else
            {
                throw new UserDoesntExistException();
            }
        }
    }
}
