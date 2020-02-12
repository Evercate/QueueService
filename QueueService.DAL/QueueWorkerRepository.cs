using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.Model;
using QueueService.Model.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public class QueueWorkerRepository : IQueueWorkerRepository
    {
        private readonly DatabaseSettings databaseSettings;
        private readonly ILogger<QueueItemRepository> logger;

        public QueueWorkerRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<QueueItemRepository> logger)
        {
            this.databaseSettings = databaseSettings.Value;
            this.logger = logger;
        }

        public async Task<List<QueueWorker>> GetQueueWorkers()
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                var result = await GetQueueWorkers(connection);
                connection.Close();
                return result;
            }
        }

        public async Task<List<QueueWorker>> GetQueueWorkers(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("Select Id, Name, Endpoint, Method, Priority, Retries, MaxProcessingTime, BatchSize, Enabled, RetryDelay, RetryDelayMultiplier, ApiKey from queueworker with(nolock)", connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<QueueWorker>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var worker = Map(reader);
                            result.Add(worker);
                        }
                    }
                    reader.Close();

                    return result;
                }
            }      
        }

        private QueueWorker Map(SqlDataReader reader)
        {
            var result = new QueueWorker
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Endpoint = reader.GetString(2),
                Method = reader.GetString(3),
                Priority = reader.GetInt16(4),
                Retries = reader.GetInt16(5),
                MaxProcessingTime = reader.GetInt16(6),
                BatchSize = reader.GetInt16(7),
                Enabled = reader.GetBoolean(8),
                RetryDelay = reader.GetInt16(9),
                RetryDelayMultiplier = reader.GetInt16(10),
                ApiKey = reader.IsDBNull(11) ? null : reader.GetString(11)
            };

            return result;
        }
    }
}
