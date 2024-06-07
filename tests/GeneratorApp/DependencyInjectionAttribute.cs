﻿using System;

namespace GeneratorApp
{
    /// <summary>
    /// 自动依赖注入特性，默认服务生命周期为 <see cref="LifetimeEnum.Scoped"/>
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
        public LifetimeEnum Lifetime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="lifetime">生命周期</param>
        public DependencyInjectionAttribute(LifetimeEnum lifetime = LifetimeEnum.Scoped)
        {
            Lifetime = lifetime;
        }
    }

    /// <summary>
    /// 服务生命周期
    /// </summary>
    public enum LifetimeEnum
    {
        /// <summary>
        /// Specifies that a single instance of the service will be created.
        /// </summary>
        Singleton,

        /// <summary>
        /// Specifies that a new instance of the service will be created for each scope.
        /// 
        /// In ASP.NET Core applications a scope is created around each server request.
        /// </summary>
        Scoped,

        /// <summary>
        /// Specifies that a new instance of the service will be created every time it is requested.
        /// </summary>
        Transient
    }
}