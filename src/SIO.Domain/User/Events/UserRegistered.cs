using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserRegistered : Event
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ActivationToken { get; }

        public UserRegistered(Guid aggregateId, string email, string firstName, string lastName, string activationToken) : base(aggregateId, 1)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            ActivationToken = activationToken;
        }
    }
}
