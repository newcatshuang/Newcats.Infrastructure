using System.Threading.Tasks;
using Newcats.DependencyInjection;

namespace Newcats.AspNetCore.Abstractions
{
    /// <summary>
    /// 权限校验服务接口
    /// </summary>
    public interface IPermissionService : IScopedDependency
    {
        /// <summary>
        /// 判断给定的用户UserId是否有某项权限
        /// </summary>
        /// <param name="userId">用户UserId</param>
        /// <param name="permissionCode">权限Code</param>
        /// <returns>true or false</returns>
        bool HasPermission(int userId, string permissionCode);

        /// <summary>
        /// 判断给定的用户UserId是否有某项权限
        /// </summary>
        /// <param name="userId">用户UserId</param>
        /// <param name="permissionCode">权限Code</param>
        /// <returns>true or false</returns>
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
    }
}