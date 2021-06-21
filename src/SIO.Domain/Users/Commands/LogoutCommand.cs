using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Users.Commands
{
    public class LogoutCommand : Command
    {
        public string LogoutId { get; }

        public LogoutCommand(string subject, CorrelationId? correlationId, string logoutId) : base(subject, correlationId, 0, Actor.Unknown)
        {
            LogoutId = logoutId;
        }
    }
}
