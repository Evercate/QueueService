using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.DAL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Runner
{
    public class QueueItemUnlocker : BackgroundService
    {
        private readonly AppSettings config;
        private readonly ILogger<QueueItemUnlocker> logger;
        private readonly IQueueItemRepository queueItemRepository;

        public QueueItemUnlocker(IQueueItemRepository queueItemRepository, IOptions<AppSettings> config, ILogger<QueueItemUnlocker> logger)
        {
            this.config = config.Value;
            this.logger = logger;
            this.queueItemRepository = queueItemRepository;

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueItemUnlocker Service is starting.");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueItemUnlocker Service is stopping.");

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await BackgroundProcessing(cancellationToken);
        }

        private async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Unlocking stuck queue items.");
                    await queueItemRepository.UnlockStuckQueueItems();
                    logger.LogInformation($"QueueItemUnlocker Service is going to sleep for {config.UnlockStuckItemsSleepTimeMS} milliseconds.");
                    await Task.Delay(config.UnlockStuckItemsSleepTimeMS, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error unlocking queue items.");
                    await Task.Delay(config.ArchiveQueueItemsSleepTimeMS, cancellationToken);
                }
            }
            logger.LogInformation("QueueItemUnlocker Service is cancelled.");
        }

    }
}
