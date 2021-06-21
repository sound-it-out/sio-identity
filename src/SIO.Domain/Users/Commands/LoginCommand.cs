using Newtonsoft.Json;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Users.Commands
{
    public class LoginCommand : Command
    {
        [JsonIgnore]
        public string Password { get; set; }

        public LoginCommand(string subject, CorrelationId? correlationId, string password) : base(subject, correlationId, 0, Actor.Unknown)
        {
            Password = password;
        }
    }
}
