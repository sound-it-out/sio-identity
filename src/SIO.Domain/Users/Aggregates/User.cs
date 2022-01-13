using SIO.Infrastructure.Domain;
using SIO.IntegrationEvents.Users;

namespace SIO.Domain.Users.Aggregates
{
    public sealed class User : Aggregate<UserState>
    {
        public User(UserState state) : base(state)
        {
            Handles<UserRegistered>(Handle);
            Handles<UserPasswordTokenGenerated>(Handle);
            Handles<UserVerified>(Handle);
            Handles<UserLoggedIn>(Handle);
            Handles<UserLoggedOut>(Handle);
        }

        public override UserState GetState() => new UserState(_state);

        public void Register(string subject, string email, string firstName, string lastName, string activationToken)
        {
            Apply(new UserRegistered(
                subject: subject,
                version: Version +1
            )
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ActivationToken = activationToken
            });
        }

        public void RequestToken(string token)
        {
            Apply(new UserPasswordTokenGenerated(
                subject: Id,
                version: Version + 1)
            {
                Token = token
            });
        }

        public void Verify()
        {
            Apply(new UserVerified(Id, Version + 1));
        }

        public void Login()
        {
            Apply(new UserLoggedIn(Id, Version + 1));
        }

        public void Logout()
        {
            Apply(new UserLoggedOut(Id, Version + 1));
        }

        private void Handle(UserRegistered @event)
        {
            Id = @event.Subject;
            _state.Email = @event.Email;
            _state.FirstName = @event.FirstName;
            _state.LastName = @event.LastName;
            _state.UserToken = @event.ActivationToken;
            _state.LoggedIn = false;
            _state.IsVerified = false;
            Version = @event.Version;
        }

        private void Handle(UserPasswordTokenGenerated @event)
        {
            _state.UserToken = @event.Token;
            Version = @event.Version;
        }

        private void Handle(UserVerified @event)
        {
            _state.IsVerified = true;
            Version = @event.Version;
        }

        private void Handle(UserLoggedIn @event)
        {
            _state.LoggedIn = true;
            Version = @event.Version;
        }

        private void Handle(UserLoggedOut @event)
        {
            _state.LoggedIn = false;
            Version = @event.Version;
        }
    }
}
