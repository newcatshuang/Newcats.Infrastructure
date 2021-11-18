using Microsoft.Extensions.Options;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 数据库上下文选项
    /// </summary>
    public class DbContextOptions : IOptions<DbContextOptions>
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public DbContextOptions Value
        {
            get
            {
                return this;
            }
        }
    }
}
