using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class UserCommandException : Exception
    {
        public UserCommandException()
        {
        }

        public UserCommandException(string message) : base(message)
        {
        }

        public UserCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
