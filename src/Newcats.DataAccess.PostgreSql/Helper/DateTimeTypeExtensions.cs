// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class DateTimeTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapDate<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Date);
        }

        internal static NpgSqlBulkCopy<TEntity> MapDate<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Date);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTime<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Time);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTime<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Time);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStamp<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Timestamp);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStamp<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Timestamp);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStampTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStampTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStampTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeStampTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapInterval<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Interval);
        }

        internal static NpgSqlBulkCopy<TEntity> MapInterval<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Interval);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }

        internal static NpgSqlBulkCopy<TEntity> MapTimeTz<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.TimeTz);
        }
    }
}