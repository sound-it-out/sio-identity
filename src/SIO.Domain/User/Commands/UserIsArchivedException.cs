using System;
using System.Runtime.Serialization;

namespace SIO.Domain.User.Commands
{
    public class UserIsArchivedException : UserCommandException
    {
        public UserIsArchivedException()
        {
        }

        public UserIsArchivedException(string message) : base(message)
        {
        }

        public UserIsArchivedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserIsArchivedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
