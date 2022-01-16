/***************************************************************************
 *GUID: c97de527-f688-4708-8793-adf782f2ce8c
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-10-19 23:43:23
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

using System.Data;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 主库数据库连接
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 从库数据库连接
        /// </summary>
        IDbConnection? ReplicaConnection { get; }
    }
}