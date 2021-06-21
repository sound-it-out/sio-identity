using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Users.Commands
{
    public class RegisterUserCommand : Command
    {
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public RegisterUserCommand(string subject, CorrelationId? correlationId, string email, string firstName, string lastName) : base(subject, correlationId, 0, Actor.Unknown)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
