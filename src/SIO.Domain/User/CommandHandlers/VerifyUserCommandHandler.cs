using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    public class VerifyUserCommandHandler : ICommandHandler<VerifyUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IEventBusPublisher _eventBusPublisher;

        public VerifyUserCommandHandler(UserManager<SIOUser> userManager, SignInManager<SIOUser> signInManager, IEventBusPublisher eventBusPublisher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));

            _userManager = userManager;
            _signInManager = signInManager;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task ExecuteAsync(VerifyUserCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.AggregateId.ToString());

            var confirmationResult = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(Convert.FromBase64String(command.Token)));

            if (!confirmationResult.Succeeded)
            {
                throw new EmailConfirmationException(confirmationResult.Errors.First().Description);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, command.Password);

            if (!addPasswordResult.Succeeded)
            {
                throw new PasswordCreationException(addPasswordResult.Errors.First().Description);
            }

            await _userManager.UpdateAsync(user);

            var userVerifiedEvent = new UserVerified(command.AggregateId, command.CorrelationId, command.AggregateId.ToString());
            userVerifiedEvent.UpdateFrom(command);

            await _eventBusPublisher.PublishAsync(userVerifiedEvent);

            await _signInManager.SignInAsync(user, false);

            var userLoggedInEvent = new UserLoggedIn(command.AggregateId, command.CorrelationId, command.AggregateId.ToString());
            userLoggedInEvent.UpdateFrom(command);

            await _eventBusPublisher.PublishAsync(userLoggedInEvent);

            
        }
    }
}
