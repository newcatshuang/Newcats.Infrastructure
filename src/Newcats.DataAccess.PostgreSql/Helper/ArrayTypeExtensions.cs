// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class ArrayTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, byte[][]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Bytea);
        }
        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int16[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Smallint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int32[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Integer);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int64[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Bigint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Decimal[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Numeric);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Single[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Real);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Double[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Double);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, String[]> propertyGetter)
        {
            return MapArray(helper, columnName, propertyGetter, NpgsqlDbType.Text);
        }

        internal static NpgSqlBulkCopy<TEntity> MapArray<TEntity, TProperty>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TProperty> propertyGetter, NpgsqlDbType type)
        {
            return helper.Map(columnName, propertyGetter, (NpgsqlDbType.Array | type));
        }
    }
}