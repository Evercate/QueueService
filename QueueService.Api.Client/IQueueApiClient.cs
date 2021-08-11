using System.Threading.Tasks;

namespace QueueService.Api.Client
{
    public interface IQueueApiClient
    {
        Task<Client.Model.EnqueueResponse> EnqueueAsync(Api.Model.EnqueueRequest request);
    }
}
