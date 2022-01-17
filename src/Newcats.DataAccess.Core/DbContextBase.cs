/***************************************************************************
 *GUID: a880c38c-cd17-4d8b-9ca1-6de2ed372466
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-10-19 18:04:49
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using Microsoft.Extensions.Options;

namespace Newcats.DataAccess.Core;

/// <summary>
/// 数据库上下文基类
/// </summary>
public abstract class DbContextBase : IDbContext
{
    /// <summary>
    /// 选项
    /// </summary>
    private readonly DbContextOptions _options;

    /// <summary>
    /// 主库数据库连接
    /// </summary>
    public IDbConnection Connection { get; }

    /// <summary>
    /// 从库数据库连接
    /// </summary>
    public IDbConnection? ReplicaConnection { get; set; }

    /// <summary>
    /// 创建数据库连接
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>数据库连接</returns>
    public abstract IDbConnection CreateConnection(string connectionString);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="optionsAccessor">配置项</param>
    public DbContextBase(IOptions<Core.DbContextOptions> optionsAccessor)
    {
        _options = optionsAccessor.Value;
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))//主库连接字符串不能为空
            throw new ArgumentNullException(nameof(_options.ConnectionString));

        //创建主库连接
        if (Connection != null)
        {
            Connection.ConnectionString = _options.ConnectionString;
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
        }
        else
        {
            Connection = CreateConnection(_options.ConnectionString);
            Connection.ConnectionString = _options.ConnectionString;
            Connection.Open();
        }

        //如果启用读写分离
        if (_options.EnableReadWriteSplit.HasValue && _options.EnableReadWriteSplit.Value)
        {
            ArgumentNullException.ThrowIfNull(nameof(_options.ReplicaConfigs));
            if (_options.ReplicaConfigs.Length == 0)
                throw new ArgumentException("If enable Read Write Splitting, ReplicaConfigs could not be null or empty!");

            //选择一个从库
            string replicaString = SelectReplicaConnectionString(_options.ReplicaConfigs, _options.ReplicaPolicy);

            //创建从库连接
            if (ReplicaConnection != null)
            {
                ReplicaConnection.ConnectionString = replicaString;
                if (ReplicaConnection.State == ConnectionState.Closed)
                    ReplicaConnection.Open();
            }
            else
            {
                ReplicaConnection = CreateConnection(replicaString);
                ReplicaConnection.ConnectionString = replicaString;
                ReplicaConnection.Open();
            }
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        if (Connection != null && Connection.State != ConnectionState.Closed)
            Connection.Close();
    }

    /// <summary>
    /// 选择一个从库连接字符串
    /// </summary>
    /// <param name="configs">从库配置</param>
    /// <param name="policy">从库选择策略</param>
    /// <returns>从库连接字符串</returns>
    private string SelectReplicaConnectionString(ReplicaConfig[] configs, ReplicaSelectPolicyEnum policy)
    {
        ArgumentNullException.ThrowIfNull(nameof(configs));
        string[] connStrs = configs.Where(c => !string.IsNullOrWhiteSpace(c.ReplicaConnectionString)).Select(c => c.ReplicaConnectionString).ToArray();
        int index = Random.Shared.Next(connStrs.Length);
        return connStrs[index];
    }
}