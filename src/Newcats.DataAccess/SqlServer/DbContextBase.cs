using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Newcats.DataAccess.SqlServer
{
    public class DbContextBase : IDisposable
    {
        public IDbConnection Connection { get; }

        public IDbTransaction GetTransaction()
        {
            return Connection.BeginTransaction();
        }

        public DbContextBase(string connectionString)
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                return;
            }
            Connection = SqlClientFactory.Instance.CreateConnection();
            Connection.ConnectionString = connectionString;
            Connection.Open();
        }

        public void Dispose()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
