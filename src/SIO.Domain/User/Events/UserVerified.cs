using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserVerified : Event
    {
        public UserVerified(Guid aggregateId, int version) : base(aggregateId, version)
        {
        }
    }
}
