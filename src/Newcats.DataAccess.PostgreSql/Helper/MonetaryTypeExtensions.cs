// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class MonetaryTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapMoney<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Decimal> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Money);
        }

        internal static NpgSqlBulkCopy<TEntity> MapMoney<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Decimal?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Money);
        }
    }
}