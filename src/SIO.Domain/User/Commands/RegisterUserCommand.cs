using System;
using System.Collections.Generic;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class RegisterUserCommand : Command
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public IEnumerable<string> Roles { get; }

        public RegisterUserCommand(Guid aggregateId, Guid correlationId, string firstName, string lastName, string email, IEnumerable<string> roles, int version, string userId) : base(aggregateId, correlationId, version, userId)
        {
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentNullException(nameof(firstName));
            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentNullException(nameof(lastName));
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
            if (roles == null)
                throw new ArgumentNullException(nameof(firstName));

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Roles = roles;
        }
    }
}
