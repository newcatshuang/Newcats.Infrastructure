using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Newcats.DependencyInjection
{
    /// <summary>
    /// 需要生成注册代码的接口和类
    /// </summary>
    internal class RegisterDependencyModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implementationType">实现类类型</param>
        /// <param name="lifetime">生命周期</param>
        public RegisterDependencyModel(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType, ServiceLifetime lifetime)
        {
            InterfaceType = interfaceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        /// <summary>
        /// 接口类型
        /// </summary>
        public INamedTypeSymbol InterfaceType { get; set; }

        /// <summary>
        /// 实现类类型
        /// </summary>
        public INamedTypeSymbol ImplementationType { get; set; }

        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }
    }
}