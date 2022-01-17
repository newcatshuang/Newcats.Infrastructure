/***************************************************************************
 *GUID: 698a7d02-3094-4b66-8a2f-008d3c01f22a
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-17 22:31:15
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace T1_ConsoleTests
{
    public class WeightedRoundRobin
    {
        private readonly int _greatestCommonDivisor = 0;
        private readonly int _maxWeight = 0;
        private readonly int _nodesCount = 0;
        private int _currentDispatchWeight = 0;
        private int _lastChosenNode = -1;

        private SpinLock _sLock = new SpinLock(true);

        private readonly List<WeightedRoundRobinModel> _nodes;

        public WeightedRoundRobin(List<WeightedRoundRobinModel> node)
        {
            _nodes = node.OrderBy(q => q.Weight).ToList();
            _greatestCommonDivisor = GetGcd(_nodes);
            _maxWeight = GetMaxWeight(_nodes);
            _nodesCount = _nodes.Count;
        }

        /// <summary>
        /// An implementation of Weighted Round Robin
        /// </summary>
        /// <returns></returns>
        public WeightedRoundRobinModel GetService()
        {
            var isLocked = false;
            _sLock.Enter(ref isLocked);

            do
            {
                _lastChosenNode = (_lastChosenNode + 1) % _nodesCount;
                if (_lastChosenNode == 0)
                {
                    _currentDispatchWeight = _currentDispatchWeight - _greatestCommonDivisor;
                    if (_currentDispatchWeight <= 0)
                    {
                        _currentDispatchWeight = _maxWeight;
                    }
                }
            } while (_nodes[_lastChosenNode].Weight < _currentDispatchWeight);

            if (isLocked)
            {
                _sLock.Exit(true);
            }

            return _nodes[_lastChosenNode];
        }

        /// <summary>
        /// Get Greatest Common Divisor
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        private int GetGcd(List<WeightedRoundRobinModel> servers)
        {
            return 1;
        }

        /// <summary>
        /// Get max weight
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        private int GetMaxWeight(List<WeightedRoundRobinModel> servers)
        {
            var max = 0;
            foreach (var s in servers)
            {
                if (s.Weight > max)
                    max = s.Weight;
            }
            return max;
        }
    }

    public class WeightedRoundRobinModel
    {
        /// <summary>
        ///  服务商ID
        /// </summary>
        public int ProviderIndex { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get; set; }
    }
}