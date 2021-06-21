using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserPasswordTokenGenerated : Event
    {
        public string Token { get; set; }

        public UserPasswordTokenGenerated(string subject, string token) : base(subject, 0)
        {
            Token = token;
        }
    }
}
