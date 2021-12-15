/***************************************************************************
 *GUID: fc16d20f-0693-4e49-99ae-cda3114857b4
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-05 22:21:41
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Dapper;
using Newcats.DataAccess.Core;
using Npgsql;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// 仓储实现类,提供数据库访问能力,封装了基本的CRUD方法。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
    public class Repository<TDbContext> : Core.RepositoryBase<TDbContext>, PostgreSql.IRepository<TDbContext> where TDbContext : Core.IDbContext
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly TDbContext _context;

        /// <summary>
        /// 数据库连接
        /// </summary>
        public override IDbConnection Connection
        {
            get
            {
                return _context.Connection;
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

            Type entityType = typeof(TEntity);

            string sqlText = $"{RepositoryHelper.GetInsertSqlText(entityType).Replace(";", "")} RETURNING {RepositoryHelper.GetTablePrimaryKey(entityType)};";
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
            NpgsqlConnection conn = (NpgsqlConnection)Connection;
            NpgsqlTransaction? tran = transaction == null ? null : (NpgsqlTransaction)transaction;
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            using (NpgSqlBulkCopy copy = new(conn, RepositoryHelper.GetTableName(typeof(TEntity)), tran))
            {
                ulong r = copy.WriteToServer(RepositoryHelper.ToDataTable(list));
                return Convert.ToInt32(r);
            }
        }

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public override TEntity Get<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(nameof(primaryKeyValue));

            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string pkName = RepositoryHelper.GetTablePrimaryKey(type);
            string fields = RepositoryHelper.GetTableFieldsQuery(type);
            string sqlText = $" SELECT {fields} FROM {tableName} WHERE {pkName}=@p_1 LIMIT 1;";
            DynamicParameters parameters = new();
            parameters.Add("@p_1", primaryKeyValue);
            return Connection.QueryFirstOrDefault<TEntity>(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，获取一条记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序集合</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public override TEntity Get<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string fields = RepositoryHelper.GetTableFieldsQuery(type);
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT 1;";
            return Connection.QueryFirstOrDefault<TEntity>(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
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
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据集合</returns>
        public override (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
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
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageSize} OFFSET {pageSize * pageIndex};";
                }
            }
            IEnumerable<TEntity> list = Connection.Query<TEntity>(sqlText, pars, transaction, true, commandTimeout, CommandType.Text);
            totalCount = Connection.ExecuteScalar<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text);
            return (list, totalCount);
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public override bool Exists<TEntity>(object primaryKeyValue) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(nameof(primaryKeyValue));
            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string pkName = RepositoryHelper.GetTablePrimaryKey(type);
            string sqlText = $" SELECT 1 FROM {tableName} WHERE {pkName}=@p_1 LIMIT 1;";
            DynamicParameters parameters = new();
            parameters.Add("@p_1", primaryKeyValue);
            object o = Connection.ExecuteScalar(sqlText, parameters, null, null, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 根据给定的条件，判断数据是否存在
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public override bool Exists<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT 1 FROM {tableName} WHERE 1=1 {sqlWhere} LIMIT 1;";
            object o = Connection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, CommandType.Text);
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

            Type entityType = typeof(TEntity);

            string sqlText = $"{RepositoryHelper.GetInsertSqlText(entityType).Replace(";", "")} RETURNING {RepositoryHelper.GetTablePrimaryKey(entityType)};";
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
            NpgsqlConnection conn = (NpgsqlConnection)Connection;
            NpgsqlTransaction? tran = transaction == null ? null : (NpgsqlTransaction)transaction;
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            using (NpgSqlBulkCopy copy = new(conn, RepositoryHelper.GetTableName(typeof(TEntity)), tran))
            {
                ulong r = await copy.WriteToServerAsync(RepositoryHelper.ToDataTable(list));
                return Convert.ToInt32(r);
            }
        }

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public override async Task<TEntity> GetAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(nameof(primaryKeyValue));

            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string pkName = RepositoryHelper.GetTablePrimaryKey(type);
            string fields = RepositoryHelper.GetTableFieldsQuery(type);
            string sqlText = $" SELECT {fields} FROM {tableName} WHERE {pkName}=@p_1 LIMIT 1;";
            DynamicParameters parameters = new();
            parameters.Add("@p_1", primaryKeyValue);
            return await Connection.QueryFirstOrDefaultAsync<TEntity>(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，获取一条记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序集合</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public override async Task<TEntity> GetAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string fields = RepositoryHelper.GetTableFieldsQuery(type);
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT 1;";
            return await Connection.QueryFirstOrDefaultAsync<TEntity>(sqlText, parameters, transaction, commandTimeout, CommandType.Text);
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
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据集合</returns>
        public override async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
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
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} LIMIT {pageSize} OFFSET {pageSize * pageIndex};";
                }
            }
            IEnumerable<TEntity> list = await Connection.QueryAsync<TEntity>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            totalCount = await Connection.ExecuteScalarAsync<int>(sqlCount, pars, transaction, commandTimeout, CommandType.Text);
            return (list, totalCount);
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public override async Task<bool> ExistsAsync<TEntity>(object primaryKeyValue) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(nameof(primaryKeyValue));
            Type type = typeof(TEntity);
            string tableName = RepositoryHelper.GetTableName(type);
            string pkName = RepositoryHelper.GetTablePrimaryKey(type);
            string sqlText = $" SELECT 1 FROM {tableName} WHERE {pkName}=@p_1 LIMIT 1;";
            DynamicParameters parameters = new();
            parameters.Add("@p_1", primaryKeyValue);
            object o = await Connection.ExecuteScalarAsync(sqlText, parameters, null, null, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 根据给定的条件，判断数据是否存在
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public override async Task<bool> ExistsAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName(typeof(TEntity));
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT 1 FROM {tableName} WHERE 1=1 {sqlWhere} LIMIT 1;";
            object o = await Connection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }
        #endregion
    }
}