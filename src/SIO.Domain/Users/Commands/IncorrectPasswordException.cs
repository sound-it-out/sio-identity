using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class IncorrectPasswordException : UserCommandException
    {
        public IncorrectPasswordException()
        {
        }

        public IncorrectPasswordException(string message) : base(message)
        {
        }

        public IncorrectPasswordException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IncorrectPasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
