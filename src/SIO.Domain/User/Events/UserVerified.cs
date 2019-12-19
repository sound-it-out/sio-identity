using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserVerified : Event
    {
        public UserVerified(Guid aggregateId, Guid correlationId, string userId) : base(aggregateId, 0)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
