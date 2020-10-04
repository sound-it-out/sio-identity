using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.Users.Events
{
    public class UserLoggedOut : Event
    {
        public UserLoggedOut(Guid aggregateId, Guid correlationId, string userId) : base(aggregateId, 0)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
