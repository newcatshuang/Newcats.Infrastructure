using System.Threading.Tasks;
using Newcats.AspNetCore.Abstractions;

namespace Newcats.AspNetCore.Defaults
{
    /// <summary>
    /// 权限校验服务接口的默认实现(全部返回true)
    /// 实际应用时需要手动重写此方法
    /// 建议对用户的权限做缓存，避免出现性能问题
    /// </summary>
    public class PermissionService : IPermissionService
    {
        /// <summary>
        /// 判断给定的用户UserId是否有某项权限
        /// </summary>
        /// <param name="userId">用户UserId</param>
        /// <param name="permissionCode">权限Code</param>
        /// <returns>true or false</returns>
        public virtual bool HasPermission(string userId, string permissionCode)
        {
            return true;
        }

        /// <summary>
        /// 判断给定的用户UserId是否有某项权限
        /// </summary>
        /// <param name="userId">用户UserId</param>
        /// <param name="permissionCode">权限Code</param>
        /// <returns>true or false</returns>
        public virtual Task<bool> HasPermissionAsync(string userId, string permissionCode)
        {
            return Task.FromResult(true);
        }
    }
}