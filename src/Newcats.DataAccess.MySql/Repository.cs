﻿/***************************************************************************
 *GUID: 8ce722bd-9957-4d67-a0e3-6b7e05adaac0
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-11-26 14:47:41
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Dapper;
using MySqlConnector;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.MySql;

/// <summary>
/// 仓储实现类,提供数据库访问能力,封装了基本的CRUD方法。
/// </summary>
/// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
public class Repository<TDbContext> : Core.RepositoryBase<TDbContext>, MySql.IRepository<TDbContext> where TDbContext : Core.IDbContext
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    private readonly TDbContext _context;

    /// <summary>
    /// 主库数据库连接
    /// </summary>
    public override IDbConnection Connection
    {
        get
        {
            return _context.Connection;
        }
    }

    /// <summary>
    /// 从库数据库连接(不为null则表示启用了读写分离)
    /// </summary>
    public override IDbConnection? ReplicaConnection
    {
        get
        {
            return _context.ReplicaConnection;
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public Repository(TDbContext context)
    {
        _context = context;
    }

    #region 同步方法
    /// <summary>
    /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
    /// </summary>
    /// <param name="entity">要插入的数据实体</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
    public override object Insert<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));

        string sqlText = $"{RepositoryHelper.GetInsertSqlText(typeof(TEntity))} SELECT LAST_INSERT_ID();";
        return Connection.ExecuteScalar<object>(sqlText, entity, transaction, commandTimeout, CommandType.Text);
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
    public override int InsertSqlBulkCopy<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        MySqlConnection conn = (MySqlConnection)Connection;
        MySqlTransaction? trans = transaction == null ? null : (MySqlTransaction)transaction;
        if (conn.State == ConnectionState.Closed)
            conn.Open();
        MySqlBulkCopy copy = new(conn, trans);
        copy.DestinationTableName = RepositoryHelper.GetTableName(typeof(TEntity));
        copy.BulkCopyTimeout = commandTimeout ?? 0;

        var r = copy.WriteToServer(RepositoryHelper.ToDataTable(list));
        return r.RowsInserted;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public override (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        int totalCount = 0;
        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string fields = RepositoryHelper.GetTableFieldsQuery(type);
        string sqlText = string.Empty, sqlWhere = string.Empty, sqlOrderBy = string.Empty;
        DynamicParameters pars = new();
        if (dbWheres != null && dbWheres.Any())
            pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        if (!string.IsNullOrWhiteSpace(sqlOrderBy))
            sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
        if (pars == null)
            pars = new DynamicParameters();
        string sqlCount = $"SELECT COUNT(1) FROM {tableName} {sqlWhere};";
        if (pageSize <= 0)
        {
            sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
        }
        else
        {
            if (pageIndex <= 0)
            {
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageSize};";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sqlOrderBy))
                {
                    //sqlOrderBy = RepositoryHelper.GetTablePrimaryKey(type);
                    //sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                    sqlOrderBy = string.Empty;
                }
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageIndex * pageSize},{pageSize};";
            }
        }
        IEnumerable<TEntity> list = useReplica ?
            ReplicaConnection.Query<TEntity>(sqlText, pars, transaction, true, commandTimeout, CommandType.Text) :
            Connection.Query<TEntity>(sqlText, pars, transaction, true, commandTimeout, CommandType.Text);
        if (returnTotal.HasValue && returnTotal.Value)
        {
            totalCount = useReplica ?
                ReplicaConnection.ExecuteScalar<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text) :
                Connection.ExecuteScalar<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text);
        }
        return (list, totalCount);
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
    public override bool Exists<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        string sqlText = $" SELECT 1 FROM {tableName} WHERE 1=1 {sqlWhere} LIMIT 1;";
        object o = useReplica ?
            ReplicaConnection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, CommandType.Text) :
            Connection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, CommandType.Text);
        if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
            return true;
        return false;
    }
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
    public override async Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));

        string sqlText = $"{RepositoryHelper.GetInsertSqlText(typeof(TEntity))} SELECT LAST_INSERT_ID();";
        return await Connection.ExecuteScalarAsync<object>(sqlText, entity, transaction, commandTimeout, CommandType.Text);
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
    public override async Task<int> InsertSqlBulkCopyAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
    {
        MySqlConnection conn = (MySqlConnection)Connection;
        MySqlTransaction? trans = transaction == null ? null : (MySqlTransaction)transaction;
        if (conn.State == ConnectionState.Closed)
            await conn.OpenAsync();
        MySqlBulkCopy copy = new(conn, trans);
        copy.DestinationTableName = RepositoryHelper.GetTableName(typeof(TEntity));
        copy.BulkCopyTimeout = commandTimeout ?? 0;

        var r = await copy.WriteToServerAsync(RepositoryHelper.ToDataTable(list));
        return r.RowsInserted;
    }

    /// <summary>
    /// 根据给定的条件及排序，分页获取数据
    /// </summary>
    /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
    /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
    /// <param name="dbWheres">条件集合</param>
    /// <param name="transaction">事务</param>
    /// <param name="commandTimeout">超时时间(单位：秒)</param>
    /// <param name="returnTotal">是否查询总记录数</param>
    /// <param name="forceToMain">启用读写分离时，强制此方法使用主库</param>
    /// <param name="dbOrderBy">排序</param>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <returns>分页数据集合</returns>
    public override async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool? returnTotal = true, bool forceToMain = false, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        Type type = typeof(TEntity);
        string tableName = RepositoryHelper.GetTableName(type);
        string fields = RepositoryHelper.GetTableFieldsQuery(type);
        string sqlText = string.Empty, sqlWhere = string.Empty, sqlOrderBy = string.Empty;
        DynamicParameters pars = new();
        if (dbWheres != null && dbWheres.Any())
            pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
        if (!string.IsNullOrWhiteSpace(sqlWhere))
            sqlWhere = $" WHERE 1=1 {sqlWhere} ";
        if (!string.IsNullOrWhiteSpace(sqlOrderBy))
            sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
        if (pars == null)
            pars = new DynamicParameters();
        int totalCount = 0;
        string sqlCount = $"SELECT COUNT(1) FROM {tableName} {sqlWhere};";
        if (pageSize <= 0)
        {
            sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
        }
        else
        {
            if (pageIndex <= 0)
            {
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageSize};";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sqlOrderBy))
                {
                    //sqlOrderBy = RepositoryHelper.GetTablePrimaryKey(type);
                    //sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                    sqlOrderBy = string.Empty;
                }
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageIndex * pageSize},{pageSize};";
            }
        }
        IEnumerable<TEntity> list = useReplica ?
            await ReplicaConnection.QueryAsync<TEntity>(sqlText, pars, transaction, commandTimeout, CommandType.Text) :
            await Connection.QueryAsync<TEntity>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
        if (returnTotal.HasValue && returnTotal.Value)
        {
            totalCount = useReplica ?
                await ReplicaConnection.ExecuteScalarAsync<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text) :
                await Connection.ExecuteScalarAsync<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text);
        }
        return (list, totalCount);
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
    public override async Task<bool> ExistsAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, bool forceToMain = false) where TEntity : class
    {
        bool useReplica = ReplicaConnection != null && forceToMain == false;//useReplica=true表示使用从库

        string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
        string sqlWhere = string.Empty;
        DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
        string sqlText = $" SELECT 1 FROM {tableName} WHERE 1=1 {sqlWhere} LIMIT 1;";
        object o = useReplica ?
            await ReplicaConnection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, CommandType.Text) :
            await Connection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, CommandType.Text);
        if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
            return true;
        return false;
    }
    #endregion
}