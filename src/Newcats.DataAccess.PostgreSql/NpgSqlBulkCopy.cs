/***************************************************************************
 *GUID: 46459171-2f55-4981-9a5e-dc1ad036da99
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-12 18:18:55
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Newcats.DataAccess.Core;
using Npgsql;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// PostgreSql的SqlBulkCopy实现
    /// 参考 https://github.com/PostgreSQLCopyHelper/PostgreSQLCopyHelper
    /// </summary>
    public class NpgSqlBulkCopy<TEntity>
    {
        #region 字段
        private bool _usePostgresQuoting;

        private readonly TableDefinition _table;

        private readonly List<ColumnDefinition<TEntity>> _columns;

        private NpgsqlConnection _connection;

        private string _schemaTableName;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">NpgsqlConnection</param>
        /// <param name="usePostgresQuoting">是否使用谓词</param>
        public NpgSqlBulkCopy(NpgsqlConnection connection, bool usePostgresQuoting = true)
        {
            _connection = connection;
            _schemaTableName = RepositoryHelper.GetTableName(typeof(TEntity));
            _usePostgresQuoting = false;//usePostgresQuoting;
            _table = new TableDefinition
            {
                Schema = _schemaTableName.Contains('.') ? _schemaTableName.Split('.').First() : string.Empty,
                TableName = _schemaTableName.Contains('.') ? _schemaTableName.Split('.').Last() : _schemaTableName
            };
            _columns = new List<ColumnDefinition<TEntity>>();
        }

        #region 公有方法
        /// <summary>
        /// 使用PostgreSql的copy命令写入数据库
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>成功的数量</returns>
        public ulong WriteToServer(IEnumerable<TEntity> entities)
        {
            using (NoSynchronizationContextScope.Enter())
            {
                return DoSaveAllAsync(_connection, entities, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 使用PostgreSql的copy命令写入数据库
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功的数量</returns>
        public async ValueTask<ulong> WriteToServerAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return await new ValueTask<ulong>(Task.FromCanceled<ulong>(cancellationToken));
            }

            using (NoSynchronizationContextScope.Enter())
            {
                return await DoSaveAllAsync(_connection, entities, cancellationToken);
            }
        }

        /// <summary>
        /// 使用PostgreSql的copy命令写入数据库
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功的数量</returns>
        public async ValueTask<ulong> WriteToServerAsync(IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return await new ValueTask<ulong>(Task.FromCanceled<ulong>(cancellationToken));
            }

            using (NoSynchronizationContextScope.Enter())
            {
                return await DoSaveAllAsync(_connection, entities, cancellationToken);
            }
        }
        #endregion

        #region internal方法
        internal NpgSqlBulkCopy<TEntity> Map<TProperty>(string columnName, Func<TEntity, TProperty> propertyGetter)
        {
            return AddColumn(columnName, (writer, entity, cancellationToken) => writer.WriteAsync(propertyGetter(entity), cancellationToken), clrType: typeof(TProperty));
        }


        internal NpgSqlBulkCopy<TEntity> Map<TProperty>(string columnName, Func<TEntity, TProperty> propertyGetter, NpgsqlDbType dbType)
        {
            return AddColumn(columnName, (writer, entity, cancellationToken) => writer.WriteAsync(propertyGetter(entity), dbType, cancellationToken), dbType, typeof(TProperty));
        }

        internal NpgSqlBulkCopy<TEntity> Map<TProperty>(string columnName, Func<TEntity, TProperty> propertyGetter, string dataTypeName)
        {
            return AddColumn(columnName, (writer, entity, cancellationToken) => writer.WriteAsync(propertyGetter(entity), dataTypeName, cancellationToken), clrType: typeof(TProperty), dataTypeName: dataTypeName);
        }

        internal NpgSqlBulkCopy<TEntity> MapNullable<TProperty>(string columnName, Func<TEntity, TProperty?> propertyGetter, NpgsqlDbType dbType)
            where TProperty : struct
        {
            return AddColumn(columnName, async (writer, entity, cancellationToken) =>
            {
                var val = propertyGetter(entity);

                if (!val.HasValue)
                {
                    await writer.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await writer.WriteAsync(val.Value, dbType, cancellationToken);
                }
            }, dbType, typeof(TProperty));
        }
        #endregion

        #region 私有方法
        private async ValueTask<ulong> DoSaveAllAsync(NpgsqlConnection connection, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await using var binaryCopyWriter = connection.BeginBinaryImport(GetCopyCommand());
            await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken);

            return await binaryCopyWriter.CompleteAsync(cancellationToken);
        }

        private async ValueTask<ulong> DoSaveAllAsync(NpgsqlConnection connection, IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await using var binaryCopyWriter = connection.BeginBinaryImport(GetCopyCommand());
            await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken);

            return await binaryCopyWriter.CompleteAsync(cancellationToken);
        }

        private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                await WriteToStreamAsync(writer, entity, cancellationToken);
            }
        }

        private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await foreach (var entity in entities.WithCancellation(cancellationToken))
            {
                await WriteToStreamAsync(writer, entity, cancellationToken);
            }
        }

        private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, TEntity entity, CancellationToken cancellationToken)
        {
            await writer.StartRowAsync(cancellationToken);

            foreach (var columnDefinition in _columns)
            {
                await columnDefinition.WriteAsync(writer, entity, cancellationToken);
            }
        }

        private NpgSqlBulkCopy<TEntity> AddColumn(string columnName, Func<NpgsqlBinaryImporter, TEntity, CancellationToken, Task> action, NpgsqlDbType? dbType = default, Type clrType = default, string dataTypeName = default)
        {
            _columns.Add(new ColumnDefinition<TEntity>
            {
                ColumnName = columnName,
                DbType = dbType,
                DataTypeName = dataTypeName,
                ClrType = clrType,
                WriteAsync = action
            });

            return this;
        }

        private string GetCopyCommand()
        {
            var commaSeparatedColumns = string.Join(", ", _columns.Select(x => x.ColumnName.GetIdentifier(_usePostgresQuoting)));

            commaSeparatedColumns = RepositoryHelper.GetTableFieldsInsert(typeof(TEntity));
            var r = $"COPY {_table.GetFullyQualifiedTableName(_usePostgresQuoting)}({commaSeparatedColumns}) FROM STDIN BINARY;";
            //COPY UserInfo(Id,Name,CreateTime) FROM STDIN BINARY;
            return r;
        }
        #endregion
    }


}

#region 参考
//using System;
//using System.Data;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Cosmos.Text;
//using Npgsql;

//namespace Cosmos.Data.Sx.SqlBulkCopy
//{
//    public class NpgSqlBulkCopy : IDisposable
//    {
//        private NpgsqlConnection _conn;
//        private NpgsqlTransaction _externalTransaction { get; set; }

//        /// <summary>
//        /// Set to TRUE if the BulkCopy object was not instantiated with an external OracleConnection
//        /// and thus it is up to the BulkCopy object to open and close connections
//        /// </summary>
//        private bool _ownsTheConnection;

//        /// <summary>
//        /// Create a new instance of <see cref="NpgSqlBulkCopy"/>
//        /// </summary>
//        /// <param name="connectionString"></param>
//        public NpgSqlBulkCopy(string connectionString) : this(new NpgsqlConnection(connectionString))
//        {
//            _ownsTheConnection = true;
//        }

//        /// <summary>
//        /// Create a new instance of <see cref="NpgSqlBulkCopy"/>
//        /// </summary>
//        /// <param name="connection"></param>
//        public NpgSqlBulkCopy(NpgsqlConnection connection) : this(connection, null) { }

//        /// <summary>
//        /// Create a new instance of <see cref="NpgSqlBulkCopy"/>
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="transaction"></param>
//        public NpgSqlBulkCopy(NpgsqlConnection connection, NpgsqlTransaction transaction = null)
//        {
//            _conn = connection;
//            _externalTransaction = transaction;
//        }

//        #region TableName

//        private string _destinationTableName;

//        /// <summary>
//        /// Destination TableName
//        /// </summary>
//        /// <exception cref="ArgumentException"></exception>
//        public string DestinationTableName
//        {
//            get => _destinationTableName;
//            set
//            {
//                if (string.IsNullOrEmpty(value))
//                    throw new ArgumentException("Destination Table Name cannot be null or empty string");
//                _destinationTableName = value;
//            }
//        }

//        private string GetTableName(DataTable table)
//        {
//            if (DestinationTableName.IsNullOrWhiteSpace())
//                return table.TableName;
//            return DestinationTableName;
//        }

//        #endregion

//        #region Internal

//        private NpgsqlBinaryImporter BuildImporter(DataTable table)
//        {
//            var colNames = table.Columns.OfType<DataColumn>().Select(c => c.ColumnName).ToArray();
//            var colNameSegment = Joiners.Joiner.On(',').Join(colNames);
//            var writer = _conn.BeginBinaryImport($"COPY {GetTableName(table)} ({colNameSegment}) FROM STDIN (FORMAT BINARY)");

//            foreach (DataRow dataRow in table.Rows)
//            {
//                writer.StartRow();
//                foreach (var colName in colNames)
//                {
//                    writer.Write(dataRow[colName]);
//                }
//            }

//            return writer;
//        }

//        #endregion

//        #region WriteToServer

//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="table"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public void WriteToServer(DataTable table)
//        {
//            if (table is null)
//                throw new ArgumentNullException(nameof(table));

//            var needAutoCloseConn = _conn.OpenIfNeeded();
//            var tx = _externalTransaction ?? _conn.BeginTransaction();

//            using (tx)
//            {
//                try
//                {
//                    using (var writer = BuildImporter(table))
//                    {
//                        writer.Complete();
//                        writer.Close();
//                    }

//                    tx.Commit();
//                }
//                catch
//                {
//                    tx.Rollback();
//                    throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="reader"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public void WriteToServer(IDataReader reader)
//        {
//            if (reader is null)
//                throw new ArgumentNullException(nameof(reader));

//            WriteToServer(reader.ToDataTable());
//        }

//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="reader"></param>
//        /// <param name="cancellationToken"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public async Task WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken = default)
//        {
//            if (reader is null)
//                throw new ArgumentNullException(nameof(reader));

//            await WriteToServerAsync(reader.ToDataTable(), cancellationToken);
//        }

//#if NET452
//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="table"></param>
//        /// <param name="cancellationToken"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public async Task WriteToServerAsync(DataTable table, CancellationToken cancellationToken = default)
//        {
//            if (table is null)
//                throw new ArgumentNullException(nameof(table));

//            var needAutoCloseConn = await _conn.OpenIfNeededAsync(cancellationToken);
//            var tx = _externalTransaction ?? _conn.BeginTransaction();

//            try
//            {
//                using (var writer = BuildImporter(table))
//                {
//                    writer.Complete();
//                    writer.Close();
//                }

//                await tx.CommitAsync(cancellationToken);
//            }
//            catch
//            {
//                await tx.RollbackAsync(cancellationToken);
//                throw;
//            }

//            await _conn.CloseIfNeededAsync(needAutoCloseConn);
//        }
//#elif NETFRAMEWORK
//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="table"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public async Task WriteToServerAsync(DataTable table, CancellationToken cancellationToken = default)
//        {
//            if (table is null)
//                throw new ArgumentNullException(nameof(table));

//            var needAutoCloseConn = await _conn.OpenIfNeededAsync(cancellationToken);
//            var tx = _externalTransaction ?? _conn.BeginTransaction();

//            try
//            {
//                await using (var writer = BuildImporter(table))
//                {
//                    await writer.CompleteAsync(cancellationToken);
//                    await writer.CloseAsync(cancellationToken);
//                }

//                await tx.CommitAsync(cancellationToken);
//            }
//            catch
//            {
//                await tx.RollbackAsync(cancellationToken);
//                throw;
//            }

//            await _conn.CloseIfNeededAsync(needAutoCloseConn);
//        }
//#else
//        /// <summary>
//        /// Write to server
//        /// </summary>
//        /// <param name="table"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public async Task WriteToServerAsync(DataTable table, CancellationToken cancellationToken = default)
//        {
//            if (table is null)
//                throw new ArgumentNullException(nameof(table));

//            var needAutoCloseConn = await _conn.OpenIfNeededAsync(cancellationToken);
//            var tx = _externalTransaction ?? await _conn.BeginTransactionAsync(cancellationToken);

//            try
//            {
//                await using (var writer = BuildImporter(table))
//                {
//                    await writer.CompleteAsync(cancellationToken);
//                    await writer.CloseAsync(cancellationToken);
//                }

//                await tx.CommitAsync(cancellationToken);
//            }
//            catch
//            {
//                await tx.RollbackAsync(cancellationToken);
//                throw;
//            }

//            await _conn.CloseIfNeededAsync(needAutoCloseConn);
//        }
//#endif

//        #endregion

//        #region Dispose

//        /// <summary>
//        /// Close and database connections.
//        /// </summary>
//        public void Close()
//        {
//            Dispose();
//        }

//        /// <summary>
//        /// Releases all resources used by the current instance of the SqliteBulkCopy class.
//        /// </summary>
//        public void Dispose()
//        {
//            if (_conn is not null)
//            {
//                // Only close the connection if the BulkCopy instance owns the connection
//                if (_ownsTheConnection)
//                    _conn.Dispose();

//                // Always set to null
//                _conn = null;
//            }
//        }

//        #endregion
//    }
//} 
#endregion