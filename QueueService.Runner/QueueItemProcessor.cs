using HMACAuthentication.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.DAL;
using QueueService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Runner
{
    public class QueueItemProcessor : BackgroundService
    {
        private readonly AppSettings config;
        private readonly ILogger<QueueItemProcessor> logger;
        private readonly IQueueItemRepository queueItemRepository;
        private readonly IQueueWorkerRepository queueWorkerRepository;
        private readonly IHttpClientFactory clientFactory;
        private readonly ISqlConnectionFactory sqlConnectionFactory;

        public QueueItemProcessor(
            IHttpClientFactory clientFactory,
            ISqlConnectionFactory sqlConnectionFactory,
            IQueueItemRepository queueItemRepository,
            IQueueWorkerRepository queueWorkerRepository,
            IOptions<AppSettings> config,
            ILogger<QueueItemProcessor> logger
        )
        {
            this.config = config.Value;
            this.logger = logger;
            this.queueItemRepository = queueItemRepository;
            this.queueWorkerRepository = queueWorkerRepository;
            this.clientFactory = clientFactory;
            this.sqlConnectionFactory = sqlConnectionFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueProcessor Service is starting.");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("QueueProcessor Service is stopping.");
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await BackgroundProcessing(cancellationToken);
        }
        protected async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            bool hasItems = true;
            while (!cancellationToken.IsCancellationRequested)
            {               
                try
                {
                    var allItems = new List<QueueItem>();

                    using (var connection = sqlConnectionFactory.GetConnection())
                    {
                        await sqlConnectionFactory.OpenAsync(connection, cancellationToken);
                        if (!hasItems)
                        {
                            hasItems = await queueItemRepository.HasItems(connection);
                            if (!hasItems)
                            {
                                logger.LogInformation($"No items to process, going to sleep for {config.NoQueueItemsToProcessSleepTimeMS} milliseconds");
                                await Task.Delay(config.NoQueueItemsToProcessSleepTimeMS, cancellationToken);
                                sqlConnectionFactory.Close(connection);
                                continue;
                            }
                        }

                        var workers = await queueWorkerRepository.GetQueueWorkers(connection);
                        foreach (var worker in workers.Where(w => w.Enabled).OrderByDescending(w => w.Priority))
                        {
                            var items = await queueItemRepository.GetQueueItems(connection, worker.Id, worker.Retries, worker.BatchSize);
                            items.ForEach(i => i.QueueWorker = worker);
                            allItems.AddRange(items);
                            if(allItems.Count > config.GlobalBatchSizeLimit)
                            {
                                break;
                            }
                        }
                        sqlConnectionFactory.Close(connection);
                    }

                    if (!allItems.Any())
                    {
                        logger.LogInformation($"No items to process, going to sleep for {config.NoQueueItemsToProcessSleepTimeMS} milliseconds");
                        hasItems = false;
                        await Task.Delay(config.NoQueueItemsToProcessSleepTimeMS, cancellationToken);
                        continue;
                    }

                    var allTasks = new List<Task>();
                    foreach(var item in allItems.OrderByDescending(i => i.QueueWorker.Priority).ThenBy(i => i.Id))
                    {
                        allTasks.Add(ProcessItem(item, cancellationToken));
                    }
                    await Task.WhenAll(allTasks);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing queue.");
                    await Task.Delay(config.NoQueueItemsToProcessSleepTimeMS, cancellationToken);
                }
            }
            logger.LogInformation("QueueProcessor Service is cancelled.");
        }

        protected async Task ProcessItem(QueueItem item, CancellationToken cancellationToken)
        {
            var nextRun = DateTime.UtcNow.AddSeconds(item.QueueWorker.RetryDelay + item.QueueWorker.RetryDelay * item.Tries * item.QueueWorker.RetryDelayMultiplier);
            var startTime = DateTime.UtcNow;

            try
            {
                var request = new HttpRequestMessage(new HttpMethod(item.QueueWorker.Method), item.QueueWorker.Endpoint);
                if (!string.IsNullOrEmpty(item.Payload))
                {
                    if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                    {
                        request.Content = new StringContent(item.Payload, Encoding.UTF8, "application/json");
                    }
                    else if (request.Method == HttpMethod.Get)
                    {
                        UriBuilder uriBuilder = new UriBuilder(request.RequestUri)
                        {
                            Query = item.Payload
                        };
                        request.RequestUri = uriBuilder.Uri;
                    }
                    else
                    {
                        throw new NotSupportedException($"{request.Method} is not supported.");
                    }
                }

                if (!string.IsNullOrEmpty(item.QueueWorker.ApiKey))
                {
                    SignatureHelper.SetHmacHeaders(request, item.QueueWorker.Name, item.QueueWorker.ApiKey, item.Payload);
                }

                var client = clientFactory.CreateClient(item.QueueWorker.Name);
                client.Timeout = new TimeSpan(0, 0, item.QueueWorker.MaxProcessingTime == 0 ? 30 : item.QueueWorker.MaxProcessingTime);

                var response = await client.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await queueItemRepository.SetSuccess(item.Id);
                }
                else
                {
                    throw new Exception($"Error processing queue item {item.Id}. Endpoint returned status code: {response.StatusCode} {response.ReasonPhrase}");                    
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Error processing queue item {item.Id}");
                var resultItem = await queueItemRepository.SetFailed(item.Id, ex.ToString(), nextRun);

                //If error state is set it will not retry again so we send out warning
                if (resultItem.State == QueueItemState.Error)
                {
                    logger.LogWarning($"Queue item set to error state and will not be retried again. Queue item Id: {resultItem.Id}");
                }
            }
            finally
            {
                var timeTaken = DateTime.UtcNow.Subtract(startTime);
                logger.LogInformation($"Finished item {item.Id}. Total Time: {timeTaken}");
            }
        }
    }
}
