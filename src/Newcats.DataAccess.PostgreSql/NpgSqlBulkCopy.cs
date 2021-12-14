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
using Npgsql;
using NpgsqlTypes;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

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
        /// <param name="transaction">数据库事务</param>
        public NpgSqlBulkCopy(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(connection));
            Connection = connection;
            Transaction = transaction;
            DestinationTableName = string.Empty;
        }

        /// <summary>
        /// 构造函数 <see cref="NpgSqlBulkCopy"/>
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="destinationTableName">目标表名(若包含架构，则为 Schema.TableName)</param>
        /// <param name="transaction">数据库事务</param>
        public NpgSqlBulkCopy(NpgsqlConnection connection, string destinationTableName, NpgsqlTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(connection));
            Connection = connection;
            Transaction = transaction;
            DestinationTableName = destinationTableName;
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
                    writer.Write(dataRow[colName]);//TODO:此处需要明确的PgsqlDbType
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
    }
}


//var dbtype = column.DbTypeText;
//var isarray = dbtype.EndsWith("[]");
//if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
//NpgsqlDbType ret = NpgsqlDbType.Unknown;
//switch (dbtype.ToLower().TrimStart('_'))
//{
//    case "int2": ret = NpgsqlDbType.Smallint; break;
//    case "int4": ret = NpgsqlDbType.Integer; break;
//    case "int8": ret = NpgsqlDbType.Bigint; break;
//    case "numeric": ret = NpgsqlDbType.Numeric; break;
//    case "float4": ret = NpgsqlDbType.Real; break;
//    case "float8": ret = NpgsqlDbType.Double; break;
//    case "money": ret = NpgsqlDbType.Money; break;

//    case "bpchar": ret = NpgsqlDbType.Char; break;
//    case "varchar": ret = NpgsqlDbType.Varchar; break;
//    case "text": ret = NpgsqlDbType.Text; break;

//    case "timestamp": ret = NpgsqlDbType.Timestamp; break;
//    case "timestamptz": ret = NpgsqlDbType.TimestampTz; break;
//    case "date": ret = NpgsqlDbType.Date; break;
//    case "time": ret = NpgsqlDbType.Time; break;
//    case "timetz": ret = NpgsqlDbType.TimeTz; break;
//    case "interval": ret = NpgsqlDbType.Interval; break;

//    case "bool": ret = NpgsqlDbType.Boolean; break;
//    case "bytea": ret = NpgsqlDbType.Bytea; break;
//    case "bit": ret = NpgsqlDbType.Bit; break;
//    case "varbit": ret = NpgsqlDbType.Varbit; break;

//    case "point": ret = NpgsqlDbType.Point; break;
//    case "line": ret = NpgsqlDbType.Line; break;
//    case "lseg": ret = NpgsqlDbType.LSeg; break;
//    case "box": ret = NpgsqlDbType.Box; break;
//    case "path": ret = NpgsqlDbType.Path; break;
//    case "polygon": ret = NpgsqlDbType.Polygon; break;
//    case "circle": ret = NpgsqlDbType.Circle; break;

//    case "cidr": ret = NpgsqlDbType.Cidr; break;
//    case "inet": ret = NpgsqlDbType.Inet; break;
//    case "macaddr": ret = NpgsqlDbType.MacAddr; break;

//    case "json": ret = NpgsqlDbType.Json; break;
//    case "jsonb": ret = NpgsqlDbType.Jsonb; break;
//    case "uuid": ret = NpgsqlDbType.Uuid; break;

//    case "int4range": ret = NpgsqlDbType.Range | NpgsqlDbType.Integer; break;
//    case "int8range": ret = NpgsqlDbType.Range | NpgsqlDbType.Bigint; break;
//    case "numrange": ret = NpgsqlDbType.Range | NpgsqlDbType.Numeric; break;
//    case "tsrange": ret = NpgsqlDbType.Range | NpgsqlDbType.Timestamp; break;
//    case "tstzrange": ret = NpgsqlDbType.Range | NpgsqlDbType.TimestampTz; break;
//    case "daterange": ret = NpgsqlDbType.Range | NpgsqlDbType.Date; break;

//    case "hstore": ret = NpgsqlDbType.Hstore; break;
//    case "geometry": ret = NpgsqlDbType.Geometry; break;
//}
//return isarray ? (ret | NpgsqlDbType.Array) : ret;