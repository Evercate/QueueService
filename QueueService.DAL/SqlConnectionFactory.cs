using Microsoft.Extensions.Options;
using QueueService.Model.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private DatabaseSettings databaseSettings;

        public SqlConnectionFactory(IOptions<DatabaseSettings> databaseSettings)
        {
            this.databaseSettings = databaseSettings.Value;
        }
        public SqlConnection GetConnection()
        {
            return new SqlConnection(databaseSettings.ConnectionString);
        }
        public async Task OpenAsync(SqlConnection connection, CancellationToken cancellationToken)
        {
            await connection.OpenAsync(cancellationToken);
        }

        public void Close(SqlConnection connection)
        {
            connection.Close();
        }
    }
}
