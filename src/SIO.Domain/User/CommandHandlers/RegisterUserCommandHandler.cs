using System;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    internal class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBusPublisher _eventBusPublisher;

        public RegisterUserCommandHandler(UserManager<SIOUser> userManager, IEventBusPublisher eventBusPublisher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            _userManager = userManager;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task ExecuteAsync(RegisterUserCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    throw new EmailInUseException();
                }
                else
                {
                    await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    throw new UserNotVerifiedException();
                }
            }

            user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                UserName = command.Email,
            };

            var userResult = await _userManager.CreateAsync(user);

            if (!userResult.Succeeded)
            {
                throw new UserCreationException();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.UpdateAsync(user);

            var userRegisteredEvent = new UserRegistered(Guid.Parse(user.Id), Guid.NewGuid(), user.Id, user.Email, user.FirstName, user.LastName, token);
            userRegisteredEvent.UpdateFrom(command);
   
            await _eventBusPublisher.PublishAsync(userRegisteredEvent);
        }
    }
}
