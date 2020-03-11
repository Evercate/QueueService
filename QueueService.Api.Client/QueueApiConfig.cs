using System;
using System.Collections.Generic;
using System.Text;

namespace QueueService.Api.Client
{
    public class QueueApiConfig
    {
        /// <summary>
        /// The Queue api can have multiple key/secret pairs. This key/id must be match the given secret on the recieving end
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The secret must match the key on the recieving end (the queue api)
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Number of retries if call to queue api fails
        /// </summary>
        public int Retries { get; set; } = 3;

        /// <summary>
        /// The delay in seconds between retries
        /// </summary>
        public int RetryDelay { get; set; } = 2;
    }
}
