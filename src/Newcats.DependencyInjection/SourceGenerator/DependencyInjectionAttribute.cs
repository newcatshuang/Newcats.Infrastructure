using Microsoft.Extensions.DependencyInjection;

namespace Newcats.DependencyInjection
{
    /// <summary>
    /// 自动依赖注入特性，默认服务生命周期为 <see cref="ServiceLifetime.Scoped"/>
    /// 
    /// <code>
    /// 标记到实现类上，会在编译时自动生成接口(若有)和实现类的注册代码，例如：
    /// 
    /// service.AddScoped&lt;IOrderService, OrderService>();
    /// 
    /// service.AddTransient&lt;UserService>();
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DependencyInjectionAttribute : Attribute
    {
        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lifetime">生命周期</param>
        public DependencyInjectionAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            Lifetime = lifetime;
        }
    }
}