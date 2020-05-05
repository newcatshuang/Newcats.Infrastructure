using System;

namespace Newcats.AspNetCore.Attributes
{
    /// <summary>
    /// 标注当前Action是否需要记录审计日志
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AuditableAttribute : Attribute
    {
    }
}