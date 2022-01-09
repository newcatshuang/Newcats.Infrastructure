/***************************************************************************
 *GUID: 8052679e-5b3c-410e-9775-4ad3e7087aed
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-09 17:55:28
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.Sqlite;

/// <summary>
/// Sqlite服务扩展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Sqlite数据访问服务
    /// </summary>
    /// <typeparam name="TDbContext">自定义的数据库上下文(一个数据库连接定义一个上下文，需要继承Newcats.DataAccess.Sqlite.DbContext)</typeparam>
    /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
    /// <param name="option">选项</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSqliteDataAccess<TDbContext>(this IServiceCollection services, Action<DbContextOptions> option) where TDbContext : DbContextBase
    {
        ArgumentNullException.ThrowIfNull(nameof(services));
        ArgumentNullException.ThrowIfNull(nameof(option));

        services.AddOptions();
        services.Configure(option);
        services.AddScoped<TDbContext>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));//注册泛型仓储

        return services;
    }
}