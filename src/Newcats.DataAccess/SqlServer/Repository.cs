using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// 1.仓储实现类,提供数据库访问能力,封装了基本的CRUD方法。
    /// 2.若要使用非默认的数据库连接，请重新给Connection属性赋值。
    /// 3.默认在Newcats.DependencyInjection里注册了作用域泛型仓储类
    /// _builder.RegisterGeneric(typeof(DataAccess.Dapper.Repository<,>)).As(typeof(DataAccess.Dapper.IRepository<,>)).InstancePerLifetimeScope();
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    /// <typeparam name="TPrimaryKey">此数据库实体类的主键类型</typeparam>
    public class Repository<TDbContext, TEntity, TPrimaryKey> : IRepository<TDbContext, TEntity, TPrimaryKey> where TEntity : class where TDbContext : DbContextBase
    {

        private readonly TDbContext _context;

        public virtual IDbConnection Connection
        {
            get { return _context.Connection; }
        }
        //private readonly IConfiguration _configuration;

        public Repository(TDbContext context)
        {
            _context = context;
            EntityType = typeof(TEntity);
        }

        #region 数据库连接/配置
        /// <summary>
        /// 构造函数，初始化Connection/EntityType属性并赋值
        /// </summary>
        //public Repository(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    Connection = CreateDbConnection();
        //    EntityType = typeof(TEntity);
        //}

        /// <summary>
        /// 1.数据库连接,在构造函数初始化(默认连接为"DefaultConnection")。
        /// 2.若要使用非默认的数据库连接，请重新赋值。
        /// 3.一般在Service类的构造函数赋值_repository.Connection=_repository.CreateDbConnection("OtherDB")。
        /// </summary>
        //public IDbConnection Connection { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        private Type EntityType { get; set; }

        /// <summary>
        /// 1.根据应用程序执行目录下的appsettings.json配置文件(默认ConnectionStrings:DefaultConnection)的连接字符串创建数据库连接
        /// 2.会在构造函数自动调用并赋值，不需要手动调用，除非需要使用非默认的数据库连接
        /// </summary>
        /// <param name="key">连接字符串名，默认为"DefaultConnection"</param>
        /// <returns>数据库连接</returns>
        //public IDbConnection CreateDbConnection(string key = "DefaultConnection")
        //{
        //    if (!key.Equals("DefaultConnection", StringComparison.OrdinalIgnoreCase) && Connection != null)
        //    {
        //        Connection.Close();
        //        Connection.Dispose();
        //    }
        //    var connectionInstance = SqlClientFactory.Instance.CreateConnection();
        //    connectionInstance.ConnectionString = GetConnectionString(key);
        //    return connectionInstance;
        //}

        /// <summary>
        /// 1.获取应用程序执行目录下的appsettings.json配置文件(默认ConnectionStrings:DefaultConnection)里的连接字符串
        /// 2.此处有缓存，如果更改了配置文件，请重新启动应用程序
        /// </summary>
        /// <param name="key">连接字符串名称</param>
        /// <returns>解密之后的连接字符串</returns>
        //private string GetConnectionString(string key)
        //{
        //    string connStr = RepositoryHelper.GetConnectionString(key);
        //    if (!string.IsNullOrWhiteSpace(connStr))
        //        return connStr;

        //    string connStrConfig = _configuration.GetValue<string>("DefaultConnection");//  Utils.Helper.ConfigHelper.AppSettings.GetConnectionString(key);
        //    if (string.IsNullOrWhiteSpace(connStrConfig))
        //    {
        //        throw new KeyNotFoundException($"The config item ConnectionStrings:{key} do not exists on file appsettings.json");
        //    }
        //    connStr = connStrConfig; //Utils.Encrypt.EncryptHelper.DESDecrypt(connStrConfig, "123456");
        //    RepositoryHelper.SetConnectionString(key, connStr);
        //    return connStr;
        //}
        #endregion

        #region 同步方法
        /// <summary>
        /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
        /// </summary>
        /// <param name="entity">要插入的数据实体</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
        public TPrimaryKey Insert(TEntity entity, int? commandTimeout = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            string sqlText = $"{RepositoryHelper.GetInsertSqlText(EntityType)} SELECT SCOPE_IDENTITY();";
            return Connection.ExecuteScalar<TPrimaryKey>(sqlText, entity, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 批量插入数据，返回成功的条数
        /// </summary>
        /// <param name="list">要插入的数据实体集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public int InsertBulk(IEnumerable<TEntity> list, int? commandTimeout = null)
        {
            if (list == null || !list.Any())
                throw new ArgumentNullException(nameof(list));

            string sqlText = RepositoryHelper.GetInsertSqlText(EntityType);
            return Connection.Execute(sqlText, list, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，删除一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public int Delete(TPrimaryKey primaryKeyValue, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@p_1", primaryKeyValue);
            return Connection.Execute(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，删除记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public int Delete(IEnumerable<DbWhere<TEntity>> dbWheres, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlText = $" DELETE FROM {tableName} {sqlWhere} ;";
            return Connection.Execute(sqlText, pars, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，更新一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="dbUpdates">要更新的字段集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public int Update(TPrimaryKey primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            if (dbUpdates == null || !dbUpdates.Any())
                throw new ArgumentNullException(nameof(dbUpdates));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string sqlUpdate = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
            parameters.Add("@" + pkName, primaryKeyValue);
            string sqlText = $" UPDATE {tableName} SET {sqlUpdate} WHERE {pkName}=@{pkName} ;";
            return Connection.Execute(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，更新记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="dbUpdates">要更新的字段集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public int Update(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, int? commandTimeout = null)
        {
            if (dbUpdates == null || !dbUpdates.Any())
                throw new ArgumentNullException(nameof(dbUpdates));
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters wherePars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlUpdate = string.Empty;
            DynamicParameters updatePars = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
            wherePars.AddDynamicParams(updatePars);
            string sqlText = $" UPDATE {tableName} SET {sqlUpdate} {sqlWhere} ;";
            return Connection.Execute(sqlText, wherePars, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>数据库实体或null</returns>
        public TEntity Get(TPrimaryKey primaryKeyValue, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@p_1", primaryKeyValue);
            return Connection.QueryFirstOrDefault<TEntity>(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，获取一条记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序集合</param>
        /// <returns>数据库实体或null</returns>
        public TEntity Get(IEnumerable<DbWhere<TEntity>> dbWheres, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
            return Connection.QueryFirstOrDefault<TEntity>(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件及排序，分页获取数据
        /// </summary>
        /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
        /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>分页数据集合</returns>
        public (IEnumerable<TEntity> list, int totalCount) GetPage(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            int totalCount = 0;
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
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
                        sqlOrderBy = RepositoryHelper.GetTablePrimaryKey(EntityType);
                        sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                    }
                    //sqlText = $" SELECT * FROM(SELECT TOP {((pageIndex + 1) * pageSize)} ROW_NUMBER() OVER({sqlOrderBy}) RowNumber_Index,{fields} FROM {tableName} {sqlWhere}) temTab1 WHERE RowNumber_Index > {(pageIndex * pageSize)} ORDER BY RowNumber_Index ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} OFFSET {(pageIndex * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
            }
            IEnumerable<TEntity> list = Connection.Query<TEntity>(sqlText, pars, null, true, commandTimeout, CommandType.Text);
            totalCount = pars.Get<int?>("@Row_Count") ?? 0;
            return (list, totalCount);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageInfo">分页信息</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>分页数据</returns>
        public (IEnumerable<TEntity> list, int totalCount) GetPage(PageInfo<TEntity> pageInfo, int? commandTimeout = null)
        {
            return GetPage(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, commandTimeout, pageInfo.OrderBy?.ToArray());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns>数据集合</returns>
        public IEnumerable<TEntity> GetAll()
        {
            var (list, totalCount) = GetPage(0, 0, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取所有数据
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>分页数据集合</returns>
        public IEnumerable<TEntity> GetAll(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            var (list, totalCount) = GetPage(0, 0, dbWheres, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 根据默认排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <returns>指定数量的数据集合</returns>
        public IEnumerable<TEntity> GetTop(int top)
        {
            var (list, totalCount) = GetPage(0, top, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>分页数据集合</returns>
        public IEnumerable<TEntity> GetTop(int top, IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            var (list, totalCount) = GetPage(0, top, dbWheres, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 获取记录总数量
        /// </summary>
        /// <returns>记录总数量</returns>
        public int Count()
        {
            return Count(null, null);
        }

        /// <summary>
        /// 根据给定的条件，获取记录数量
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>记录数量</returns>
        public int Count(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            if (dbWheres != null && dbWheres.Any())
            {
                string sqlWhere = string.Empty;
                DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
                string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
                return Connection.ExecuteScalar<int>(sqlText, pars, null, commandTimeout, CommandType.Text);
            }
            else
            {
                string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
                return Connection.ExecuteScalar<int>(sqlText, null, null, commandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <returns>是否存在</returns>
        public bool Exists(TPrimaryKey primaryKeyValue)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
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
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>是否存在</returns>
        public bool Exists(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            object o = Connection.ExecuteScalar(sqlText, pars, null, commandTimeout, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcedureName">存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteStoredProcedure(string storedProcedureName, DynamicParameters pars, int? commandTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));
            return Connection.Execute(storedProcedureName, pars, null, commandTimeout, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>受影响的行数</returns>
        public int Execute(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.Execute(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public T ExecuteScalar<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.ExecuteScalar<T>(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public object ExecuteScalar(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.ExecuteScalar(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，返回结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果集</returns>
        public IEnumerable<T> Query<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.Query<T>(sqlText, pars, null, true, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行单行查询，返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public T QueryFirstOrDefault<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return Connection.QueryFirstOrDefault<T>(sqlText, pars, null, commandTimeout, commandType);
        }
        #endregion

        #region 异步方法
        /// <summary>
        /// 插入一条数据，成功时返回当前主键的值，否则返回主键类型的默认值
        /// </summary>
        /// <param name="entity">要插入的数据实体</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功时返回当前主键的值，否则返回主键类型的默认值</returns>
        public async Task<TPrimaryKey> InsertAsync(TEntity entity, int? commandTimeout = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            string sqlText = $"{RepositoryHelper.GetInsertSqlText(EntityType)} SELECT SCOPE_IDENTITY();";
            return await Connection.ExecuteScalarAsync<TPrimaryKey>(sqlText, entity, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 批量插入数据，返回成功的条数
        /// </summary>
        /// <param name="list">要插入的数据实体集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public async Task<int> InsertBulkAsync(IEnumerable<TEntity> list, int? commandTimeout = null)
        {
            if (list == null || !list.Any())
                throw new ArgumentNullException(nameof(list));

            string sqlText = RepositoryHelper.GetInsertSqlText(EntityType);
            return await Connection.ExecuteAsync(sqlText, list, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，删除一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public async Task<int> DeleteAsync(TPrimaryKey primaryKeyValue, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string sqlText = $" DELETE FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@p_1", primaryKeyValue);
            return await Connection.ExecuteAsync(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，删除记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public async Task<int> DeleteAsync(IEnumerable<DbWhere<TEntity>> dbWheres, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlText = $" DELETE FROM {tableName} {sqlWhere} ;";
            return await Connection.ExecuteAsync(sqlText, pars, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，更新一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="dbUpdates">要更新的字段集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public async Task<int> UpdateAsync(TPrimaryKey primaryKeyValue, IEnumerable<DbUpdate<TEntity>> dbUpdates, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            if (dbUpdates == null || !dbUpdates.Any())
                throw new ArgumentNullException(nameof(dbUpdates));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string sqlUpdate = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
            parameters.Add("@" + pkName, primaryKeyValue);
            string sqlText = $" UPDATE {tableName} SET {sqlUpdate} WHERE {pkName}=@{pkName} ;";
            return await Connection.ExecuteAsync(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，更新记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="dbUpdates">要更新的字段集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>成功的条数</returns>
        public async Task<int> UpdateAsync(IEnumerable<DbWhere<TEntity>> dbWheres, IEnumerable<DbUpdate<TEntity>> dbUpdates, int? commandTimeout = null)
        {
            if (dbUpdates == null || !dbUpdates.Any())
                throw new ArgumentNullException(nameof(dbUpdates));
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters wherePars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlUpdate = string.Empty;
            DynamicParameters updatePars = SqlBuilder.GetUpdateDynamicParameter(dbUpdates, ref sqlUpdate);
            wherePars.AddDynamicParams(updatePars);
            string sqlText = $" UPDATE {tableName} SET {sqlUpdate} {sqlWhere} ;";
            return await Connection.ExecuteAsync(sqlText, wherePars, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据主键，获取一条记录
        /// </summary>
        /// <param name="primaryKeyValue">主键的值</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>数据库实体或null</returns>
        public async Task<TEntity> GetAsync(TPrimaryKey primaryKeyValue, int? commandTimeout = null)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));

            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} WHERE {pkName}=@p_1 ;";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@p_1", primaryKeyValue);
            return await Connection.QueryFirstOrDefaultAsync<TEntity>(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件，获取一条记录
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序集合</param>
        /// <returns>数据库实体或null</returns>
        public async Task<TEntity> GetAsync(IEnumerable<DbWhere<TEntity>> dbWheres, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters parameters = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            if (!string.IsNullOrWhiteSpace(sqlWhere))
                sqlWhere = $" WHERE 1=1 {sqlWhere} ";
            string sqlOrderBy = SqlBuilder.GetOrderBySql(dbOrderBy);
            if (!string.IsNullOrWhiteSpace(sqlOrderBy))
                sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
            string sqlText = $" SELECT TOP 1 {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} ;";
            return await Connection.QueryFirstOrDefaultAsync<TEntity>(sqlText, parameters, null, commandTimeout, CommandType.Text);
        }

        /// <summary>
        /// 根据给定的条件及排序，分页获取数据
        /// </summary>
        /// <param name="pageIndex">页码索引（从0开始）（pageIndex小于等于0，返回第0页数据）</param>
        /// <param name="pageSize">页大小(pageSize小于等于0，返回所有数据)</param>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>分页数据集合</returns>
        public async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync(int pageIndex, int pageSize, IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string fields = RepositoryHelper.GetTableFieldsQuery(EntityType);
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
                        sqlOrderBy = RepositoryHelper.GetTablePrimaryKey(EntityType);
                        sqlOrderBy = $" ORDER BY {sqlOrderBy} ";
                    }
                    //sqlText = $" SELECT * FROM(SELECT TOP {((pageIndex + 1) * pageSize)} ROW_NUMBER() OVER({sqlOrderBy}) RowNumber_Index,{fields} FROM {tableName} {sqlWhere}) temTab1 WHERE RowNumber_Index > {(pageIndex * pageSize)} ORDER BY RowNumber_Index ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                    sqlText = $" SELECT {fields} FROM {tableName} {sqlWhere} {sqlOrderBy} OFFSET {(pageIndex * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY ; SELECT @Row_Count=COUNT(1) FROM {tableName} {sqlWhere} ;";
                }
            }
            IEnumerable<TEntity> list = await Connection.QueryAsync<TEntity>(sqlText, pars, null, commandTimeout, CommandType.Text);
            totalCount = pars.Get<int?>("@Row_Count") ?? 0;
            return (list, totalCount);
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageInfo">分页信息</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>分页数据</returns>
        public async Task<(IEnumerable<TEntity> list, int totalCount)> GetPageAsync(PageInfo<TEntity> pageInfo, int? commandTimeout = null)
        {
            return await GetPageAsync(pageInfo.PageIndex, pageInfo.PageSize, pageInfo.Where, commandTimeout, pageInfo.OrderBy?.ToArray());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns>数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var (list, totals) = await GetPageAsync(0, 0, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取所有数据
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            var (list, totals) = await GetPageAsync(0, 0, dbWheres, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 根据默认排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <returns>指定数量的数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetTopAsync(int top)
        {
            var (list, totals) = await GetPageAsync(0, top, null, null, null);
            return list;
        }

        /// <summary>
        /// 根据给定的条件及排序，获取指定数量的数据
        /// </summary>
        /// <param name="top">指定数量</param>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="dbOrderBy">排序</param>
        /// <returns>指定数量的数据集合</returns>
        public async Task<IEnumerable<TEntity>> GetTopAsync(int top, IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null, params DbOrderBy<TEntity>[] dbOrderBy)
        {
            var (list, totals) = await GetPageAsync(0, top, dbWheres, commandTimeout, dbOrderBy);
            return list;
        }

        /// <summary>
        /// 获取记录总数量
        /// </summary>
        /// <returns>记录总数量</returns>
        public async Task<int> CountAsync()
        {
            return await CountAsync(null, null);
        }

        /// <summary>
        /// 根据给定的条件，获取记录数量
        /// </summary>
        /// <param name="dbWheres">条件集合</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>记录数量</returns>
        public async Task<int> CountAsync(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            if (dbWheres != null && dbWheres.Any())
            {
                string sqlWhere = string.Empty;
                DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
                string sqlText = $" SELECT COUNT(1) FROM {tableName} WHERE 1=1 {sqlWhere} ;";
                return await Connection.ExecuteScalarAsync<int>(sqlText, pars, null, commandTimeout, CommandType.Text);
            }
            else
            {
                string sqlText = $" SELECT COUNT(1) FROM {tableName} ;";
                return await Connection.ExecuteScalarAsync<int>(sqlText, null, null, commandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 根据主键，判断数据是否存在
        /// </summary>
        /// <param name="primaryKeyValue">主键值</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsAsync(TPrimaryKey primaryKeyValue)
        {
            if (primaryKeyValue == null)
                throw new ArgumentNullException(nameof(primaryKeyValue));
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string pkName = RepositoryHelper.GetTablePrimaryKey(EntityType);
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
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsAsync(IEnumerable<DbWhere<TEntity>> dbWheres = null, int? commandTimeout = null)
        {
            string tableName = RepositoryHelper.GetTableName(EntityType);
            string sqlWhere = string.Empty;
            DynamicParameters pars = SqlBuilder.GetWhereDynamicParameter(dbWheres, ref sqlWhere);
            string sqlText = $" SELECT TOP 1 1 FROM {tableName} WHERE 1=1 {sqlWhere} ;";
            object o = await Connection.ExecuteScalarAsync(sqlText, pars, null, commandTimeout, CommandType.Text);
            if (o != null && o != DBNull.Value && Convert.ToInt32(o) == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcedureName">存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, DynamicParameters pars, int? commandTimeout = null)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));
            return await Connection.ExecuteAsync(storedProcedureName, pars, null, commandTimeout, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql语句，返回受影响的行数
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteAsync(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteAsync(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public async Task<T> ExecuteScalarAsync<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteScalarAsync<T>(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public async Task<object> ExecuteScalarAsync(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.ExecuteScalarAsync(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询，返回结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果集</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.QueryAsync<T>(sqlText, pars, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行单行查询，返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sqlText">sql语句</param>
        /// <param name="pars">参数</param>
        /// <param name="commandTimeout">超时时间(单位：秒)</param>
        /// <param name="commandType">执行类型，默认为CommandType.Text</param>
        /// <returns>查询结果</returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sqlText, DynamicParameters pars = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrWhiteSpace(sqlText))
                throw new ArgumentNullException(nameof(sqlText));
            return await Connection.QueryFirstOrDefaultAsync<T>(sqlText, pars, null, commandTimeout, commandType);
        }
        #endregion

        public bool Execute(IEnumerable<Action<IDbTransaction>> actions)
        {
            bool success = false;
            Connection.Open();
            using (IDbTransaction transaction = _context.GetTransaction())
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
    }
}