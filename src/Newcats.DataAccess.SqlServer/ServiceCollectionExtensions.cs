using Microsoft.Extensions.DependencyInjection;
using Newcats.DataAccess.Core;

namespace Newcats.DataAccess.SqlServer
{
    /// <summary>
    /// SqlServer服务扩展类
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加SqlServer数据访问服务
        /// </summary>
        /// <typeparam name="TDbContext">自定义的数据库上下文(一个数据库连接定义一个上下文，需要继承Newcats.DataAccess.SqlServer.DbContext)</typeparam>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="option">选项</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSqlServerDataAccess<TDbContext>(this IServiceCollection services, Action<DbContextOptions> option) where TDbContext : DbContextBase
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            services.AddOptions();
            services.Configure(option);
            services.AddScoped<TDbContext>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));//注册泛型仓储

            return services;
        }
    }
}