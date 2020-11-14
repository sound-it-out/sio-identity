using System;
using System.Runtime.Serialization;

namespace SIO.Domain.Users.Commands
{
    public class UserIsLockedOutException : UserCommandException
    {
        public UserIsLockedOutException()
        {
        }

        public UserIsLockedOutException(string message) : base(message)
        {
        }

        public UserIsLockedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserIsLockedOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
