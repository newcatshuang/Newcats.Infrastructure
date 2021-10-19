using System.Data;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; }
    }
}