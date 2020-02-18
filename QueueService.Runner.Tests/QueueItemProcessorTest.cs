using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueService.DAL;
using QueueService.Model;
using QueueService.Model.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Runner.Tests
{

    public class QueueItemProcessorPublic : QueueItemProcessor
    {
        public QueueItemProcessorPublic(
            IHttpClientFactory clientFactory, 
            ISqlConnectionFactory sqlConnectionFactory,
            IQueueItemRepository queueItemRepository, 
            IQueueWorkerRepository queueWorkerRepository, 
            IOptions<AppSettings> config, 
            ILogger<QueueItemProcessor> logger
            ) : base(clientFactory, sqlConnectionFactory, queueItemRepository, queueWorkerRepository, config, logger)
        {
        }

        public async Task BackgroundProcessingPublic(CancellationToken cancellationToken)
        {
            await base.BackgroundProcessing(cancellationToken);
        }
        public async Task ProcessItemPublic(QueueItem item, CancellationToken cancellationToken)
        {
            await base.ProcessItem(item, cancellationToken);
        }
    }

    [TestClass]
    [TestCategory("Unit")]
    public class QueueItemProcessorTest
    {
        private QueueItemProcessorPublic queueItemProcessor;
        private Mock<IHttpClientFactory> clientFactoryMock;
        private Mock<HttpClient> clientMock;
        private Mock<ISqlConnectionFactory> sqlConnectionFactoryMock;
        private Mock<IQueueItemRepository> queueItemRepositoryMock;
        private Mock<IQueueWorkerRepository> queueWorkerRepositoryMock;
        private Mock<ILogger<QueueItemProcessor>> loggerMock;
      
        [TestInitialize]
        public void Initialize()
        {
            clientFactoryMock = new Mock<IHttpClientFactory>();
            clientMock = new Mock<HttpClient>();
            sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
            queueItemRepositoryMock = new Mock<IQueueItemRepository>();
            queueWorkerRepositoryMock = new Mock<IQueueWorkerRepository>();
            loggerMock = new Mock<ILogger<QueueItemProcessor>>();

            queueItemProcessor = new QueueItemProcessorPublic(
                clientFactoryMock.Object,
                sqlConnectionFactoryMock.Object,
                queueItemRepositoryMock.Object, 
                queueWorkerRepositoryMock.Object, 
                Options.Create(new AppSettings {NoQueueItemsToProcessSleepTimeMS = 100}), 
                loggerMock.Object
                );
        }

        private QueueItem GenerateItem()
        {
            var queueItem = new QueueItem
            {
                QueueWorker = new QueueWorker
                {
                    Method = "POST",
                    Id = 1,
                    Enabled = true,
                    BatchSize = 1,
                    Endpoint = "http://localhost/",
                    Name = "1",
                    Priority = 1,
                    Retries = 1,
                    RetryDelay = 0,
                    RetryDelayMultiplier = 0
                },
                Id = 1,
                Payload = "somepayload",
                State = QueueItemState.New,
                QueueWorkerId = 1
            };

            return queueItem;
        }

        [TestMethod]
        public async Task ProcessItemTestSuccess()
        {

            var queueItem = GenerateItem();

            clientMock.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(
                r => r.Method == HttpMethod.Post
                && r.RequestUri.AbsoluteUri.Equals(queueItem.QueueWorker.Endpoint, StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            clientFactoryMock.Setup(c => c.CreateClient(It.IsAny<string>())).Returns(clientMock.Object);
 
            await queueItemProcessor.ProcessItemPublic(queueItem, CancellationToken.None);

            queueItemRepositoryMock.Verify(r => r.SetSuccess(queueItem.Id), Times.Once);
            queueItemRepositoryMock.Verify(r => r.SetFailed(queueItem.Id, It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [TestMethod]
        public async Task ProcessItemTestFailed()
        {
            var queueItem = GenerateItem();

            clientMock.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(
                r => r.Method == HttpMethod.Post
                && r.RequestUri.AbsoluteUri.Equals(queueItem.QueueWorker.Endpoint, StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound, ReasonPhrase = "Not Found"});

            clientFactoryMock.Setup(c => c.CreateClient(It.IsAny<string>())).Returns(clientMock.Object);
            queueItemRepositoryMock.Setup(r => r.SetFailed(queueItem.Id, It.IsAny<string>(), It.IsAny<DateTime?>())).ReturnsAsync(queueItem);


            await queueItemProcessor.ProcessItemPublic(queueItem, CancellationToken.None);

            queueItemRepositoryMock.Verify(r => r.SetSuccess(queueItem.Id), Times.Never);
            queueItemRepositoryMock.Verify(r => r.SetFailed(queueItem.Id, It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Once);
        }

        [TestMethod]
        public async Task BackgroundProcessingCancelled()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var task = queueItemProcessor.BackgroundProcessingPublic(cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                await Task.Delay(500);
                Assert.IsTrue(task.IsCompleted);
            }
        }

        [TestMethod]
        public async Task BackgroundProcessingGetZeroWorkers()
        {
            queueWorkerRepositoryMock.Setup(r => r.GetQueueWorkers(It.IsAny<SqlConnection>())).ReturnsAsync(new List<QueueWorker>());
            queueItemRepositoryMock.Setup(r => r.HasItems(It.IsAny<SqlConnection>())).ReturnsAsync(false);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = queueItemProcessor.BackgroundProcessingPublic(token);
                
                await Task.Delay(500);
                cancellationTokenSource.Cancel();
            }

            queueItemRepositoryMock.Verify(r => r.HasItems(It.IsAny<SqlConnection>()), Times.AtLeastOnce);
        }


        [TestMethod]
        public async Task BackgroundProcessingGetZeroItems()
        {
            var queueItem = GenerateItem();

            queueWorkerRepositoryMock.Setup(r => r.GetQueueWorkers(It.IsAny<SqlConnection>())).ReturnsAsync(new List<QueueWorker>() { queueItem.QueueWorker });
            queueItemRepositoryMock.Setup(r => r.HasItems(It.IsAny<SqlConnection>())).ReturnsAsync(false);
            queueItemRepositoryMock.Setup(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize)).ReturnsAsync(new List<QueueItem>());
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = queueItemProcessor.BackgroundProcessingPublic(token);

                await Task.Delay(500);
                cancellationTokenSource.Cancel();
            }

            queueItemRepositoryMock.Verify(r => r.HasItems(It.IsAny<SqlConnection>()), Times.AtLeastOnce);
            queueItemRepositoryMock.Verify(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize), Times.Once);

        }

        [TestMethod]
        public async Task BackgroundProcessingRetryTest()
        {
            var queueItem = GenerateItem();
            var queueItem2 = GenerateItem();
            queueItem2.Tries = 1;

            queueWorkerRepositoryMock.Setup(r => r.GetQueueWorkers(It.IsAny<SqlConnection>())).ReturnsAsync(new List<QueueWorker>() { queueItem.QueueWorker });
            queueItemRepositoryMock.Setup(r => r.HasItems(It.IsAny<SqlConnection>())).ReturnsAsync(false);
            queueItemRepositoryMock.SetupSequence(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize))
                .ReturnsAsync(new List<QueueItem>() { queueItem })
                .ReturnsAsync(new List<QueueItem>() { queueItem2 })
                .ReturnsAsync(new List<QueueItem>() {  });

            clientMock.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(
                r => r.Method == HttpMethod.Post
                && r.RequestUri.AbsoluteUri.Equals(queueItem.QueueWorker.Endpoint, StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound, ReasonPhrase = "Not Found" });

            clientFactoryMock.Setup(c => c.CreateClient(It.IsAny<string>())).Returns(clientMock.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = queueItemProcessor.BackgroundProcessingPublic(token);

                await Task.Delay(500);
                cancellationTokenSource.Cancel();
            }

            queueItemRepositoryMock.Verify(r => r.HasItems(It.IsAny<SqlConnection>()), Times.AtLeastOnce);
            queueItemRepositoryMock.Verify(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize), Times.Exactly(3));
            queueItemRepositoryMock.Verify(r => r.SetFailed(queueItem.Id, It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Exactly(2));
            queueItemRepositoryMock.Verify(r => r.SetSuccess(queueItem.Id), Times.Never);
        }

        [TestMethod]
        public async Task BackgroundProcessingSuccessTest()
        {
            var queueItem = GenerateItem();

            queueWorkerRepositoryMock.Setup(r => r.GetQueueWorkers(It.IsAny<SqlConnection>())).ReturnsAsync(new List<QueueWorker>() { queueItem.QueueWorker });
            queueItemRepositoryMock.Setup(r => r.HasItems(It.IsAny<SqlConnection>())).ReturnsAsync(false);
            queueItemRepositoryMock.SetupSequence(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize))
                .ReturnsAsync(new List<QueueItem>() { queueItem })
                .ReturnsAsync(new List<QueueItem>() { });

            clientMock.Setup(c => c.SendAsync(It.Is<HttpRequestMessage>(
                r => r.Method == HttpMethod.Post
                && r.RequestUri.AbsoluteUri.Equals(queueItem.QueueWorker.Endpoint, StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            clientFactoryMock.Setup(c => c.CreateClient(It.IsAny<string>())).Returns(clientMock.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = queueItemProcessor.BackgroundProcessingPublic(token);

                await Task.Delay(500);
                cancellationTokenSource.Cancel();
            }

            queueItemRepositoryMock.Verify(r => r.HasItems(It.IsAny<SqlConnection>()), Times.AtLeastOnce);
            queueItemRepositoryMock.Verify(r => r.GetQueueItems(It.IsAny<SqlConnection>(), queueItem.QueueWorker.Id, queueItem.QueueWorker.Retries, queueItem.QueueWorker.BatchSize), Times.Exactly(2));
            queueItemRepositoryMock.Verify(r => r.SetFailed(queueItem.Id, It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
            queueItemRepositoryMock.Verify(r => r.SetSuccess(queueItem.Id), Times.Once);
        }
    }
}
