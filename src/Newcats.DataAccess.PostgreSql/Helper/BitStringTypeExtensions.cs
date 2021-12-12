// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class BitStringTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapBit<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, bool> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Bit);
        }

        internal static NpgSqlBulkCopy<TEntity> MapBit<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, bool?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Bit);
        }
    }
}