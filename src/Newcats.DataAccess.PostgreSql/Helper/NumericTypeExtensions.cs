// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class NumericTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapSmallInt<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int16> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Smallint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapSmallInt<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int16?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Smallint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapInteger<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int32> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Integer);
        }

        internal static NpgSqlBulkCopy<TEntity> MapInteger<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int32?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Integer);
        }

        internal static NpgSqlBulkCopy<TEntity> MapBigInt<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int64> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Bigint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapBigInt<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Int64?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Bigint);
        }

        internal static NpgSqlBulkCopy<TEntity> MapNumeric<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Decimal> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Numeric);
        }

        internal static NpgSqlBulkCopy<TEntity> MapNumeric<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Decimal?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Numeric);
        }

        internal static NpgSqlBulkCopy<TEntity> MapReal<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Single> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Real);
        }

        internal static NpgSqlBulkCopy<TEntity> MapReal<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Single?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Real);
        }

        internal static NpgSqlBulkCopy<TEntity> MapDouble<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Double> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Double);
        }

        internal static NpgSqlBulkCopy<TEntity> MapDouble<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Double?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Double);
        }
    }
}