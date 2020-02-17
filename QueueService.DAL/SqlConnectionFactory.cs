using Microsoft.Extensions.Options;
using QueueService.Model.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

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
    }
}
