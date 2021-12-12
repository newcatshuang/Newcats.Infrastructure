// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newcats.DataAccess.PostgreSql;

namespace Newcats.DataAccess.PostgreSql
{
    internal class TargetTable
    {
        public string SchemaName { get; internal set; }

        public string TableName { get; internal set; }

        public bool UsePostgresQuoting { get; internal set; }

        public IReadOnlyList<TargetColumn> Columns { get; internal set; }

        public string GetFullyQualifiedTableName()
        {
            return NpgsqlUtils.GetFullyQualifiedTableName(SchemaName, TableName, UsePostgresQuoting);
        }
    }
}