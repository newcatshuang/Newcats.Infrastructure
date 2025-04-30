namespace SyncDataFromProductionEnv.Models;

/// <summary>
/// 同步配置实体类，用于存储数据同步的配置信息
/// </summary>
public class SyncDefinition
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 同步配置的唯一标识键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 需要同步的表名列表
    /// </summary>
    public string[] Tables { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 配置创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 配置最后更新时间
    /// </summary>
    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDelete { get; set; }
}