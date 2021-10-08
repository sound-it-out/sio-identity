using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Aggregates;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IAggregateRepository _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(UserManager<SIOUser> userManager,
            IAggregateRepository aggregateRepository,
            IAggregateFactory aggregateFactory,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (aggregateRepository == null)
                throw new ArgumentNullException(nameof(aggregateRepository));
            if (aggregateFactory == null)
                throw new ArgumentNullException(nameof(aggregateFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _userManager = userManager;
            _aggregateRepository = aggregateRepository;
            _aggregateFactory = aggregateFactory;
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

                var aggregate = await _aggregateRepository.GetAsync<User, UserState>(command.Subject, cancellationToken);
                var expectedVersion = aggregate.Version;

                if (aggregate == null)
                    throw new ArgumentNullException(nameof(aggregate));

                aggregate.RequestToken(token);

                await _aggregateRepository.SaveAsync(aggregate, command, expectedVersion, cancellationToken);              
            }
            else
            {
                throw new UserDoesntExistException();
            }
        }
    }
}
