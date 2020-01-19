using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class RegisterUserCommand : Command
    {
        public RegisterUserCommand(Guid aggregateId, Guid correlationId, int version, string userId, string email, string firstName, string lastName) : base(aggregateId, correlationId, version, userId)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
