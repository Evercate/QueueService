using System;

namespace QueueService.Api.Client.Model
{
    public class EnqueueResponse
    {
        public string State { get; set; }
        public DateTime CreateDate { get; set; }
        public string UniqueKey { get; set; }
    }
}
