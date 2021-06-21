using System;
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
    internal class LoginCommandHandler : ICommandHandler<LoginCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IEventManager _eventManager;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(UserManager<SIOUser> userManager,
            SignInManager<SIOUser> signInManager,
            IEventManager eventManager,
            ILogger<LoginCommandHandler> logger)
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

        public async Task ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(LoginCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var user = await _userManager.FindByIdAsync(command.Subject);

            if (user == null)
                throw new UserDoesntExistException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserIsLockedOutException();

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.UpdateAsync(user);
                await _eventManager.ProcessAsync(StreamId.New(), new UserPasswordTokenGenerated(command.Subject, token));                

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
            await _eventManager.ProcessAsync(StreamId.New(), new UserLoggedIn(command.Subject));            
        }
    }
}
