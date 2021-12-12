// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class CharacterTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapVarchar<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, String> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Varchar);
        }

        internal static NpgSqlBulkCopy<TEntity> MapCharacter<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, String> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Char);
        }

        internal static NpgSqlBulkCopy<TEntity> MapText<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, String> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Text);
        }
    }
}