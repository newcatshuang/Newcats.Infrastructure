using System.Data;
using MySqlConnector;

namespace Newcats.DataAccess.MySql
{
    /// <summary>
    /// SqlServer数据库上下文基类
    /// </summary>
    public class DbContextBase : Core.IDbContext
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString"></param>
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

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
