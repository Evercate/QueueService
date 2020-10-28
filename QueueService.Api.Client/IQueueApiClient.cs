using QueueService.Api.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Api.Client
{
    public interface IQueueApiClient
    {
        Task<EnqueueResponse> EnqueueAsync(EnqueueRequest request);
    }
}
