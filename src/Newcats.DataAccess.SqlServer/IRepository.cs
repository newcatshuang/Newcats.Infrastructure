namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// SqlServer仓储接口,提供数据库访问能力,封装了基本的CRUD方法。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
    public interface IRepository<TDbContext> : Core.IRepository<TDbContext> where TDbContext : Core.IDbContext
    {
    }
}