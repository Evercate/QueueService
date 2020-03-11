using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using QueueService.Api.Client.Exceptions;
using QueueService.Api.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Api.Client.Tests
{
    [TestClass]
    public class EnqueueTest
    {
        [TestMethod]
        public async Task RetryOnFailure()
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent("{'State':'processing'}"),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://no-valid-url.com/"),
            };

            var settings = new QueueApiConfig()
            {
                Key = "fakeKey",
                Secret = "fakeSecret",
                Retries = 3,
                RetryDelay = 3
            };
            IOptions<QueueApiConfig> appSettingsOptions = Options.Create(settings);

            var loggerMock = new Mock<ILogger<QueueApiClient>>();

            var subjectUnderTest = new QueueApiClient(httpClient, loggerMock.Object, appSettingsOptions);

            var request = new EnqueueRequest()
            {
                QueueName = "FakeQueue",
                Payload = "{'fakePayload':'fakevalue'}"
            };

            // ACT
            await Assert.ThrowsExceptionAsync<EnqueueFailedException>(() => subjectUnderTest.EnqueueAsync(request));

            // ASSERT
            
            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://no-valid-url.com/queue");

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(4), // one original call + 3 retries
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );

            
        }
    }
}
