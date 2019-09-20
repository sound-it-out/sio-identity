using System;
using OpenEventSourcing.Commands;

namespace SIO.Domain.User.Commands
{
    internal class UserCommandException : CommandException
    {
        public UserCommandException(string message) : base(message)
        {
        }

        public UserCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
