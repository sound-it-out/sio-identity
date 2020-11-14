using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.Users.Commands
{
    public class LogoutCommand : Command
    {
        public string LogoutId { get; set; }

        public LogoutCommand(Guid aggregateId, Guid correlationId, int version, string userId, string logoutId) : base(aggregateId, correlationId, version, userId)
        {
            LogoutId = logoutId;
        }
    }
}
