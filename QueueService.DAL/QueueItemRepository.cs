using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.Model;
using QueueService.Model.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public class QueueItemRepository : IQueueItemRepository
    {
        private readonly DatabaseSettings databaseSettings;
        private readonly ILogger<QueueItemRepository> logger;


        public QueueItemRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<QueueItemRepository> logger)
        {
            this.databaseSettings = databaseSettings.Value;
            this.logger = logger;
        }

        public async Task<bool> HasItems()
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                var result = await HasItems(connection);
                connection.Close();

                return result;
            }
        }

        public async Task<bool> HasItems(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("[dbo].[HasItems]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                var returnParam = new SqlParameter("@HasItems", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                command.Parameters.Add(returnParam);
                await command.ExecuteNonQueryAsync();

                return (bool)returnParam.Value;
            }
        }


        public async Task ArchiveQueueItems(int days)
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                await ArchiveQueueItems(connection, days);
                connection.Close();
            }
        }

        public async Task ArchiveQueueItems(SqlConnection connection, int days)
        {
            using (SqlCommand command = new SqlCommand("[dbo].[ArchiveQueueItems]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@days", days);
                await command.ExecuteNonQueryAsync();
            }
            
        }

        public async Task UnlockStuckQueueItems()
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                await UnlockStuckQueueItems(connection);
                connection.Close();
            }
        }

        public async Task UnlockStuckQueueItems(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("[dbo].[UnlockStuckQueueItems]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task SetSuccess(long id)
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                await SetSuccess(connection, id);
                connection.Close();
            }
        }

        public async Task SetSuccess(SqlConnection connection, long id)
        {
                using (SqlCommand command = new SqlCommand("[dbo].[SetSuccess]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
        }

        public async Task<QueueItem> SetFailed(long id, string error, DateTime? nextRun)
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                var result = await SetFailed(connection, id, error, nextRun);
                connection.Close();
                return result;
            }
        }

        public async Task<QueueItem> SetFailed(SqlConnection connection, long id, string error, DateTime? nextRun)
        {
            using (SqlCommand command = new SqlCommand("[dbo].[SetFailed]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ExecuteResult", error);
                if (nextRun.HasValue)
                {
                    command.Parameters.AddWithValue("@NextRun", nextRun);
                }
                else
                {
                    command.Parameters.AddWithValue("@NextRun", DBNull.Value);
                }

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    QueueItem item = null;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        item = Map(reader);
                    }
                    reader.Close();

                    return item;
                }
            }
        }

        public async Task<QueueItem> InsertQueueItem(string workerName, string payload, DateTime? executeOn, string uniqueKey)
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                var result = await InsertQueueItem(connection, workerName, payload, executeOn, uniqueKey);
                connection.Close();

                return result;
            }
        }

        public async Task<QueueItem> InsertQueueItem(SqlConnection connection, string workerName, string payload, DateTime? executeOn, string uniqueKey)
        {
            using (SqlCommand command = new SqlCommand("[dbo].[InsertQueueItem]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@WorkerName", workerName);
                command.Parameters.AddWithValue("@Payload", payload);
                command.Parameters.AddWithValue("@ExecuteOn", executeOn);
                command.Parameters.AddWithValue("@UniqueKey", uniqueKey);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    QueueItem item = null;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        item = Map(reader);
                    }
                    reader.Close();

                    return item;
                }
            }  
        }

        public async Task<List<QueueItem>> GetQueueItems(long workerId, short maxRetries, short batchSize)
        {
            using (SqlConnection connection = new SqlConnection(databaseSettings.ConnectionString))
            {
                await connection.OpenAsync();
                var result = await GetQueueItems(connection, workerId, maxRetries, batchSize);
                connection.Close();

                return result;
            }
        }

        public async Task<List<QueueItem>> GetQueueItems(SqlConnection connection, long workerId, short maxRetries, short batchSize)
        {

            using (SqlCommand command = new SqlCommand("[dbo].[GetNextId]", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@WorkerId", workerId);
                command.Parameters.AddWithValue("@MaxRetries", maxRetries);
                command.Parameters.AddWithValue("@BatchSize", batchSize);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<QueueItem>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var queueItem = Map(reader);
                            result.Add(queueItem);
                        }
                    }

                    reader.Close();

                    return result;
                }
            }
            
        }

        private QueueItem Map(SqlDataReader reader)
        {
            var result = new QueueItem
            {
                Id = reader.GetInt64(0),
                QueueWorkerId = reader.GetInt64(1),
                State = (QueueItemState)reader.GetInt16(2),
                CreateDate = reader.GetDateTime(3),
                Payload = reader.IsDBNull(4) ? null : reader.GetString(4),
                Tries = reader.GetInt16(5),
                ExecuteTimeStart = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                ExecuteTimeEnd = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                ExecuteTimeNext = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                ExecuteResult = reader.IsDBNull(9) ? null : reader.GetString(9),
                UniqueKey = reader.IsDBNull(10) ? null : reader.GetString(10)
            };

            return result;
        }
    }
}
