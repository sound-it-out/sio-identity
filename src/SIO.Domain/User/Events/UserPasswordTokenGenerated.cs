using System;
using System.Collections.Generic;
using System.Text;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserPasswordTokenGenerated : Event
    {
        public string Token { get; set; }
        public UserPasswordTokenGenerated(Guid aggregateId, Guid correlationId, int version, string userId, string token) : base(aggregateId, version)
        {
            CorrelationId = correlationId;
            UserId = userId;
            Token = token;
        }
    }
}
