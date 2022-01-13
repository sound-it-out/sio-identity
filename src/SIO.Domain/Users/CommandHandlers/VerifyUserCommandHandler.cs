using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIO.Domain.Users.Aggregates;
using SIO.Domain.Users.Commands;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Events;
using SIO.Migrations;

namespace SIO.Domain.Users.CommandHandlers
{
    public class VerifyUserCommandHandler : ICommandHandler<VerifyUserCommand>
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IAggregateRepository<SIOStoreDbContext> _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ILogger<VerifyUserCommandHandler> _logger;

        public VerifyUserCommandHandler(UserManager<SIOUser> userManager,
            SignInManager<SIOUser> signInManager,
            IAggregateRepository<SIOStoreDbContext> aggregateRepository,
            IAggregateFactory aggregateFactory,
            ILogger<VerifyUserCommandHandler> logger)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (aggregateRepository == null)
                throw new ArgumentNullException(nameof(aggregateRepository));
            if (aggregateFactory == null)
                throw new ArgumentNullException(nameof(aggregateFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _userManager = userManager;
            _signInManager = signInManager;
            _aggregateRepository = aggregateRepository;
            _aggregateFactory = aggregateFactory;
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

            var aggregate = await _aggregateRepository.GetAsync<User, UserState>(command.Subject, cancellationToken);
            var expectedVersion = aggregate.Version;

            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            var streamId = StreamId.New();

            await _userManager.UpdateAsync(user);
            aggregate.Verify();

            await _signInManager.SignInAsync(user, false);
            aggregate.Login();

            await _aggregateRepository.SaveAsync(aggregate, command, expectedVersion, cancellationToken);
        }
    }
}
