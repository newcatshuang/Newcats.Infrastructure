/***************************************************************************
 *GUID: f020296b-9337-49d4-a81c-ec00836a52d7
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-05 23:11:41
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// PostgreSql服务扩展类
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加PostgreSql数据访问服务
        /// </summary>
        /// <typeparam name="TDbContext">自定义的数据库上下文(一个数据库连接定义一个上下文，需要继承Newcats.DataAccess.PostgreSql.DbContext)</typeparam>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="option">选项</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddPostgreSqlDataAccess<TDbContext>(this IServiceCollection services, Action<DbContextOptions> option) where TDbContext : DbContextBase
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
}