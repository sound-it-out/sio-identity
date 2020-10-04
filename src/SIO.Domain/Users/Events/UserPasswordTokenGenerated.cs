using System;
using System.Collections.Generic;
using System.Text;
using OpenEventSourcing.Events;

namespace SIO.Domain.Users.Events
{
    public class UserPasswordTokenGenerated : Event
    {
        public string Token { get; set; }
        public UserPasswordTokenGenerated(Guid aggregateId, Guid correlationId, string userId, string token) : base(aggregateId, 0)
        {
            CorrelationId = correlationId;
            UserId = userId;
            Token = token;
        }
    }
}
