using QueueService.Model.Settings;

namespace QueueService.Runner
{
    public class AppSettings
    {
        public int NoQueueItemsToProcessSleepTimeMS { get; set; }
        public int UnlockStuckItemsSleepTimeMS { get; set; }
        public int ArchiveQueueItemsSleepTimeMS { get; set; }
        public int ArchiveQueueItemsAgeDays { get; set; }
        public int GlobalBatchSizeLimit { get; set; }
    }
}
