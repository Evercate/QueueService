using QueueService.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public interface IQueueWorkerRepository
    {
        Task<List<QueueWorker>> GetQueueWorkers();
        Task<List<QueueWorker>> GetQueueWorkers(SqlConnection connection);
    }
}
