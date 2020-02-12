using System;

namespace QueueService.Model
{
    public enum QueueItemState { 
        New = 0,
        Processing = 1,
        Finished = 2,
        Error = 3
    }

    public class QueueItem
    {
        public long Id { get; set; }
        public long QueueWorkerId { get; set; }
        public QueueItemState State { get; set; }
        public DateTime CreateDate { get; set; }
        public string Payload { get; set; }
        public short Tries { get; set; }
        public DateTime? ExecuteTimeStart { get; set; }
        public DateTime? ExecuteTimeEnd { get; set; }
        public DateTime? ExecuteTimeNext { get; set; }
        public string ExecuteResult { get; set; }
        public QueueWorker QueueWorker { get; set; }
        public string UniqueKey { get; set; }
    }
}
