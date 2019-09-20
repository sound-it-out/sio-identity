using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Migrations;

namespace SIO.Domain.User.CommandHandlers
{
    internal class RegisterUserCommandCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventBus _eventBus;

        public RegisterUserCommandCommandHandler(UserManager<SIOUser> userManager, IEventBus eventBus)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (eventBus == null)
                throw new ArgumentNullException(nameof(eventBus));

            _userManager = userManager;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(RegisterUserCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if ((await _userManager.FindByEmailAsync(command.Email)) != null)
                throw new UserCommandException($"{command.Email} is already in use");

            var user = new SIOUser
            {
                Id = command.AggregateId.ToString(),
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                UserName = command.Email,
            };

            var userResult = await _userManager.CreateAsync(user);

            if (!userResult.Succeeded)
                throw new UserCommandException($"Failed to register user: {command.AggregateId} {Environment.NewLine}Errors: {string.Join(Environment.NewLine, userResult.Errors.Select(e => $"code: {e.Code}, description: {e.Description}"))}");

            var claimResult = await _userManager.AddClaimsAsync(user, command.Roles.Select(r => new Claim(JwtClaimTypes.Role, r)));

            if (!claimResult.Succeeded)
                throw new UserCommandException($"Failed to add roles to user: {command.AggregateId} {Environment.NewLine}Errors: {string.Join(Environment.NewLine, claimResult.Errors.Select(e => $"code: {e.Code}, description: {e.Description}"))}");

            await _eventBus.PublishAsync(new UserRegistered(
                aggregateId: command.AggregateId,
                email: command.Email,
                firstName: command.FirstName,
                lastName: command.LastName,
                activationToken: await _userManager.GenerateEmailConfirmationTokenAsync(user)
            ));
        }
    }
}
