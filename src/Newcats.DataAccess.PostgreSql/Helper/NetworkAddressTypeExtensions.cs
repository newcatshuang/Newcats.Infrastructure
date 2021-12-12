// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.NetworkInformation;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    internal static class NetworkAddressTypeExtensions
    {
        internal static NpgSqlBulkCopy<TEntity> MapInetAddress<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, IPAddress> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.Inet);
        }

        internal static NpgSqlBulkCopy<TEntity> MapMacAddress<TEntity>(this NpgSqlBulkCopy<TEntity> helper, string columnName, Func<TEntity, PhysicalAddress> propertyGetter)
        {
            return helper.Map(columnName, propertyGetter, NpgsqlDbType.MacAddr);
        }
    }
}