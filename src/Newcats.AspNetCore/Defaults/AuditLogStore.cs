using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newcats.AspNetCore.Abstractions;
using Newcats.AspNetCore.Models;

namespace Newcats.AspNetCore.Defaults
{
    /// <summary>
    /// 审计日志对象持久化接口的默认实现(输出到Microsoft.Extensions.Logging日志)
    /// 可以重写此方法，实现自定义持久化
    /// 建议使用消息队列，避免出现性能问题
    /// </summary>
    public class AuditLogStore : IAuditLogStore
    {
        private readonly ILogger<AuditLogStore> _logger;

        public AuditLogStore(ILogger<AuditLogStore> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 保存审计日志对象
        /// </summary>
        /// <param name="model">审计日志</param>
        public virtual void Save(AuditLogModel model)
        {
            _logger.LogInformation(model.ToJson());
        }

        /// <summary>
        /// 保存审计日志对象
        /// </summary>
        /// <param name="model">审计日志</param>
        public virtual Task SaveAsync(AuditLogModel model)
        {
            _logger.LogInformation(model.ToJson());
            return Task.CompletedTask;
        }
    }
}