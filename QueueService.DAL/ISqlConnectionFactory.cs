using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace QueueService.DAL
{
    public interface ISqlConnectionFactory
    {
        SqlConnection GetConnection();
    }
}
