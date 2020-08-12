using System;
using System.Runtime.Serialization;

namespace SIO.Domain.User.Commands
{
    public class PasswordCreationException : UserCommandException
    {
        public PasswordCreationException()
        {
        }

        public PasswordCreationException(string message) : base(message)
        {
        }

        public PasswordCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PasswordCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
