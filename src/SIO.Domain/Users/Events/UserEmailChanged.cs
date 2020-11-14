using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.Users.Events
{
    public class UserEmailChanged : Event
    {
        public string Email { get; set; }
        public UserEmailChanged(Guid aggregateId, Guid correlationId, int version, string userId, string email) : base(aggregateId, version)
        {
            Email = email;
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
