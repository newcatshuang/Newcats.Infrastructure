using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Newcats.DataAccess.MySql
{
    /// <summary>
    /// MySql数据库上下文基类
    /// </summary>
    public abstract class DbContextBase : Core.IDbContext
    {
        /// <summary>
        /// 选项
        /// </summary>
        private readonly DbContextOptions _options;

        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor">配置项</param>
        public DbContextBase(IOptions<DbContextOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.ConnectionString = _options.ConnectionString;
                    Connection.Open();
                }
                return;
            }
            Connection = MySqlConnectorFactory.Instance.CreateConnection();
            Connection.ConnectionString = _options.ConnectionString;
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
