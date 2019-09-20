using System;
using OpenEventSourcing.Events;

namespace SIO.Domain.User.Events
{
    public class UserEmailChanged : Event
    {
        public string Email { get; set; }
        public UserEmailChanged(Guid aggregateId, int version, string email) : base(aggregateId, version)
        {
            Email = email;
        }
    }
}
