using HMACAuthentication.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QueueService.Api.Model;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace QueueService.Api.Client
{
    public class QueueApiClient : IQueueApiClient
    {
        private readonly QueueApiConfig config;

        private readonly ILogger<QueueApiClient> logger;
        private readonly HttpClient httpClient;


        public QueueApiClient(HttpClient httpClient, ILogger<QueueApiClient> logger, IOptions<QueueApiConfig> config)
        {
            if(config == null || config.Value == null)
                throw new ArgumentException("Config not set");
            this.config = config.Value;

            if (string.IsNullOrEmpty(this.config.Key))
                throw new ArgumentException("Key must be set and must match secret as a key/secret pair (on the queue service)");
            if (string.IsNullOrEmpty(this.config.Secret))
                throw new ArgumentException("Secret must be set and must match key as a key/secret pair (on the queue service)");

            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        }


        /// <summary>
        /// Enqueues a new queue item to be executed by the specified queue/worker in the queue service
        /// </summary>
        /// <param name="request">Contains payload and settings for the job being added to the queue</param>
        /// <returns></returns>
        public async Task<EnqueueResponse> EnqueueAsync(EnqueueRequest request)
        {
            string jsonPayload = null;

            try
            {
                jsonPayload = JsonConvert.SerializeObject(request);
                var startTime = DateTime.UtcNow;

                using (var httpRequest = new HttpRequestMessage(new HttpMethod("POST"), new Uri(httpClient.BaseAddress, "queue")))
                {
                    httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    SignatureHelper.SetHmacHeaders(httpRequest, config.Key, config.Secret, jsonPayload);

                    using (var response = await httpClient.SendAsync(httpRequest, CancellationToken.None))
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<EnqueueResponse>(responseString);
                            return result;
                        }
                        var timeTaken = DateTime.UtcNow.Subtract(startTime);

                        var exception = new Exception($"Http call to enqueue failed. It took {timeTaken.TotalSeconds}s and response code was \"{response.StatusCode}\" with message \"{response.ReasonPhrase}\" payload was: {jsonPayload}. Response content was: {responseString}");
                        throw exception;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error enqueuing, payload: {jsonPayload}");
                throw;
            }
        }
    }
}
