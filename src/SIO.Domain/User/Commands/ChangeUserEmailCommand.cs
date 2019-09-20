using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class ChangeUserEmailCommand : Command
    {
        public string Email { get; }
        public ChangeUserEmailCommand(Guid aggregateId, Guid correlationId, string email, int version, string userId) : base(aggregateId, correlationId, version, userId)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            Email = email;
        }
    }
}
