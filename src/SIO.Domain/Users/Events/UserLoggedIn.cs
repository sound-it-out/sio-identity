using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserLoggedIn : Event
    {
        public UserLoggedIn(string subject, int version) : base(subject, version)
        {
        }
    }
}
