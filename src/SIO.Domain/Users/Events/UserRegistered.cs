﻿using SIO.Infrastructure.Events;

namespace SIO.Domain.Users.Events
{
    public class UserRegistered : Event
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ActivationToken { get; set; }

        public UserRegistered(string subject, int version, string email, string firstName, string lastName, string activationToken) : base(subject, version)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            ActivationToken = activationToken;
        }
    }
}
