using System;
using SIO.Infrastructure.Domain;

namespace SIO.Domain.Users.Aggregates
{
    public sealed class UserState : IAggregateState
    {
        public bool LoggedIn { get; set; }
        public bool IsVerified { get; set; }
        public string UserToken { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public UserState() { }

        public UserState(UserState state)
        {
            if(state == null)
                throw new ArgumentNullException(nameof(state));

            LoggedIn = state.LoggedIn;
            IsVerified = state.IsVerified;
            UserToken = state.UserToken;
            Email = state.Email;
            FirstName = state.FirstName;
            LastName = state.LastName;
        }
    }
}
