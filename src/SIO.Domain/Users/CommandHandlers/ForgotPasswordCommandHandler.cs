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
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IEventManager _eventManager;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(UserManager<SIOUser> userManager, 
            IEventManager eventManager,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (eventManager == null)
                throw new ArgumentNullException(nameof(eventManager));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _userManager = userManager;
            _eventManager = eventManager;
            _logger = logger;
        }

        public async Task ExecuteAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(ForgotPasswordCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var user = await _userManager.FindByIdAsync(command.Subject);

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
                await _eventManager.ProcessAsync(StreamId.New(), new UserPasswordTokenGenerated(command.Subject, token));                
            }
            else
            {
                throw new UserDoesntExistException();
            }
        }
    }
}
