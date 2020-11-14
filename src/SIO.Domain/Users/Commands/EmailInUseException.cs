using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class EmailInUseException : UserCommandException
    {
        public EmailInUseException()
        {
        }

        public EmailInUseException(string message) : base(message)
        {
        }

        public EmailInUseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EmailInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
