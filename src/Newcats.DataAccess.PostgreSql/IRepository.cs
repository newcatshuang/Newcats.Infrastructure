/***************************************************************************
 *GUID: f528c5f8-5334-4a96-81d9-cfea9306937d
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-05 22:19:10
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// PostgreSql仓储接口,提供数据库访问能力,封装了基本的CRUD方法。
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文，不同的数据库连接注册不同的DbContext</typeparam>
    public interface IRepository<TDbContext> : Core.IRepository<TDbContext> where TDbContext : Core.IDbContext
    {
    }
}