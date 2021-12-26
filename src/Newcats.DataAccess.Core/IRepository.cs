using System.Data;
using Dapper;

namespace Newcats.DataAccess.Core;
/// <summary>
/// 通用仓储接口,提供数据库访问能力,封装了基本的CRUD方法。
/// </summary>
/// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
public interface IRepository<TDbContext> where TDbContext : IDbContext
{
    #region 同步方法
    /// <summary>
    /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
    object Insert<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 插入一条数据，返回是否成功
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回true，否则返回false</returns>
    bool Insert<TEntity>(TEntity entity, IDbTransaction transaction) where TEntity : class;

    /// <summary>
    /// 批量插入数据，返回成功的条数
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int InsertBulk<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 通过SqlBulkCopy批量插入数据，返回成功的条数
    /// (此方法性能最优)(MySql要在连接字符串加上 AllowLoadLocalInfile=true 并且服务端设置变量 set global local_infile = 1;或修改全局配置文件)
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int InsertSqlBulkCopy<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，删除一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int Delete<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，删除记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int Delete<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，更新一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int Update<TEntity>(object primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，更新记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    int Update<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，获取一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    TEntity Get<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，获取一条记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序集合</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    TEntity Get<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 分页获取数据
    /// </summary>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据</returns>
    (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="dbOrderBy">排序</param>
    /// <returns>分页数据集合</returns>
    PageInfo<TEntity> GetPageInfo<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <returns>分页数据集合</returns>
    PageInfo<TEntity> GetPageInfo<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true) where TEntity : class;

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，获取所有数据
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    IEnumerable<TEntity> GetAll<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据默认排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    IEnumerable<TEntity> GetTop<TEntity>(int top) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    IEnumerable<TEntity> GetTop<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 获取记录总数量
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录总数量</returns>
    int Count<TEntity>() where TEntity : class;

    /// <summary>
    /// 根据给定的条件，获取记录数量
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录数量</returns>
    int Count<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，判断数据是否存在
    /// </summary>
    /// <param name="primaryKeyValue">主键值</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    bool Exists<TEntity>(object primaryKeyValue) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，判断数据是否存在
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    bool Exists<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="storedProcedureName">存储过程名称</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <returns>受影响的行数</returns>
    int ExecuteStoredProcedure(string storedProcedureName, DynamicParameters pars, IDbTransaction? transaction = null, int? commandTimeout = null);

    /// <summary>
    /// 执行sql语句，返回受影响的行数
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>受影响的行数</returns>
    int Execute(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    T ExecuteScalar<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    object ExecuteScalar(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，返回结果集
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果集</returns>
    IEnumerable<T> Query<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行单行查询，返回结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    T QueryFirstOrDefault<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    #endregion

    #region 异步方法
    /// <summary>
    /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
    Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 插入一条数据，返回是否成功
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回true，否则返回false</returns>
    Task<bool> InsertAsync<TEntity>(TEntity entity, IDbTransaction transaction) where TEntity : class;

    /// <summary>
    /// 批量插入数据，返回成功的条数
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> InsertBulkAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 通过SqlBulkCopy批量插入数据，返回成功的条数
    /// (此方法性能最优)(MySql要在连接字符串加上 AllowLoadLocalInfile=true 并且服务端设置变量 set global local_infile = 1;或修改全局配置文件)
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> InsertSqlBulkCopyAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，删除一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> DeleteAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，删除记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> DeleteAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，更新一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> UpdateAsync<TEntity>(object primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，更新记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    Task<int> UpdateAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，获取一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    Task<TEntity> GetAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，获取一条记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序集合</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    Task<TEntity> GetAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 分页获取数据
    /// </summary>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据</returns>
    Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="dbOrderBy">排序</param>
    /// <returns>分页数据集合</returns>
    Task<PageInfo<TEntity>> GetPageInfoAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <returns>分页数据集合</returns>
    Task<PageInfo<TEntity>> GetPageInfoAsync<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true) where TEntity : class;

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，获取所有数据
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 根据默认排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top) where TEntity : class;

    /// <summary>
    /// 根据给定的条件及排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 获取记录总数量
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录总数量</returns>
    Task<int> CountAsync<TEntity>() where TEntity : class;

    /// <summary>
    /// 根据给定的条件，获取记录数量
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录数量</returns>
    Task<int> CountAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，判断数据是否存在
    /// </summary>
    /// <param name="primaryKeyValue">主键值</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync<TEntity>(object primaryKeyValue) where TEntity : class;

    /// <summary>
    /// 根据给定的条件，判断数据是否存在
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="storedProcedureName">存储过程名称</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <returns>受影响的行数</returns>
    Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, DynamicParameters pars, IDbTransaction? transaction = null, int? commandTimeout = null);

    /// <summary>
    /// 执行sql语句，返回受影响的行数
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>受影响的行数</returns>
    Task<int> ExecuteAsync(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    Task<T> ExecuteScalarAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    Task<object> ExecuteScalarAsync(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行查询，返回结果集
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果集</returns>
    Task<IEnumerable<T>> QueryAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);

    /// <summary>
    /// 执行单行查询，返回结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    Task<T> QueryFirstOrDefaultAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    #endregion

    #region 事务
    /// <summary>
    /// 开启事务
    /// </summary>
    /// <returns>事务</returns>
    IDbTransaction BeginTransaction();

    /// <summary>
    /// 开启事务
    /// </summary>
    /// <param name="il">事务等级</param>
    /// <returns>事务</returns>
    IDbTransaction BeginTransaction(IsolationLevel il);

    /// <summary>
    /// 执行通用事务
    /// </summary>
    /// <param name="actions">事务方法</param>
    /// <returns>是否成功</returns>
    bool Execute(IEnumerable<Action<IDbTransaction>> actions);
    #endregion
}