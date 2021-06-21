using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserEmailChanged : Event
    {
        public string Email { get; set; }

        public UserEmailChanged(string subject, int version) : base(subject, version)
        {
        }
    }
}
