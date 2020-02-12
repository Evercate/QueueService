using QueueService.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public interface IQueueItemRepository
    {
        Task<bool> HasItems(SqlConnection connection);
        Task<bool> HasItems();
        Task<List<QueueItem>> GetQueueItems(long workerId, short maxRetries, short batchSize);
        Task<List<QueueItem>> GetQueueItems(SqlConnection connection, long workerId, short maxRetries, short batchSize);
        Task SetSuccess(long id);
        Task SetSuccess(SqlConnection connection, long id);
        Task<QueueItem> SetFailed(long id, string error, DateTime? nextRun);
        Task<QueueItem> SetFailed(SqlConnection connection, long id, string error, DateTime? nextRun);
        Task<QueueItem> InsertQueueItem(string workerName, string payload, DateTime? executeOn, string uniqueKey);
        Task<QueueItem> InsertQueueItem(SqlConnection connection, string workerName, string payload, DateTime? executeOn, string uniqueKey);
        Task UnlockStuckQueueItems();
        Task UnlockStuckQueueItems(SqlConnection connection);
        Task ArchiveQueueItems(int days);
        Task ArchiveQueueItems(SqlConnection connection, int days);
    }
}
