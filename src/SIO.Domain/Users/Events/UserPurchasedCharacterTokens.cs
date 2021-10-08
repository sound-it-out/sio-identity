using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserPurchasedCharacterTokens : Event
    {
        public long CharacterTokens { get; set; }

        public UserPurchasedCharacterTokens(string subject, long characterTokens) : base(subject, 0)
        {
            CharacterTokens = characterTokens;
        }
    }
}
