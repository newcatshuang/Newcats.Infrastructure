using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Newcats.DependencyInjection
{
    /// <summary>
    /// Microsoft.Extensions.DependencyInjection依赖注入扩展
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 依赖注入服务，自动扫描执行目录下的所有程序集，并且注册单例/作用域/瞬态依赖
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors</param>
        /// <returns>collection of service descriptors</returns>
        public static IServiceCollection AddReflectionDependencies(this IServiceCollection services)
        {
            //要排除的接口类型
            Type[] exceptTypes = new Type[] { typeof(ISingletonDependency), typeof(IScopedDependency), typeof(ITransientDependency), typeof(IDisposable) };

            IFind finder = new Finder();
            List<Assembly> assList = finder.GetAssemblies();//所有的程序集

            #region 注册单例依赖
            List<Type> singletonTypes = finder.Find<ISingletonDependency>(assList);//单例依赖
            if (singletonTypes != null && singletonTypes.Count > 0)
            {
                foreach (Type impType in singletonTypes)
                {
                    IEnumerable<Type> interfaceTypes = impType.GetTypeInfo().ImplementedInterfaces.Except(exceptTypes).Distinct();
                    foreach (Type interfaceType in interfaceTypes)
                    {
                        services.AddSingleton(interfaceType, impType);
                    }
                }
            }
            #endregion

            #region 注册作用域依赖
            List<Type> scopeTypes = finder.Find<IScopedDependency>(assList);//作用域依赖
            if (scopeTypes != null && scopeTypes.Count > 0)
            {
                foreach (Type impType in scopeTypes)
                {
                    IEnumerable<Type> interfaceTypes = impType.GetTypeInfo().ImplementedInterfaces.Except(exceptTypes).Distinct();
                    foreach (Type interfaceType in interfaceTypes)
                    {
                        services.AddScoped(interfaceType, impType);
                    }
                }
            }
            #endregion

            #region 注册瞬态依赖
            List<Type> transientTypes = finder.Find<ITransientDependency>(assList);//瞬态依赖
            if (transientTypes != null && transientTypes.Count > 0)
            {
                foreach (Type impType in transientTypes)
                {
                    IEnumerable<Type> interfaceTypes = impType.GetTypeInfo().ImplementedInterfaces.Except(exceptTypes).Distinct();
                    foreach (Type interfaceType in interfaceTypes)
                    {
                        services.AddTransient(interfaceType, impType);
                    }
                }
            }
            #endregion

            #region 解析依赖注册器
            List<Type> registrarTypes = finder.Find<IDependencyRegistrar>(assList);
            if (registrarTypes != null && registrarTypes.Count > 0)
            {
                var registrars = registrarTypes.Select(t => CreateInstance<IDependencyRegistrar>(t));
                foreach (var item in registrars)
                {
                    item.Register(services);
                }
            }
            #endregion

            return services;
        }

        /// <summary>
        /// 动态创建实例
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="parameters">传递给构造函数的参数</param>
        /// <returns>目标实例</returns>
        private static T CreateInstance<T>(Type type, params object[] parameters)
        {
            var obj = Activator.CreateInstance(type, parameters);
            if (obj == null)
                return default;
            if (obj is string && string.IsNullOrWhiteSpace(obj.ToString()))
                return default;
            Type tType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            try
            {
                if (obj is IConvertible)
                    return (T)Convert.ChangeType(obj, tType);
                return (T)obj;
            }
            catch
            {
                return default;
            }
        }
    }
}