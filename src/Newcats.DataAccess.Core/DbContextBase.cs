using System.Data;
using Microsoft.Extensions.Options;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 数据库上下文基类
    /// </summary>
    public abstract class DbContextBase : IDbContext
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
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>数据库连接</returns>
        public abstract IDbConnection CreateConnection(string connectionString);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor">配置项</param>
        public DbContextBase(IOptions<Core.DbContextOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
            if (Connection != null)
            {
                Connection.ConnectionString = _options.ConnectionString;
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
                return;
            }
            Connection = CreateConnection(_options.ConnectionString);
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