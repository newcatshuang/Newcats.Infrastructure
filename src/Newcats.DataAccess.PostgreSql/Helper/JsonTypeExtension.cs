// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class JsonTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapJson<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, string> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Json);
        }

        internal static NpgSqlBulkCopy<TEntity> MapJsonb<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, string> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Jsonb);
        }
    }
}