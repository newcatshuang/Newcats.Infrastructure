using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newcats.AspNetCore.Abstractions;
using Newcats.AspNetCore.Models;
using System.Diagnostics;

namespace Newcats.AspNetCore.Filters
{
    /// <summary>
    /// 启用审计日志
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AuditAttribute : Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 启用审计日志
        /// </summary>
        /// <param name="ignore">是否忽略</param>
        public AuditAttribute(bool ignore = false)
        {
            Ignore = ignore;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // do something before the action executes
            Stopwatch stopwatch = new();
            stopwatch.Start();

            ActionExecutedContext resultContext = await next();//执行Action

            // do something after the action executes; resultContext.Result will be set
            stopwatch.Stop();
            if (!Ignore)
            {
                try
                {
                    AuditLogModel model = new()
                    {
                        CurrentUserId = context.HttpContext.User.Identity.IsAuthenticated ? context.HttpContext.User.FindFirst("sub").Value : string.Empty,
                        Action = context.ActionDescriptor.DisplayName.ToSubstring(127),
                        HttpMethod = context.HttpContext.Request.Method,
                        IP = HttpHelper.GetIP(context.HttpContext),
                        ExecuteDuration = (int)stopwatch.ElapsedMilliseconds,
                        ExecuteTime = DateTime.Now,
                        Arguments = context.ActionArguments.Count > 0 ? context.ActionArguments.ToJson().ToSubstring(1023) : string.Empty,
                        Exception = resultContext.Exception?.Message.ToSubstring(127),
                        Result = (resultContext.Result is JsonResult) ? (resultContext.Result as JsonResult).Value?.ToJson().ToSubstring(2047) : string.Empty
                    };
                    IAuditLogStore store = context.HttpContext.RequestServices.GetService<IAuditLogStore>();
                    await store.SaveAsync(model);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}