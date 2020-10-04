using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    internal class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventPublisher _eventPublisher;

        public RegisterUserCommandHandler(UserManager<SIOUser> userManager, IEventPublisher eventPublisher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            _userManager = userManager;
            _eventPublisher = eventPublisher;
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
            try
            {
                await _eventPublisher.PublishAsync(userRegisteredEvent);
            }            
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
