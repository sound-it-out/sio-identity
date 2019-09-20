using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class VerifyUserCommand : Command
    {
        public string Token { get; }
        public VerifyUserCommand(Guid aggregateId, Guid correlationId, string token, int version, string userId) : base(aggregateId, correlationId, version, userId)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            Token = token;
        }
    }
}
