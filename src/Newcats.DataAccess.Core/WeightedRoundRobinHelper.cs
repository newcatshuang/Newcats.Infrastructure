/***************************************************************************
 *GUID: 13842369-a663-446a-893e-5799357ca6b4
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-18 19:18:06
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Security.Cryptography;
using System.Text;

namespace Newcats.DataAccess.Core;

/// <summary>
/// 平滑加权轮询(需要实例化为单例)
/// </summary>
/// <typeparam name="T">节点值类型</typeparam>
internal class WeightedRoundRobinHelper<T>
{
    #region 字段
    /// <summary>
    /// 最大公约数
    /// </summary>
    private readonly int _gcd = 0;

    /// <summary>
    /// 最大权重值
    /// </summary>
    private readonly int _maxWeight = 0;

    /// <summary>
    /// 节点数
    /// </summary>
    private readonly int _nodesCount = 0;

    /// <summary>
    /// 当前权重
    /// </summary>
    private int _currentWeight = 0;

    /// <summary>
    /// 上次选中的节点
    /// </summary>
    private int _lastChosenNode = -1;

    /// <summary>
    /// 锁
    /// </summary>
    private SpinLock _sLock = new SpinLock(true);

    /// <summary>
    /// 节点
    /// </summary>
    private readonly List<WeightedNode<T>> _nodes;
    #endregion

    /// <summary>
    /// 当前所有节点按权重正序排列之后序列化的json字符串的md5值(System.Text.Json的默认配置)
    /// </summary>
    public string Md5Value { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="nodes">节点</param>
    public WeightedRoundRobinHelper(List<WeightedNode<T>> nodes)
    {
        _nodes = nodes.OrderBy(q => q.Weight).ToList();
        _gcd = GetGcd(_nodes);
        _maxWeight = GetMaxWeight(_nodes);
        _nodesCount = _nodes.Count;
        Md5Value = GetMd5(_nodes);
    }

    /// <summary>
    /// 获取此次选择结果
    /// </summary>
    public WeightedNode<T> GetResult()
    {
        var isLocked = false;
        _sLock.Enter(ref isLocked);

        do
        {
            _lastChosenNode = (_lastChosenNode + 1) % _nodesCount;
            if (_lastChosenNode == 0)
            {
                _currentWeight -= _gcd;
                if (_currentWeight <= 0)
                {
                    _currentWeight = _maxWeight;
                }
            }
        } while (_nodes[_lastChosenNode].Weight < _currentWeight);

        if (isLocked)
        {
            _sLock.Exit(true);
        }

        return _nodes[_lastChosenNode];
    }

    /// <summary>
    /// 取权重的最大公约数(GreatestCommonDivisor)
    /// </summary>
    private int GetGcd(List<WeightedNode<T>> nodes)
    {
        int index = _lastChosenNode;
        if (index < 0)
            index = 0;

        int a = nodes[index].Weight;

        if (index >= _nodesCount - 1)
            index = -1;

        int b = nodes[index + 1].Weight;
        while (b != 0)
        {
            var t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    /// <summary>
    /// 取最大权重值
    /// </summary>
    private int GetMaxWeight(List<WeightedNode<T>> nodes)
    {
        int max = nodes.Max(n => n.Weight);
        return max;
    }

    /// <summary>
    /// 取所有节点的json字符串的md5值(System.Text.Json的默认配置)
    /// </summary>
    private string GetMd5(List<WeightedNode<T>> nodes)
    {
        return Helper.JsonMd5(nodes);
    }
}

/// <summary>
/// 权重节点
/// </summary>
internal class WeightedNode<T>
{
    /// <summary>
    /// 节点值
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 初始权重
    /// </summary>
    public int Weight { get; set; }
}

/// <summary>
/// 帮助类
/// </summary>
internal class Helper
{
    /// <summary>
    /// 先序列化为json字符串，在计算Md5指纹
    /// </summary>
    internal static string JsonMd5<T>(T obj)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(obj);
        return Md5(json);
    }

    /// <summary>
    /// Md5加密
    /// </summary>
    private static string Md5(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var md5 = MD5.Create();
        string result;
        try
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            result = BitConverter.ToString(hash);
        }
        finally
        {
            md5.Clear();
        }
        return result.Replace("-", "");
    }
}