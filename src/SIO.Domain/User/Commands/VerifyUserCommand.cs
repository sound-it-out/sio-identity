using System;
using System.Collections.Generic;
using System.Text;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class VerifyUserCommand : Command
    {
        public string Token { get; set; }
        public string Password { get; set; }

        public VerifyUserCommand(Guid aggregateId, Guid correlationId, int version, string userId, string token, string password) : base(aggregateId, correlationId, version, userId)
        {
            Token = token;
            Password = password;
        }
    }
}
