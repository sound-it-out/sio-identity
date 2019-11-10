using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserLoggedOut : Event
    {
        public UserLoggedOut(Guid aggregateId, Guid correlationId, int version, string userId) : base(aggregateId, version)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
