﻿using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserVerified : Event
    {
        public UserVerified(Guid aggregateId, Guid correlationId, int version, string userId) : base(aggregateId, version)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}
