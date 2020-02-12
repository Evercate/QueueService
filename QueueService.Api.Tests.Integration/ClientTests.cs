using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.Api.Tests.Integration
{
    [TestClass]
    public class ClientTests
    {
        private static HttpClient client;
        private static Client.QueueApiClient apiClient;
        private static Mock<ILogger<QueueService.Api.Client.QueueApiClient>> loggerMock;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext context)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseStartup<QueueService.Api.Startup>();
                });
            hostBuilder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            var host = await hostBuilder.StartAsync();

            client = host.GetTestClient();
            client.BaseAddress = new System.Uri(client.BaseAddress.ToString().Replace("http", "https"));

            loggerMock = new Mock<ILogger<Client.QueueApiClient>>();

            apiClient = new Client.QueueApiClient(client, loggerMock.Object, Options.Create(new Client.QueueApiConfig { Key = "DevQueueService", Secret = "devhmacsecret" }));
        }

        [TestMethod]
        public async Task PostTest()
        {
            await Assert.ThrowsExceptionAsync<Exception>(() => apiClient.EnqueueAsync(new Model.EnqueueRequest { QueueName = "NOT EXISTS", Payload = "test" }));
        }
    }
}
