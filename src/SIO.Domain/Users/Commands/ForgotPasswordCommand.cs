using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Users.Commands
{
    public class ForgotPasswordCommand : Command
    {
        public ForgotPasswordCommand(string subject, CorrelationId? correlationId) : base(subject, correlationId, 0, Actor.Unknown)
        {
        }
    }
}
