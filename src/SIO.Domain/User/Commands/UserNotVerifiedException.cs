using System;
using System.Runtime.Serialization;

namespace SIO.Domain.User.Commands
{
    public class UserNotVerifiedException : UserCommandException
    {
        public UserNotVerifiedException()
        {
        }

        public UserNotVerifiedException(string message) : base(message)
        {
        }

        public UserNotVerifiedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserNotVerifiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
