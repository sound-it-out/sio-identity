using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    public class VerifyUserCommandHandler : ICommandHandler<VerifyUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IEventManager _eventManager;
        private readonly ILogger<VerifyUserCommandHandler> _logger;

        public VerifyUserCommandHandler(UserManager<SIOUser> userManager,
            SignInManager<SIOUser> signInManager,
            IEventManager eventManager,
            ILogger<VerifyUserCommandHandler> logger)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (eventManager == null)
                throw new ArgumentNullException(nameof(eventManager));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _userManager = userManager;
            _signInManager = signInManager;
            _eventManager = eventManager;
            _logger = logger;
        }

        public async Task ExecuteAsync(VerifyUserCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(RegisterUserCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var user = await _userManager.FindByIdAsync(command.Subject);

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

            var streamId = StreamId.New();

            await _userManager.UpdateAsync(user);
            await _eventManager.ProcessAsync(streamId, new UserVerified(command.Subject));
            await _signInManager.SignInAsync(user, false);
            await _eventManager.ProcessAsync(streamId, new UserLoggedIn(command.Subject));
        }
    }
}
