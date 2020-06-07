using System;
using System.ComponentModel.DataAnnotations;

namespace Newcats.AspNetCore.Models
{
    /// <summary>
    /// 审计日志
    /// </summary>
    public class AuditLogModel
    {
        /// <summary>
        /// 主键Id，主键/自增
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 当前操作人Id(未登录时则为string.Empty)
        /// </summary>
        public string CurrentUserId { get; set; }

        /// <summary>
        /// 完整的方法名(控制器+方法)
        /// </summary>
        [MaxLength(128)]
        public string Action { get; set; }

        /// <summary>
        /// HttpMethod
        /// </summary>
        [MaxLength(16)]
        public string HttpMethod { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [MaxLength(64)]
        public string IP { get; set; }

        /// <summary>
        /// 执行耗时(单位:ms)
        /// </summary>
        public int ExecuteDuration { get; set; }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime ExecuteTime { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [MaxLength(1024)]
        public string Arguments { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        [MaxLength(128)]
        public string Exception { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        [MaxLength(2048)]
        public string Result { get; set; }
    }
}