using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueService.Api.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

//[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace QueueService.Api.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class ClientTests
    {
        private static HttpClient client;
        private static Client.QueueApiClient apiClient;
        private static Mock<ILogger<QueueService.Api.Client.QueueApiClient>> loggerMock;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext context)
        {
            var factory = new WebApplicationFactory<Program>();
            client = factory.CreateClient();
            client.BaseAddress = new Uri(client.BaseAddress.ToString().Replace("http", "https"));

            loggerMock = new Mock<ILogger<Client.QueueApiClient>>();

            apiClient = new Client.QueueApiClient(client, loggerMock.Object, Options.Create(new Client.QueueApiConfig { Key = "DevQueueService", Secret = "devhmacsecret" }));
        }

        [TestMethod]
        public async Task PostTest()
        {
            await Assert.ThrowsExceptionAsync<EnqueueFailedException>(() => apiClient.EnqueueAsync(new Model.EnqueueRequest { QueueName = "NOT EXISTS", Payload = "test" }));
        }
    }
}
