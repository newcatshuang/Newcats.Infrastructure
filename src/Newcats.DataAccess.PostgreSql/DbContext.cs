/***************************************************************************
 *GUID: bededa8c-f3e2-4a49-adb4-8eb22d6b700c
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-05 22:08:50
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Microsoft.Extensions.Options;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// PostgreSql数据库上下文
    /// </summary>
    public class DbContext : DbContextBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor">配置项</param>
        protected DbContext(IOptions<DbContextOptions> optionsAccessor) : base(optionsAccessor)
        {
        }

        /// <summary>
        /// 创建数据库连接的MySql实现
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>数据库连接</returns>
        protected override IDbConnection CreateConnection(string connectionString)
        {
            return Npgsql.NpgsqlFactory.Instance.CreateConnection();
        }
    }
}