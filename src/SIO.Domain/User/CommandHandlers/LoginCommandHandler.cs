using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    internal class LoginCommandHandler : ICommandHandler<LoginCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IEventBusPublisher _eventBusPublisher;

        public LoginCommandHandler(UserManager<SIOUser> userManager,
            SignInManager<SIOUser> signInManager,
            IEventBusPublisher eventBusPublisher)
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

        public async Task ExecuteAsync(LoginCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.AggregateId.ToString());

            if (user == null)
                throw new UserDoesntExistException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserIsLockedOutException();

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var userPasswordTokenGeneratedEvent = new UserPasswordTokenGenerated(command.AggregateId, command.CorrelationId, command.AggregateId.ToString(), token);
                userPasswordTokenGeneratedEvent.UpdateFrom(command);

                await _eventBusPublisher.PublishAsync(userPasswordTokenGeneratedEvent);

                await _userManager.UpdateAsync(user);

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

            await _signInManager.SignInAsync(user, false);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.UpdateAsync(user);

            var userLoggedInEvent = new UserLoggedIn(command.AggregateId, command.CorrelationId, command.AggregateId.ToString());
            userLoggedInEvent.UpdateFrom(command);

            await _eventBusPublisher.PublishAsync(userLoggedInEvent);            
        }
    }
}
