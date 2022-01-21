/***************************************************************************
 *GUID: b6a5cac1-5597-422d-a5ad-ffb20320ee14
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-11-18 19:01:26
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Dapper;

namespace Newcats.DataAccess.Core;
/// <summary>
/// 仓储实现类基类,提供了默认的CRUD方法,对于不同数据库有不同实现的函数,需要重写相关抽象方法。
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public abstract class RepositoryBase<TDbContext> : IRepository<TDbContext> where TDbContext : IDbContext
{
    /// <summary>
    /// 主库数据库连接
    /// </summary>
    public abstract IDbConnection Connection { get; }

    /// <summary>
    /// 从库数据库连接(不为null则表示启用了读写分离)
    /// </summary>
    public abstract IDbConnection? ReplicaConnection { get; }

    #region 同步方法
    #region 写操作
    /// <summary>
    /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
    public abstract object Insert<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 插入一条数据，返回是否成功
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回true，否则返回false</returns>
    public bool Insert<TEntity>(TEntity entity, IDbTransaction transaction) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));
        string sqlText = RepositoryHelper.GetInsertSqlText(typeof(TEntity));
        return Connection.Execute(sqlText, entity, transaction, commandType: CommandType.Text) > 0;
    }

    /// <summary>
    /// 批量插入数据，返回成功的条数
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public int InsertBulk<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (list == null || !list.Any())
            throw new ArgumentNullException(nameof(list));

        string sqlText = RepositoryHelper.GetInsertSqlText(typeof(TEntity));
        return Connection.Execute(sqlText, list, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 通过SqlBulkCopy批量插入数据，返回成功的条数
    /// (此方法性能最优)(MySql要在连接字符串加上 AllowLoadLocalInfile=true 并且服务端设置变量 set global local_infile = 1;或修改全局配置文件)
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public abstract int InsertSqlBulkCopy<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，删除一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public int Delete<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (primaryKeyValue == null)
            throw new ArgumentNullException(nameof(primaryKeyValue));

        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string pkName = RepositoryHelper.GetTablePrimaryKey(type);
        string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
        DynamicParameters parameters = new();
        parameters.Add("@p_1", primaryKeyValue);
        return Connection.Execute(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据给定的条件，删除记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public int Delete<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        string sqlText = $" DELETE FROM {tableName} {sqlWhere} ;";
        return Connection.Execute(sqlText, pars, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据主键，更新一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public int Update<TEntity>(object primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (primaryKeyValue == null)
            throw new ArgumentNullException(nameof(primaryKeyValue));
        if (dbUpdates == null || !dbUpdates.Any())
            throw new ArgumentNullException(nameof(dbUpdates));

        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string pkName = RepositoryHelper.GetTablePrimaryKey(type);
        string sqlUpdate = string.Empty;
        DynamicParameters parameters = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
        parameters.Add("@" + pkName, primaryKeyValue);
        string sqlText = $" UPDATE {tableName} SET {sqlUpdate} WHERE {pkName}=@{pkName} ;";
        return Connection.Execute(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据给定的条件，更新记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public int Update<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (dbUpdates == null || !dbUpdates.Any())
            throw new ArgumentNullException(nameof(dbUpdates));
        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters wherePars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        string sqlUpdate = string.Empty;
        DynamicParameters updatePars = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
        wherePars.AddDynamicParams(updatePars);
        string sqlText = $" UPDATE {tableName} SET {sqlUpdate} {sqlWhere} ;";
        return Connection.Execute(sqlText, wherePars, transaction, commandTimeout, CommandType.Text);
    }
    #endregion

    #region 读操作
    /// <summary>
    /// 根据主键，获取一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    public TEntity Get<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        DbWhere<TEntity> dbWhere = new DbWhere<TEntity>(RepositoryHelper.GetTablePrimaryKey(typeof(TEntity)), primaryKeyValue);
        return Get(new List<DbWhere<TEntity>> { dbWhere }, transaction, commandTimeout, forceToMain);
    }

    /// <summary>
    /// 根据给定的条件，获取一条记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序集合</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    public TEntity Get<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        IEnumerable<TEntity>? r = GetTop(1, dbWheres, transaction, commandTimeout, forceToMain, dbOrderBy);
        if (r != null && r.Any())
            return r.First();
        return null;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public abstract (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 分页获取数据
    /// </summary>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据</returns>
    public (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false) where TEntity : class
    {
        return GetPage(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, returnTotal, forceToMain, pageInfo.OrderBy?.ToArray());
    }

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
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <returns>分页数据集合</returns>
    public PageInfo<TEntity> GetPageInfo<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var r = GetPage(pageIndex, pageSize, dbWheres, transaction, commandTimeout, returnTotal, forceToMain, dbOrderBy);
        PageInfo<TEntity> page = new PageInfo<TEntity>(pageIndex, pageSize);
        page.TotalRecords = r.totalCount;
        page.Data = r.list == null ? null : r.list.ToList();
        return page;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <returns>分页数据集合</returns>
    public PageInfo<TEntity> GetPageInfo<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false) where TEntity : class
    {
        return GetPageInfo(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, returnTotal, forceToMain, pageInfo.OrderBy?.ToArray());
    }

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
    {
        var (list, _) = GetPage<TEntity>(0, 0, null, null, null, false);
        return list;
    }

    /// <summary>
    /// 根据给定的条件及排序，获取所有数据
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public IEnumerable<TEntity> GetAll<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var (list, _) = GetPage(0, 0, dbWheres, transaction, commandTimeout, false, forceToMain, dbOrderBy);
        return list;
    }

    /// <summary>
    /// 根据默认排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    public IEnumerable<TEntity> GetTop<TEntity>(int top, bool forceToMain = false) where TEntity : class
    {
        var (list, _) = GetPage<TEntity>(0, top, null, null, null, false, forceToMain);
        return list;
    }

    /// <summary>
    /// 根据给定的条件及排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public IEnumerable<TEntity> GetTop<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var (list, _) = GetPage(0, top, dbWheres, transaction, commandTimeout, false, forceToMain, dbOrderBy);
        return list;
    }

    /// <summary>
    /// 获取记录总数量
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录总数量</returns>
    public int Count<TEntity>() where TEntity : class
    {
        return Count<TEntity>(null, null);
    }

    /// <summary>
    /// 根据给定的条件，获取记录数量
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录数量</returns>
    public int Count<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        if (dbWheres != null && dbWheres.Any())
        {
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            return useReplica ?
                ReplicaConnection.ExecuteScalar<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text) :
                Connection.ExecuteScalar<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
        }
        else
        {
            string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
            return useReplica ?
                ReplicaConnection.ExecuteScalar<int>(sqlText, null, transaction, commandTimeout, CommandType.Text) :
                Connection.ExecuteScalar<int>(sqlText, null, transaction, commandTimeout, CommandType.Text);
        }
    }

    /// <summary>
    /// 根据主键，判断数据是否存在
    /// </summary>
    /// <param name="primaryKeyValue">主键值</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    public bool Exists<TEntity>(object primaryKeyValue, bool forceToMain = false) where TEntity : class
    {
        DbWhere<TEntity> dbWhere = new DbWhere<TEntity>(RepositoryHelper.GetTablePrimaryKey(typeof(TEntity)), primaryKeyValue);
        return Exists(new List<DbWhere<TEntity>> { dbWhere }, null, null, forceToMain);
    }

    /// <summary>
    /// 根据给定的条件，判断数据是否存在
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    public abstract bool Exists<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class;
    #endregion

    #region 执行自定义sql
    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="storedProcedureName">存储过程名称</param>
    /// <param name="pars">参数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <returns>受影响的行数</returns>
    public int ExecuteStoredProcedure(string storedProcedureName, DynamicParameters pars, bool forceToMain, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(storedProcedureName))
            throw new ArgumentNullException(nameof(storedProcedureName));
        return useReplica ?
            ReplicaConnection.Execute(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure) :
            Connection.Execute(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure);
    }

    /// <summary>
    /// 执行sql语句，返回受影响的行数
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>受影响的行数</returns>
    public int Execute(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            ReplicaConnection.Execute(sqlText, pars, transaction, commandTimeout, commandType) :
            Connection.Execute(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public T ExecuteScalar<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            ReplicaConnection.ExecuteScalar<T>(sqlText, pars, transaction, commandTimeout, commandType) :
            Connection.ExecuteScalar<T>(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public object ExecuteScalar(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return
            useReplica ?
            ReplicaConnection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, commandType) :
            Connection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，返回结果集
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果集</returns>
    public IEnumerable<T> Query<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            ReplicaConnection.Query<T>(sqlText, pars, transaction, true, commandTimeout, commandType) :
            Connection.Query<T>(sqlText, pars, transaction, true, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行单行查询，返回结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public T QueryFirstOrDefault<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            ReplicaConnection.QueryFirstOrDefault<T>(sqlText, pars, transaction, commandTimeout, commandType) :
            Connection.QueryFirstOrDefault<T>(sqlText, pars, transaction, commandTimeout, commandType);
    }
    #endregion
    #endregion

    #region 异步方法
    #region 写操作
    /// <summary>
    /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
    public abstract Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 插入一条数据，返回是否成功
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回true，否则返回false</returns>
    public async Task<bool> InsertAsync<TEntity>(TEntity entity, IDbTransaction transaction) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));
        string sqlText = RepositoryHelper.GetInsertSqlText(typeof(TEntity));
        return await Connection.ExecuteAsync(sqlText, entity, transaction, commandType: CommandType.Text) > 0;
    }

    /// <summary>
    /// 批量插入数据，返回成功的条数
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public async Task<int> InsertBulkAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (list == null || !list.Any())
            throw new ArgumentNullException(nameof(list));

        string sqlText = RepositoryHelper.GetInsertSqlText(typeof(TEntity));
        return await Connection.ExecuteAsync(sqlText, list, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 通过SqlBulkCopy批量插入数据，返回成功的条数
    /// (此方法性能最优)(MySql要在连接字符串加上 AllowLoadLocalInfile=true 并且服务端设置变量 set global local_infile = 1;或修改全局配置文件)
    /// </summary>
    /// <param name="list">要插入的数据实体集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public abstract Task<int> InsertSqlBulkCopyAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class;

    /// <summary>
    /// 根据主键，删除一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public async Task<int> DeleteAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (primaryKeyValue == null)
            throw new ArgumentNullException(nameof(primaryKeyValue));

        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string pkName = RepositoryHelper.GetTablePrimaryKey(type);
        string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
        DynamicParameters parameters = new();
        parameters.Add("@p_1", primaryKeyValue);
        return await Connection.ExecuteAsync(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据给定的条件，删除记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public async Task<int> DeleteAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        string sqlText = $" DELETE FROM {tableName} {sqlWhere} ;";
        return await Connection.ExecuteAsync(sqlText, pars, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据主键，更新一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public async Task<int> UpdateAsync<TEntity>(object primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (primaryKeyValue == null)
            throw new ArgumentNullException(nameof(primaryKeyValue));
        if (dbUpdates == null || !dbUpdates.Any())
            throw new ArgumentNullException(nameof(dbUpdates));

        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string pkName = RepositoryHelper.GetTablePrimaryKey(type);
        string sqlUpdate = string.Empty;
        DynamicParameters parameters = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
        parameters.Add("@" + pkName, primaryKeyValue);
        string sqlText = $" UPDATE {tableName} SET {sqlUpdate} WHERE {pkName}=@{pkName} ;";
        return await Connection.ExecuteAsync(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
    }

    /// <summary>
    /// 根据给定的条件，更新记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="dbUpdates">要更新的字段集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功的条数</returns>
    public async Task<int> UpdateAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        if (dbUpdates == null || !dbUpdates.Any())
            throw new ArgumentNullException(nameof(dbUpdates));
        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters wherePars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        string sqlUpdate = string.Empty;
        DynamicParameters updatePars = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
        wherePars.AddDynamicParams(updatePars);
        string sqlText = $" UPDATE {tableName} SET {sqlUpdate} {sqlWhere} ;";
        return await Connection.ExecuteAsync(sqlText, wherePars, transaction, commandTimeout, CommandType.Text);
    }
    #endregion

    #region 读操作
    /// <summary>
    /// 根据主键，获取一条记录
    /// </summary>
    /// <param name="primaryKeyValue">主键的值</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    public async Task<TEntity> GetAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        DbWhere<TEntity> dbWhere = new DbWhere<TEntity>(RepositoryHelper.GetTablePrimaryKey(typeof(TEntity)), primaryKeyValue);
        return await GetAsync(new List<DbWhere<TEntity>> { dbWhere }, transaction, commandTimeout, forceToMain);
    }

    /// <summary>
    /// 根据给定的条件，获取一条记录
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序集合</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据库实体或null</returns>
    public async Task<TEntity> GetAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        IEnumerable<TEntity>? r = await GetTopAsync(1, dbWheres, transaction, commandTimeout, forceToMain, dbOrderBy);
        if (r != null && r.Any())
            return r.First();
        return null;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="dbOrderBy">排序</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public abstract Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class;

    /// <summary>
    /// 分页获取数据
    /// </summary>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据</returns>
    public async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false) where TEntity : class
    {
        return await GetPageAsync(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, returnTotal, forceToMain, pageInfo.OrderBy?.ToArray());
    }

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
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <returns>分页数据集合</returns>
    public async Task<PageInfo<TEntity>> GetPageInfoAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var r = await GetPageAsync(pageIndex, pageSize, dbWheres, transaction, commandTimeout, returnTotal, forceToMain, dbOrderBy);
        PageInfo<TEntity> page = new PageInfo<TEntity>(pageIndex, pageSize);
        page.TotalRecords = r.totalCount;
        page.Data = r.list == null ? null : r.list.ToList();
        return page;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <param name="pageInfo">分页信息</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <returns>分页数据集合</returns>
    public async Task<PageInfo<TEntity>> GetPageInfoAsync<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false) where TEntity : class
    {
        return await GetPageInfoAsync(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, returnTotal, forceToMain, pageInfo.OrderBy?.ToArray());
    }

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class
    {
        var (list, _) = await GetPageAsync<TEntity>(0, 0, null, null, null, false);
        return list;
    }

    /// <summary>
    /// 根据给定的条件及排序，获取所有数据
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>数据集合</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var (list, _) = await GetPageAsync(0, 0, dbWheres, transaction, commandTimeout, false, forceToMain, dbOrderBy);
        return list;
    }

    /// <summary>
    /// 根据默认排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    public async Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top, bool forceToMain = false) where TEntity : class
    {
        var (list, _) = await GetPageAsync<TEntity>(0, top, null, null, null, false, forceToMain);
        return list;
    }

    /// <summary>
    /// 根据给定的条件及排序，获取指定数量的数据
    /// </summary>
    /// <param name="top">指定数量</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>指定数量的数据集合</returns>
    public async Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        var (list, _) = await GetPageAsync(0, top, dbWheres, transaction, commandTimeout, false, forceToMain, dbOrderBy);
        return list;
    }

    /// <summary>
    /// 获取记录总数量
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录总数量</returns>
    public async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        return await CountAsync<TEntity>(null, null);
    }

    /// <summary>
    /// 根据给定的条件，获取记录数量
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>记录数量</returns>
    public async Task<int> CountAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        if (dbWheres != null && dbWheres.Any())
        {
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            return useReplica ?
               await ReplicaConnection.ExecuteScalarAsync<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text) :
               await Connection.ExecuteScalarAsync<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
        }
        else
        {
            string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
            return useReplica ?
               await ReplicaConnection.ExecuteScalarAsync<int>(sqlText, null, transaction, commandTimeout, CommandType.Text) :
               await Connection.ExecuteScalarAsync<int>(sqlText, null, transaction, commandTimeout, CommandType.Text);
        }
    }

    /// <summary>
    /// 根据主键，判断数据是否存在
    /// </summary>
    /// <param name="primaryKeyValue">主键值</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync<TEntity>(object primaryKeyValue, bool forceToMain = false) where TEntity : class
    {
        DbWhere<TEntity> dbWhere = new DbWhere<TEntity>(RepositoryHelper.GetTablePrimaryKey(typeof(TEntity)), primaryKeyValue);
        return await ExistsAsync(new List<DbWhere<TEntity>> { dbWhere }, null, null, forceToMain);
    }

    /// <summary>
    /// 根据给定的条件，判断数据是否存在
    /// </summary>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>是否存在</returns>
    public abstract Task<bool> ExistsAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class;
    #endregion

    #region 执行自定义sql
    /// <summary>
    /// 执行存储过程
    /// </summary>
    /// <param name="storedProcedureName">存储过程名称</param>
    /// <param name="pars">参数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <returns>受影响的行数</returns>
    public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, DynamicParameters pars, bool forceToMain, IDbTransaction? transaction = null, int? commandTimeout = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(storedProcedureName))
            throw new ArgumentNullException(nameof(storedProcedureName));
        return useReplica ?
            await ReplicaConnection.ExecuteAsync(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure) :
            await Connection.ExecuteAsync(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure);
    }

    /// <summary>
    /// 执行sql语句，返回受影响的行数
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>受影响的行数</returns>
    public async Task<int> ExecuteAsync(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return
            useReplica ?
            await ReplicaConnection.ExecuteAsync(sqlText, pars, transaction, commandTimeout, commandType) :
            await Connection.ExecuteAsync(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public async Task<T> ExecuteScalarAsync<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            await ReplicaConnection.ExecuteScalarAsync<T>(sqlText, pars, transaction, commandTimeout, commandType) :
            await Connection.ExecuteScalarAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
    /// </summary>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public async Task<object> ExecuteScalarAsync(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            await ReplicaConnection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, commandType) :
            await Connection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行查询，返回结果集
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果集</returns>
    public async Task<IEnumerable<T>> QueryAsync<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            await ReplicaConnection.QueryAsync<T>(sqlText, pars, transaction, commandTimeout, commandType) :
            await Connection.QueryAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// 执行单行查询，返回结果
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sqlText">sql语句</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="pars">参数</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="commandType">执行类型，默认为CommandType.Text</param>
    /// <returns>查询结果</returns>
    public async Task<T> QueryFirstOrDefaultAsync<T>(string sqlText, bool forceToMain, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        if (string.IsNullOrWhiteSpace(sqlText))
            throw new ArgumentNullException(nameof(sqlText));
        return useReplica ?
            await ReplicaConnection.QueryFirstOrDefaultAsync<T>(sqlText, pars, transaction, commandTimeout, commandType) :
            await Connection.QueryFirstOrDefaultAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
    }
    #endregion
    #endregion

    #region 事务
    /// <summary>
    /// 开启主库事务
    /// </summary>
    /// <returns>事务</returns>
    public IDbTransaction BeginTransaction()
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
        return Connection.BeginTransaction();
    }

    /// <summary>
    /// 开启主库事务
    /// </summary>
    /// <param name="il">事务等级</param>
    /// <returns>事务</returns>
    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
        return Connection.BeginTransaction(il);
    }

    /// <summary>
    /// 执行主库通用事务
    /// </summary>
    /// <param name="actions">事务方法</param>
    /// <returns>是否成功</returns>
    public bool Execute(IEnumerable<Action<IDbTransaction>> actions)
    {
        bool success = false;
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
        using (IDbTransaction transaction = Connection.BeginTransaction())
        {
            try
            {
                foreach (var action in actions)
                {
                    action(transaction);
                }
                transaction.Commit();
                success = true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        return success;
    }

    /// <summary>
    /// 开启从库事务(若未启用从库，则返回主库事务)
    /// </summary>
    /// <returns>事务</returns>
    public IDbTransaction BeginReplicaTransaction()
    {
        if (ReplicaConnection != null)
        {
            if (ReplicaConnection.State == ConnectionState.Closed)
                ReplicaConnection.Open();
            return ReplicaConnection.BeginTransaction();
        }

        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
        return Connection.BeginTransaction();
    }

    /// <summary>
    /// 开启从库事务(若未启用从库，则返回主库事务)
    /// </summary>
    /// <param name="il">事务等级</param>
    /// <returns>事务</returns>
    public IDbTransaction BeginReplicaTransaction(IsolationLevel il)
    {
        if (ReplicaConnection != null)
        {
            if (ReplicaConnection.State == ConnectionState.Closed)
                ReplicaConnection.Open();
            return ReplicaConnection.BeginTransaction(il);
        }

        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
        return Connection.BeginTransaction(il);
    }

    /// <summary>
    /// 执行从库通用事务(若未启用从库，则返回主库事务)
    /// </summary>
    /// <param name="actions">事务方法</param>
    /// <returns>是否成功</returns>
    public bool ExecuteReplica(IEnumerable<Action<IDbTransaction>> actions)
    {
        bool success = false;

        if (ReplicaConnection != null)
        {
            if (ReplicaConnection.State == ConnectionState.Closed)
                ReplicaConnection.Open();
            using (IDbTransaction transaction = ReplicaConnection.BeginTransaction())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        action(transaction);
                    }
                    transaction.Commit();
                    success = true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        else
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            using (IDbTransaction transaction = Connection.BeginTransaction())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        action(transaction);
                    }
                    transaction.Commit();
                    success = true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        return success;
    }
    #endregion
}