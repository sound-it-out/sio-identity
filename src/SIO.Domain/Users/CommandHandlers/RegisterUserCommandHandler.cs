using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Aggregates;
using SIO.Domain.Users.Commands;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    internal class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IAggregateRepository _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(UserManager<SIOUser> userManager,
            IAggregateRepository aggregateRepository,
            IAggregateFactory aggregateFactory,
            ILogger<RegisterUserCommandHandler> logger)
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

        public async Task ExecuteAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(RegisterUserCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

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
                Id = command.Subject,
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

            var aggregate = _aggregateFactory.FromHistory<User, UserState>(Enumerable.Empty<IEvent>());

            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            aggregate.Register(
                subject: user.Id,
                email: user.Email,
                firstName: user.FirstName,
                lastName: user.LastName,
                activationToken: token
            );

            await _aggregateRepository.SaveAsync(aggregate, command, cancellationToken: cancellationToken);
        }
    }
}
