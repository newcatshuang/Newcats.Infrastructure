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
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// PostgreSql的SqlBulkCopy实现
    /// </summary>
    public class NpgSqlBulkCopy : IDisposable
    {
        /// <summary>
        /// 目标表名(若包含架构，则为 Schema.TableName)
        /// </summary>
        public string? DestinationTableName { get; set; }

        /// <summary>
        /// 去除架构名的表名
        /// </summary>
        private string? shortName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DestinationTableName))
                    return DestinationTableName.Contains('.') ? DestinationTableName.Split('.').Last() : DestinationTableName;
                return string.Empty;
            }
        }

        /// <summary>
        /// 字段定义
        /// </summary>
        private List<FieldDefinition>? FieldDefinitions { get; set; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public NpgsqlConnection? Connection { get; set; }

        /// <summary>
        /// 数据库事务
        /// </summary>
        public NpgsqlTransaction? Transaction { get; set; }

        /// <summary>
        /// 构造函数 <see cref="NpgSqlBulkCopy"/>
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="destinationTableName">目标表名(若包含架构，则为 Schema.TableName)</param>
        /// <param name="transaction">数据库事务</param>
        public NpgSqlBulkCopy(NpgsqlConnection connection, string destinationTableName, NpgsqlTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(connection));
            ArgumentNullException.ThrowIfNull(nameof(destinationTableName));
            if (string.IsNullOrWhiteSpace(destinationTableName))
                throw new ArgumentNullException(nameof(destinationTableName));
            Connection = connection;
            Transaction = transaction;
            DestinationTableName = destinationTableName;
            GetFieldDefinitions();
        }

        /// <summary>
        /// 构建NpgsqlBinaryImporter
        /// </summary>
        /// <param name="table">数据源</param>
        /// <returns>NpgsqlBinaryImporter</returns>
        private NpgsqlBinaryImporter BuildImporter(DataTable table)
        {
            var colNames = table.Columns.OfType<DataColumn>().Select(c => c.ColumnName).ToArray();
            var colNameSegment = string.Join(',', colNames);

            var writer = Connection.BeginBinaryImport($"COPY {DestinationTableName} ({colNameSegment}) FROM STDIN (FORMAT BINARY)");

            foreach (DataRow dataRow in table.Rows)
            {
                writer.StartRow();
                //writer.WriteRow(dataRow);

                foreach (var colName in colNames)
                {
                    writer.Write(dataRow[colName], GetNpgFieldType(colName));
                }
            }
            return writer;
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <param name="table">数据源</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>写入的数量</returns>
        public ulong WriteToServer(DataTable table)
        {
            ArgumentNullException.ThrowIfNull(nameof(table));
            var tran = Transaction ?? Connection.BeginTransaction();
            ulong result = 0;
            using (tran)
            {
                try
                {
                    using (var writer = BuildImporter(table))
                    {
                        result = writer.Complete();
                        writer.Close();
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <param name="reader">数据源</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>写入的数量</returns>
        public ulong WriteToServer(IDataReader reader)
        {
            ArgumentNullException.ThrowIfNull(nameof(reader));
            var dt = new DataTable();
            dt.Load(reader);
            return WriteToServer(dt);
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <param name="reader">数据源</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>写入的数量</returns>
        public async Task<ulong> WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(reader));
            var dt = new DataTable();
            dt.Load(reader);
            return await WriteToServerAsync(dt, cancellationToken);
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>写入的数量</returns>
        public async Task<ulong> WriteToServerAsync(DataTable table, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(table));
            var tran = Transaction ?? await Connection.BeginTransactionAsync(cancellationToken);
            ulong result;

            try
            {
                await using (var writer = BuildImporter(table))
                {
                    result = await writer.CompleteAsync(cancellationToken);
                    await writer.CloseAsync(cancellationToken);
                }

                await tran.CommitAsync(cancellationToken);
            }
            catch
            {
                await tran.RollbackAsync(cancellationToken);
                throw;
            }

            return result;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// 获取所有的字段定义
        /// </summary>
        /// <returns></returns>
        private void GetFieldDefinitions()
        {
            ArgumentNullException.ThrowIfNull(nameof(shortName));
            string sql = @$"
            SELECT a.attname AS FieldName, t.typname AS FieldType,a.attnotnull AS NotNull, b.description AS Description
            FROM 
            pg_class c, pg_attribute a
            LEFT JOIN pg_description b ON a.attrelid = b.objoid
            AND a.attnum = b.objsubid, pg_type t
            WHERE c.relname = '{shortName}' AND a.attnum > 0 AND a.attrelid = c.oid AND a.atttypid = t.oid ORDER BY a.attnum;";//此处shortName是大小写敏感的
            this.FieldDefinitions = Connection.Query<FieldDefinition>(sql).ToList();
        }

        private NpgsqlDbType GetNpgFieldType(string fieldName)
        {
            string fieldType = FieldDefinitions.FirstOrDefault(r => r.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).FieldType;
            var dbTypes = NpgsqlTypeHelper.GetAllNpgsqlTypes(typeof(NpgsqlDbType));
            return dbTypes.First(r => r.PostgresType.Equals(fieldType, StringComparison.OrdinalIgnoreCase)).NpgType;
        }

        /// <summary>
        /// 字段定义
        /// </summary>
        private class FieldDefinition
        {
            //const string sql = @"
            //SELECT a.attname AS FieldName, t.typname AS FieldType,a.attnotnull AS NotNull, b.description AS Description
            //FROM 
            //pg_class c, pg_attribute a
            //LEFT JOIN pg_description b ON a.attrelid = b.objoid
            //AND a.attnum = b.objsubid, pg_type t
            //WHERE c.relname = 'userinfo' AND a.attnum > 0 AND a.attrelid = c.oid AND a.atttypid = t.oid ORDER BY a.attnum;";

            /// <summary>
            /// 字段名
            /// </summary>
            public string FieldName { get; set; }

            /// <summary>
            /// 字段类型
            /// </summary>
            public string FieldType { get; set; }

            /// <summary>
            /// 是否不可null
            /// </summary>
            public bool NotNull { get; set; }

            /// <summary>
            /// 字段注释
            /// </summary>
            public string? Description { get; set; }
        }
    }
}