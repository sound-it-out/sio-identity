using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserVerified : Event
    {
        public UserVerified(string subject, int version) : base(subject, version)
        {
        }
    }
}
