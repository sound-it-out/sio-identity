using System;
using System.Runtime.Serialization;

namespace SIO.Domain.User.Commands
{
    public class EmailConfirmationException : UserCommandException
    {
        public EmailConfirmationException()
        {
        }

        public EmailConfirmationException(string message) : base(message)
        {
        }

        public EmailConfirmationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EmailConfirmationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
