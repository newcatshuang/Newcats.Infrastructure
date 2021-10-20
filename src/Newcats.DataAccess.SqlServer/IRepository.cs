namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// SqlServer仓储接口,提供数据库访问能力,封装了基本的CRUD方法。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <typeparam name="TPrimaryKey">此数据库实体类的主键类型</typeparam>
    public interface IRepository<TDbContext, TEntity, TPrimaryKey> : Core.IRepository<TDbContext, TEntity, TPrimaryKey> where TEntity : class where TDbContext : Core.IDbContext
    {
    }
}