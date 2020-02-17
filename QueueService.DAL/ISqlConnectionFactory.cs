using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.DAL
{
    public interface ISqlConnectionFactory
    {
        SqlConnection GetConnection();
        Task OpenAsync(SqlConnection connection, CancellationToken cancellationToken);
        void Close(SqlConnection connection);
    }
}
