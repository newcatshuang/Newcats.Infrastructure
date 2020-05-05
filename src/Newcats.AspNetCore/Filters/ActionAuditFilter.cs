using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newcats.AspNetCore.Attributes;
using Newcats.AspNetCore.Interfaces;
using Newcats.AspNetCore.Models;

namespace Newcats.AspNetCore.Filters
{
    /// <summary>
    /// 全局操作审计过滤器
    /// </summary>
    public class ActionAuditFilter : IAsyncActionFilter
    {
        private readonly IAuditLogStore _auditLogStore;
        private readonly IHttpContextAccessor _accessor;

        public ActionAuditFilter(IAuditLogStore auditLogStore, IHttpContextAccessor accessor)
        {
            _auditLogStore = auditLogStore;
            _accessor = accessor;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // do something before the action executes
            string httpMethod = context.HttpContext.Request.Method;
            string actionName = context.ActionDescriptor.RouteValues["Action"];
            context.ActionDescriptor.FilterDescriptors.Where(f => f.Scope == FilterScope.Action)
               .Select(f => f.Filter).OfType<AuditableAttribute>();

            bool isAudit = (!httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)) && (!actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase));

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var resultContext = await next();//执行Action
            stopWatch.Stop();

            // do something after the action executes; resultContext.Result will be set
            if (isAudit)
            {
                try
                {
                    AuditLogModel entity = new AuditLogModel
                    {
                        CurrentUserId = int.Parse(context.HttpContext.User.FindFirst("sub").Value),
                        Action = context.ActionDescriptor.DisplayName.ToSubstring(127),
                        HttpMethod = httpMethod,
                        IP = HttpHelper.GetIP(_accessor).ToSubstring(63),
                        ExecuteDuration = (int)stopWatch.ElapsedMilliseconds,
                        ExecuteTime = DateTime.Now,
                        Arguments = context.ActionArguments.Count > 0 ? context.ActionArguments.ToJson().ToSubstring(1023) : string.Empty,
                        Exception = resultContext.Exception?.Message.ToSubstring(127),
                        Result = (resultContext.Result is JsonResult) ? (resultContext.Result as JsonResult).Value?.ToJson().ToSubstring(2047) : string.Empty
                    };
                    await _auditLogStore.SaveAsync(entity);
                }
                catch (Exception ex) { throw ex; }
            }
        }
    }
}