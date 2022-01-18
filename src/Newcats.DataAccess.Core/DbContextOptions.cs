/***************************************************************************
 *GUID: ad2d90e5-afed-4c82-a6b3-3374aeafd201
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-11-18 16:14:29
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.ComponentModel;
using Microsoft.Extensions.Options;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 数据库上下文选项
    /// </summary>
    public class DbContextOptions : IOptions<DbContextOptions>
    {
        /// <summary>
        /// 主库连接字符串
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// 是否启用读写分离
        /// </summary>
        public bool? EnableReadWriteSplit { get; set; }

        /// <summary>
        /// 从库配置
        /// </summary>
        public ReplicaConfig[]? ReplicaConfigs { get; set; }

        /// <summary>
        /// 从库选择策略(默认为加权轮询算法)
        /// </summary>
        public ReplicaSelectPolicyEnum ReplicaPolicy { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public DbContextOptions Value
        {
            get
            {
                return this;
            }
        }
    }

    /// <summary>
    /// 从库配置
    /// </summary>
    public class ReplicaConfig
    {
        /// <summary>
        /// 从库Id(只需要保证当前从库数组里唯一即可，自定义数值)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 从库连接字符串
        /// </summary>
        public string? ReplicaConnectionString { get; set; }

        /// <summary>
        /// 权重(使用加权轮询算法时需要配置对应从库的权重)
        /// </summary>
        public int Weight { get; set; }
    }

    /// <summary>
    /// 从库选择策略
    /// </summary>
    public enum ReplicaSelectPolicyEnum
    {
        /// <summary>
        /// 平滑加权轮询
        /// </summary>
        [Description("平滑加权轮询")]
        WeightedRoundRobin = 0,

        /// <summary>
        /// 轮询
        /// </summary>
        [Description("轮询")]
        RoundRobin = 1,

        /// <summary>
        /// 随机
        /// </summary>
        [Description("随机")]
        Random = 2,

        /// <summary>
        /// 自定义(默认实现为Random算法，否则需要重写<see cref="DbContextBase.CustomizeReplicaStringSelector(List{string})"/>方法)
        /// </summary>
        [Description("自定义")]
        Customize = 3
    }
}