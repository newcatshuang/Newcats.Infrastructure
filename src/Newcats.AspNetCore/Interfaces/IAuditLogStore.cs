using System.Threading.Tasks;
using Newcats.AspNetCore.Models;
using Newcats.DenpendencyInjection;

namespace Newcats.AspNetCore.Interfaces
{
    /// <summary>
    /// 审计日志对象持久化接口
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