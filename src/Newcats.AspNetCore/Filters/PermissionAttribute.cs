using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newcats.AspNetCore.Interfaces;

namespace Newcats.AspNetCore.Filters
{
    /// <summary>
    /// 1.权限验证特性。
    /// 2.执行方法之前验证当前用户是否具有指定的PermissionCode。
    /// 3.传入多个PermissionCode时，只要有一个Code符合即通过验证。
    /// 4.不传PermissionCode时，只验证当前用户是否登录(Identity.IsAuthenticated)。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PermissionAttribute : Attribute, IAsyncResourceFilter
    {
        //备注：
        //不能实现IAuthorizationFilter接口，因为会在Authorize特性之前执行
        //所以最优的方案是实现IResourceFilter接口，在IAuthorizationFilter之后模型绑定之前执行

        /// <summary>
        /// 权限code
        /// </summary>
        public string[] PermissionCodes { get; set; }

        /// <summary>
        /// 启用权限校验
        /// </summary>
        /// <param name="permissionCodes">权限code</param>
        public PermissionAttribute(params string[] permissionCodes)
        {
            PermissionCodes = permissionCodes;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            bool isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                context.Result = new ContentResult() { StatusCode = 403, Content = "You have no permission to access this service." };
                return;
            }

            bool hasPermission = false;
            int userId = int.Parse(context.HttpContext.User.FindFirst("sub").Value);
            IPermissionService service = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (PermissionCodes == null || PermissionCodes.Length == 0)
            {
                hasPermission = isAuthenticated;
            }
            else
            {
                foreach (string item in PermissionCodes)
                {
                    hasPermission = await service.HasPermissionAsync(userId, item);
                    if (hasPermission)
                        break;
                }
            }

            if (!hasPermission)
            {
                context.Result = new ContentResult() { StatusCode = 403, Content = "You have no permission to access this service." };
                return;
            }

            await next();
        }
    }
}