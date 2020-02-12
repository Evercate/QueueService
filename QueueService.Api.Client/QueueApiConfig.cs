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
    }
}
