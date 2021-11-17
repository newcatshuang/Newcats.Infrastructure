using Dapper;
using Microsoft.Data.SqlClient;
using Newcats.DataAccess.Core;
using System.Data;

namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// 仓储实现类,提供数据库访问能力,封装了基本的CRUD方法。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
    public class Repository<TDbContext> : IRepository<TDbContext> where TDbContext : IDbContext
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly TDbContext _context;

        /// <summary>
        /// 数据库连接
        /// </summary>
        public virtual IDbConnection Connection
        {
            get { return _context.Connection; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
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
        public object Insert<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            string sqlText = $"{RepositoryHelper.GetInsertSqlText<TEntity>()} SELECT SCOPE_IDENTITY();";
            return Connection.ExecuteScalar<object>(sqlText, entity, transaction, commandTimeout, CommandType.Text);
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

            string sqlText = RepositoryHelper.GetInsertSqlText<TEntity>();
            return Connection.Execute(sqlText, list, transaction, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 通过SqlBulkCopy批量插入数据，返回成功的条数
        /// (此方法性能最优)
        /// </summary>
        /// <param name="list">要插入的数据实体集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>成功的条数</returns>
        public int InsertSqlBulkCopy<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            SqlBulkCopy copy = null;
            if (transaction == null)
                copy = new SqlBulkCopy((SqlConnection)Connection);
            else
                copy = new SqlBulkCopy((SqlConnection)Connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction);
            using (copy)
            {
                copy.DestinationTableName = RepositoryHelper.GetTableName<TEntity>();
                copy.BulkCopyTimeout = commandTimeout ?? 0;
                copy.BatchSize = list.Count();
                copy.WriteToServer(RepositoryHelper.ToDataTable(list));
                return copy.RowsCopied;
            }
        }

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

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
            string tableName = RepositoryHelper.GetTableName<TEntity>();
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

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
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
            string tableName = RepositoryHelper.GetTableName<TEntity>();
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

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public TEntity Get<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
        public TEntity Get<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
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
        public (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            int totalCount = 0;
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlText = string.Empty, sqlWhere = string.Empty, sqlOrderBy = string.Empty;
            DynamicParameters pars = new DynamicParameters();
            if (dbWheres != null && dbWheres.Any())
                pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            if (pars == null)
                pars = new DynamicParameters();
            pars.Add("@Row_Count", totalCount, DbType.Int32, ParameterDirection.Output);
            if (pageSize <= 0)
            {
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere};";
            }
            else
            {
                if (pageIndex <= 0)
                {
                    sqlText = $" SELECT TOP {pageSize} {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(sqlOrderBy))
                    {
                        //sqlOrderBy = RepositoryHelper.GetTablePrimaryKey<TEntity>();
                        //sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                        sqlOrderBy = string.Empty;
                    }
                    //sqlText = $" SELECT * FROM(SELECT TOP {((pageIndex + 1) * pageSize)} ROW_NUMBER() OVER({sqlOrderBy}) RowNumber_Index,{fields} FROM {tableName} {sqlWhere}) temTab1 WHERE RowNumber_Index > {(pageIndex * pageSize)} ORDER BY RowNumber_Index ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} OFFSET {(pageIndex * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
            }
            IEnumerable<TEntity> list = Connection.Query<TEntity>(sqlText, pars, transaction, true, commandTimeout, CommandType.Text);
            totalCount = pars.Get<int?>("@Row_Count") ?? 0;
            return (list, totalCount);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageInfo">分页信息</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据</returns>
        public (IEnumerable<TEntity> list, int totalCount) GetPage<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            return GetPage(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, pageInfo.OrderBy?.ToArray());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据集合</returns>
        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            var (list, _) = GetPage<TEntity>(0, 0, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取所有数据
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据集合</returns>
        public IEnumerable<TEntity> GetAll<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            var (list, _) = GetPage(0, 0, dbWheres, transaction, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 根据默认排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>指定数量的数据集合</returns>
        public IEnumerable<TEntity> GetTop<TEntity>(int top) where TEntity : class
        {
            var (list, _) = GetPage<TEntity>(0, top, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据集合</returns>
        public IEnumerable<TEntity> GetTop<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            var (list, _) = GetPage(0, top, dbWheres, transaction, commandTimeout, dbOrderBy);
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
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>记录数量</returns>
        public int Count<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            if (dbWheres != null && dbWheres.Any())
            {
                string sqlWhere = string.Empty;
                DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
                string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
                return Connection.ExecuteScalar<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            }
            else
            {
                string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
                return Connection.ExecuteScalar<int>(sqlText, null, transaction, commandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public bool Exists<TEntity>(object primaryKeyValue) where TEntity : class
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
        public bool Exists<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            object o = Connection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcedureName">存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteStoredProcedure(string storedProcedureName, DynamicParameters pars, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));
            return Connection.Execute(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>受影响的行数</returns>
        public int Execute(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.Execute(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public T ExecuteScalar<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.ExecuteScalar<T>(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public object ExecuteScalar(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.ExecuteScalar(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public IEnumerable<T> Query<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.Query<T>(sqlText, pars, transaction, true, commandTimeout, commandType);
        }

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
        public T QueryFirstOrDefault<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.QueryFirstOrDefault<T>(sqlText, pars, transaction, commandTimeout, commandType);
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
        public async Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            string sqlText = $"{RepositoryHelper.GetInsertSqlText<TEntity>()} SELECT SCOPE_IDENTITY();";
            return await Connection.ExecuteScalarAsync<object>(sqlText, entity, transaction, commandTimeout, CommandType.Text);
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

            string sqlText = RepositoryHelper.GetInsertSqlText<TEntity>();
            return await Connection.ExecuteAsync(sqlText, list, transaction, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 通过SqlBulkCopy批量插入数据，返回成功的条数
        /// (此方法性能最优)
        /// </summary>
        /// <param name="list">要插入的数据实体集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>成功的条数</returns>
        public async Task<int> InsertSqlBulkCopyAsync<TEntity>(IEnumerable<TEntity> list, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            SqlBulkCopy copy = null;
            if (transaction == null)
                copy = new SqlBulkCopy((SqlConnection)Connection);
            else
                copy = new SqlBulkCopy((SqlConnection)Connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction);
            using (copy)
            {
                copy.DestinationTableName = RepositoryHelper.GetTableName<TEntity>();
                copy.BulkCopyTimeout = commandTimeout ?? 0;
                copy.BatchSize = list.Count();
                await copy.WriteToServerAsync(RepositoryHelper.ToDataTable(list));
                return copy.RowsCopied;
            }
        }

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

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
            string tableName = RepositoryHelper.GetTableName<TEntity>();
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

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
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
            string tableName = RepositoryHelper.GetTableName<TEntity>();
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

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据库实体或null</returns>
        public async Task<TEntity> GetAsync<TEntity>(object primaryKeyValue, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
        public async Task<TEntity> GetAsync<TEntity>(IEnumerable<DbWhere<TEntity>> dbWheres, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
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
        public async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string fields = RepositoryHelper.GetTableFieldsQuery<TEntity>();
            string sqlText = string.Empty, sqlWhere = string.Empty, sqlOrderBy = string.Empty;
            DynamicParameters pars = new DynamicParameters();
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
            pars.Add("@Row_Count", totalCount, DbType.Int32, ParameterDirection.Output);
            if (pageSize <= 0)
            {
                sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
            }
            else
            {
                if (pageIndex <= 0)
                {
                    sqlText = $" SELECT TOP {pageSize} {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(sqlOrderBy))
                    {
                        //sqlOrderBy = RepositoryHelper.GetTablePrimaryKey<TEntity>();
                        //sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                        sqlOrderBy = string.Empty;
                    }
                    //sqlText = $" SELECT * FROM(SELECT TOP {((pageIndex + 1) * pageSize)} ROW_NUMBER() OVER({sqlOrderBy}) RowNumber_Index,{fields} FROM {tableName} {sqlWhere}) temTab1 WHERE RowNumber_Index > {(pageIndex * pageSize)} ORDER BY RowNumber_Index ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} OFFSET {(pageIndex * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
            }
            IEnumerable<TEntity> list = await Connection.QueryAsync<TEntity>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            totalCount = pars.Get<int?>("@Row_Count") ?? 0;
            return (list, totalCount);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageInfo">分页信息</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>分页数据</returns>
        public async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync<TEntity>(PageInfo<TEntity> pageInfo, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            return await GetPageAsync(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, transaction, commandTimeout, pageInfo.OrderBy?.ToArray());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class
        {
            var (list, _) = await GetPageAsync<TEntity>(0, 0, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取所有数据
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            var (list, _) = await GetPageAsync(0, 0, dbWheres, transaction, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 根据默认排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>指定数量的数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top) where TEntity : class
        {
            var (list, _) = await GetPageAsync<TEntity>(0, top, null, null, null);
            return list;
        }

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
        public async Task<IEnumerable<TEntity>> GetTopAsync<TEntity>(int top, IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy) where TEntity : class
        {
            var (list, _) = await GetPageAsync(0, top, dbWheres, transaction, commandTimeout, dbOrderBy);
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
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>记录数量</returns>
        public async Task<int> CountAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            if (dbWheres != null && dbWheres.Any())
            {
                string sqlWhere = string.Empty;
                DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
                string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
                return await Connection.ExecuteScalarAsync<int>(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            }
            else
            {
                string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
                return await Connection.ExecuteScalarAsync<int>(sqlText, null, transaction, commandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <typeparam name="TEntity">数据库实体类</typeparam>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsAsync<TEntity>(object primaryKeyValue) where TEntity : class
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string pkName = RepositoryHelper.GetTablePrimaryKey<TEntity>();
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
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
        public async Task<bool> ExistsAsync<TEntity>(IEnumerable<DbWhere<TEntity>>? dbWheres = null, IDbTransaction? transaction = null, int? commandTimeout = null) where TEntity : class
        {
            string tableName = RepositoryHelper.GetTableName<TEntity>();
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            object o = await Connection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcedureName">存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, DynamicParameters pars, IDbTransaction? transaction = null, int? commandTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));
            return await Connection.ExecuteAsync(storedProcedureName, pars, transaction, commandTimeout, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteAsync(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteAsync(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public async Task<T> ExecuteScalarAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteScalarAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public async Task<object> ExecuteScalarAsync(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteScalarAsync(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public async Task<IEnumerable<T>> QueryAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.QueryAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
        }

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
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sqlText, DynamicParameters? pars = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.QueryFirstOrDefaultAsync<T>(sqlText, pars, transaction, commandTimeout, commandType);
        }
        #endregion

        #region 事务
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>事务</returns>
        public IDbTransaction BeginTransaction()
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            return Connection.BeginTransaction();
        }

        /// <summary>
        /// 开启事务
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
        /// 执行通用事务
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
        #endregion
    }
}