using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class UserCreationException : UserCommandException
    {
        public UserCreationException()
        {
        }

        public UserCreationException(string message) : base(message)
        {
        }

        public UserCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
