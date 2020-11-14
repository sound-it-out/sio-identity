using System;
using Newtonsoft.Json;
using OpenEventSourcing.Commands;

namespace SIO.Domain.Users.Commands
{
    public class VerifyUserCommand : Command
    {
        public string Token { get; set; }
        [JsonIgnore]
        public string Password { get; set; }

        public VerifyUserCommand(Guid aggregateId, Guid correlationId, int version, string userId, string token, string password) : base(aggregateId, correlationId, version, userId)
        {
            Token = token;
            Password = password;
        }
    }
}
