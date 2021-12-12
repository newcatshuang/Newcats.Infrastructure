// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Newcats.DataAccess.PostgreSql
{
    internal static class StringExtensions
    {
        internal static string GetIdentifier(this string identifier, bool usePostgresQuotes)
        {
            return usePostgresQuotes
                ? NpgsqlUtils.QuoteIdentifier(identifier)
                : identifier;
        }
    }
}