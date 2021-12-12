// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class BinaryDataTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapByteArray<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, byte[]> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Bytea);
        }
    }
}