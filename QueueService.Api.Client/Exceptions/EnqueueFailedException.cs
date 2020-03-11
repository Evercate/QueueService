using System;

namespace QueueService.Api.Client.Exceptions
{
    public class EnqueueFailedException : Exception
    {
        public EnqueueFailedException()
        {
        }

        public EnqueueFailedException(string message)
            : base(message)
        {
        }

        public EnqueueFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
