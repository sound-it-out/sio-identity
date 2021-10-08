using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserLoggedOut : Event
    {
        public UserLoggedOut(string subject, int version) : base(subject, version)
        {
        }
    }
}
