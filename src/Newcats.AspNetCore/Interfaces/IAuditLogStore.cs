using System.Threading.Tasks;
using Newcats.AspNetCore.Models;
using Newcats.DependencyInjection;

namespace Newcats.AspNetCore.Interfaces
{
    /// <summary>
    /// 审计日志对象持久化接口
    /// 默认实现为输出到Microsoft.Extensions.Logging日志
    /// 可以重写此方法，实现自定义持久化
    /// 建议使用消息队列，避免出现性能问题
    /// </summary>
    public interface IAuditLogStore : IScopedDependency
    {
        /// <summary>
        /// 保存审计日志对象
        /// </summary>
        /// <param name="model">审计日志</param>
        void Save(AuditLogModel model);

        /// <summary>
        /// 保存审计日志对象
        /// </summary>
        /// <param name="model">审计日志</param>
        Task SaveAsync(AuditLogModel model);
    }
}