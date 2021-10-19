﻿using MySqlConnector;
using System.Data;

namespace Newcats.DataAccess.MySQL
{
    /// <summary>
    /// SqlServer数据库上下文基类
    /// </summary>
    public class DbContextBase : IDbContext
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }

        public DbContextBase(string connectionString)
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                return;
            }
            Connection = MySqlConnectorFactory.Instance.CreateConnection();
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
