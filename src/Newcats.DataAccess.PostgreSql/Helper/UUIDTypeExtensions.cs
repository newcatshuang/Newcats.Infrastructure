// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class UUIDTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapUUID<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Guid> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Uuid);
        }

        internal static NpgSqlBulkCopy<TEntity> MapUUID<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, Guid?> propertyGetter)
        {
            return helper.MapNullable(columnName, propertyGetter, NpgsqlDbType.Uuid);
        }
    }
}