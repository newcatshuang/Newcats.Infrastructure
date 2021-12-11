/***************************************************************************
 *GUID: 40fb8615-fed9-4581-9cd8-cd535286ad30
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-02 18:42:29
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Collections.Concurrent;
using Newcats.Utils.Extensions;
using Newcats.Utils.Models;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// 枚举操作帮助类
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 缓存，键为类的全名
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<EnumDescription>> _cache = new();

        /// <summary>
        /// 获取当前枚举项所在的枚举的所有枚举项描述类
        /// </summary>
        /// <typeparam name="T">要转换的枚举</typeparam>
        /// <returns>枚举项描述类集合EnumDescription</returns>
        public static List<EnumDescription> GetAllEnumDescriptions<T>() where T : Enum
        {
            Type type = typeof(T);
            return GetAllEnumDescriptions(type);
        }

        /// <summary>
        /// 获取当前枚举项所在的枚举的所有枚举项描述类
        /// </summary>
        /// <param name="enumType">指定的枚举类型</param>
        /// <returns>枚举项描述类集合EnumDescription(若指定类型为非枚举类型，则返回null)</returns>
        public static List<EnumDescription> GetAllEnumDescriptions(Type enumType)
        {
            if (!enumType.IsEnum)
                return null;
            List<EnumDescription> list = new();
            if (_cache.TryGetValue(enumType.FullName, out list))
            {
                if (list != null && list.Count > 0)
                    return list;
            }
            list = new List<EnumDescription>();
            foreach (Enum e in Enum.GetValues(enumType))
            {
                list.Add(e.GetEnumDescription());
            }

            _cache.TryAdd(enumType.FullName, list);
            return list;
        }
    }
}