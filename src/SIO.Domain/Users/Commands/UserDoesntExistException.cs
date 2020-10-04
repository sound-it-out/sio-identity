using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class UserDoesntExistException : UserCommandException
    {
        public UserDoesntExistException()
        {
        }

        public UserDoesntExistException(string message) : base(message)
        {
        }

        public UserDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
