/***************************************************************************
 *GUID: 0b1dddd3-dcdf-478f-bb35-d71c914a26ca
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-09 17:53:07
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

namespace Newcats.DataAccess.Sqlite;

/// <summary>
/// Sqlite仓储接口,提供数据库访问能力,封装了基本的CRUD方法。
/// </summary>
/// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
public interface IRepository<TDbContext> : Core.IRepository<TDbContext> where TDbContext : Core.IDbContext
{
}