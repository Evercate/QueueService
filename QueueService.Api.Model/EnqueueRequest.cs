using System;
using System.Collections.Generic;
using System.Text;

namespace QueueService.Api.Model
{
    public class EnqueueRequest
    {
        /// <summary>
        /// The name of the queue/worker which will decide where the message is sent
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The payload that will be sent with the request
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Optional: Unique key, if key already exists it will not be added again. Useful to avoide duplication
        /// </summary>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Optional: If not set will execute as soon as possible. If set execution will happen as soon after given time (UTC) as possible. 
        /// </summary>
        public DateTime? ExecuteOn { get; set; }
    }
}
