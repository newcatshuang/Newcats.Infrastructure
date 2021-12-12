// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class BooleanTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapBoolean<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, bool> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Boolean);
        }

        internal static NpgSqlBulkCopy<TEntity> MapBoolean<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, bool?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Boolean);
        }
    }
}