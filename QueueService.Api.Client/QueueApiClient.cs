using HMACAuthentication.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QueueService.Api.Client.Exceptions;
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
        public async Task<Client.Model.EnqueueResponse> EnqueueAsync(Api.Model.EnqueueRequest request)
        {
            var jsonPayload = JsonConvert.SerializeObject(request);
            var startTime = DateTime.UtcNow;

            for (int i = 0; i <= config.Retries; i++)
            {
                //For each try we must redo the entire request as hmac specifics (nonce, time) needs to be updated
                using (var httpRequest = new HttpRequestMessage(new HttpMethod("POST"), new Uri(httpClient.BaseAddress, "queue")))
                {
                    httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    SignatureHelper.SetHmacHeaders(httpRequest, config.Key, config.Secret, jsonPayload);

                    var response = await httpClient.SendAsync(httpRequest, CancellationToken.None);
                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Api.Model.EnqueueResponse>(responseString);

                        if(!result.Success)
                            throw new EnqueueFailedException($"Failed to enqueue. The queue service responded with a http status indicating success but the return object says error. Payload: {jsonPayload}  Error message: {result.ErrorMessage}");

                        return new Model.EnqueueResponse() { 
                            State = result.State,
                            CreateDate = result.CreateDate,
                            UniqueKey = result.UniqueKey
                        };
                    }
                    var timeTaken = DateTime.UtcNow.Subtract(startTime);
                    var callInformation = $"Call took {timeTaken.TotalSeconds}s and response code was \"{response.StatusCode}\" with message \"{response.ReasonPhrase}\" and response content: \"{responseString}\" payload was: \"{jsonPayload}\". it was for queue \"{request.QueueName}\" with unique name \"{request.UniqueKey}\" and was to be executed \"{(request.ExecuteOn.HasValue ? request.ExecuteOn.Value.ToString("yyyy-MM-dd HH:mm:ss") : "immidately")}\". ";

                    logger.LogWarning($"Enqueue attempt failed. This was attempt {(i + 1)} of {(config.Retries + 1)}. {callInformation}");

                    if (i < config.Retries)
                    {
                        await Task.Delay(config.RetryDelay * 1000);
                    }
                    
                }            

            }

            throw new EnqueueFailedException($"Failed to enqueue. All {(config.Retries + 1)} attempts failed.");

        }
    }
}
