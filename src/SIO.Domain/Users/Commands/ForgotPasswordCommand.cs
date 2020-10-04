using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.Users.Commands
{
    public class ForgotPasswordCommand : Command
    {
        public ForgotPasswordCommand(Guid aggregateId, Guid correlationId, int version, string userId) : base(aggregateId, correlationId, version, userId)
        {
        }
    }
}
