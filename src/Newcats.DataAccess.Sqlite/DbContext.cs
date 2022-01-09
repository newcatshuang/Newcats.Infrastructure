/***************************************************************************
 *GUID: 1194387c-eab1-442c-9775-852bd15218fd
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-09 17:45:03
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.Sqlite;

/// <summary>
/// Sqlite数据库上下文
/// </summary>
public class DbContext : DbContextBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="optionsAccessor">配置项</param>
    public DbContext(IOptions<DbContextOptions> optionsAccessor) : base(optionsAccessor)
    {
    }

    /// <summary>
    /// 创建数据库连接的Sqlite实现
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接</returns>
    public override IDbConnection CreateConnection(string connectionString)
    {
        return SqliteFactory.Instance.CreateConnection();
    }
}