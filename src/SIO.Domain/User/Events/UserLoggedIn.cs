using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserLoggedIn : Event
    {
        public UserLoggedIn(Guid aggregateId, Guid correlationId, int version, string userId) : base(aggregateId, version)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
