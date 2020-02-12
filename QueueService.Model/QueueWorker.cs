namespace QueueService.Model
{
    public class QueueWorker
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public short Priority { get; set; }
        public short Retries { get; set; }
        public short MaxProcessingTime { get; set; }
        public short BatchSize { get; set; }
        public bool Enabled { get; set; }
        public short RetryDelay { get; set; }
        public short RetryDelayMultiplier { get; set; }
        public string ApiKey { get; set; }
    }
}
