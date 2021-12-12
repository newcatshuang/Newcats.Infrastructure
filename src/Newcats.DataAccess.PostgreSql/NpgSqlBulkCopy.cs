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