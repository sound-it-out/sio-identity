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

        public UserRegistered(Guid aggregateId, Guid correlationId, string userId, string email, string firstName, string lastName, string activationToken) : base(aggregateId, 0)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            ActivationToken = activationToken;
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
