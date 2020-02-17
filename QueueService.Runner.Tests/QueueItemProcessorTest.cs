using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueService.DAL;
using QueueService.Model;
using QueueService.Model.Settings;
using System;
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
                Options.Create(new AppSettings { }), 
                loggerMock.Object
                );
        }

        [TestMethod]
        public async Task ProcessItemTest()
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
    }
}
