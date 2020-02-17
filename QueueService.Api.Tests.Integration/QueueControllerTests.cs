using HMACAuthentication.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueService.Api.Model;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Api.Tests.Integration
{
    [TestClass]
    [TestCategory("Integration")]
    public class QueueControllerTests
    {

        private static HttpClient client;

        [ClassInitialize]
        public static async Task TestFixtureSetup(TestContext context)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseStartup<Startup>();
                });
            hostBuilder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            var host = await hostBuilder.StartAsync();

            client = host.GetTestClient();
            client.BaseAddress = new Uri(client.BaseAddress.ToString().Replace("http", "https"));
        }
        [TestMethod]
        public async Task GetRootUrlTest()
        {
            var response = await client.GetAsync("");
            Assert.IsTrue(response.IsSuccessStatusCode);
        }


        [TestMethod]
        public async Task GetNotSupportedTest()
        {
            using var response = await client.GetAsync("/queue");
            Assert.AreEqual(System.Net.HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public async Task PostNotAuthenticated()
        {
            using var response = await client.PostAsync("/queue", new StringContent("somecontent"));
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        [DataRow(6)]
        [DataRow(-6)]
        public async Task PostDateOutOfRange(int offset)
        {
            string payload = "test";

            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request.Content = new StringContent(payload);
            request.Headers.Date = DateTimeOffset.UtcNow.AddMinutes(offset);
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task PostNotJson()
        {
            string payload = "test";

            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request.Content = new StringContent(payload);
            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [TestMethod]
        public async Task PostAlteredContent()
        {
            string payload = "test";
            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request.Content = new StringContent(payload);
            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);
            request.Content = new StringContent("test1");
            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [TestMethod]
        [DataRow("DevQueueService1", "devhmacsecret")]
        [DataRow("DevQueueService", "devhmacsecret1")]
        [DataRow("DevQueueService1", "devhmacsecret1")]
        public async Task PostWrongCredentials(string key, string secret)
        {
            string payload = "test";
            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request.Content = new StringContent(payload);
            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate(secret, SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", $"{key}:{authenticationSignature}");
            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task PostReplayed()
        {
            string payload = "test";
            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request.Content = new StringContent(payload);
            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.UnsupportedMediaType, response.StatusCode);

            var request2 = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
            request2.Content = new StringContent(payload);
            request2.Headers.Date = request.Headers.Date;
            request2.Headers.Add("Nonce", nonce);
            request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response2 = await client.SendAsync(request2, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response2.StatusCode);
        }

        [TestMethod]
        public async Task PostExecuteInThePast()
        {
            string payload = System.Text.Json.JsonSerializer.Serialize(new EnqueueRequest { Payload = "test", QueueName = "test", ExecuteOn = DateTime.UtcNow.AddMinutes(-5) });

            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"));
           
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
      
            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task PostExecuteWorkerNotExists()
        {
            string payload = System.Text.Json.JsonSerializer.Serialize(new EnqueueRequest { Payload = "test", QueueName = "test"});

            var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri(client.BaseAddress, "/queue"))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            request.Headers.Date = DateTimeOffset.UtcNow;
            var nonce = Guid.NewGuid().ToString();
            request.Headers.Add("Nonce", nonce);
            string authenticationSignature = SignatureHelper.Calculate("devhmacsecret", SignatureHelper.Generate(request.Headers.Date.Value, payload, request.Method.Method, request.RequestUri.AbsolutePath, request.RequestUri.Query, nonce));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", "DevQueueService:" + authenticationSignature);

            using var response = await client.SendAsync(request, CancellationToken.None);
            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }
}


