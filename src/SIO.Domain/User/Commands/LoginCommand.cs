using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class LoginCommand : Command
    {
        public string Password { get; set; }

        public LoginCommand(Guid aggregateId, Guid correlationId, int version, string userId, string password) : base(aggregateId, correlationId, version, userId)
        {
            Password = password;
        }
    }
}
