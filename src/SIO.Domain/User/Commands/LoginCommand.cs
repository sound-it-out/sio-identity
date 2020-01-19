using System;
using Newtonsoft.Json;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    public class LoginCommand : Command
    {
        [JsonIgnore]
        public string Password { get; set; }

        public LoginCommand(Guid aggregateId, Guid correlationId, int version, string userId, string password) : base(aggregateId, correlationId, version, userId)
        {
            Password = password;
        }
    }
}
