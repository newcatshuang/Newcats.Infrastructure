using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// SqlServer数据库上下文
    /// </summary>
    public class DbContext : DbContextBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor">配置项</param>
        public DbContext(IOptions<DbContextOptions> optionsAccessor) : base(optionsAccessor)
        {
        }

        /// <summary>
        /// 创建数据库连接的SqlServer实现
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>数据库连接</returns>
        public override IDbConnection CreateConnection(string connectionString)
        {
            return SqlClientFactory.Instance.CreateConnection();
        }
    }
}