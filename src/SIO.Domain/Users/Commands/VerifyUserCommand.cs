using Newtonsoft.Json;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Users.Commands
{
    public class VerifyUserCommand : Command
    {
        public string Token { get; set; }
        [JsonIgnore]
        public string Password { get; set; }

        public VerifyUserCommand(string subject, CorrelationId? correlationId, string token, string password) : base(subject, correlationId, 0, Actor.Unknown)
        {
            Token = token;
            Password = password;
        }
    }
}
