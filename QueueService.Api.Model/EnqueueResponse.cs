using System;
using System.Collections.Generic;
using System.Text;

namespace QueueService.Api.Model
{
    public class EnqueueResponse : ApiResponse
    {
        public string State { get; set; }
        public DateTime CreateDate { get; set; }
        public string UniqueKey { get; set; }
    }
}
