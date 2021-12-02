using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Newcats.Utils.Models;

namespace Newcats.Utils.Extensions
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 缓存，键为类的全名+值的名字
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _cacheDes = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 缓存，键为类的全名
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<EnumDescription>> _cacheAll = new ConcurrentDictionary<string, List<EnumDescription>>();

        /// <summary>
        /// 获取描述,使用System.ComponentModel.Description特性设置描述
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>Description特性描述</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string key = $"{type.FullName}:{value}";
            if (_cacheDes.TryGetValue(key, out string des))
            {
                if (!string.IsNullOrWhiteSpace(des))
                    return des;
            }

            string memberName = Enum.GetName(type, value);
            MemberInfo memberInfo = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            des = memberInfo.GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute attribute ? attribute.Description : memberInfo.Name;
            _cacheDes.TryAdd(key, des);
            return des;
        }

        /// <summary>
        /// 获取枚举项的枚举描述类
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>枚举项描述类EnumDescription</returns>
        public static EnumDescription GetEnumDescription(this Enum value)
        {
            EnumDescription description = new EnumDescription();
            description.Value = Convert.ToInt32(value);
            description.Name = value.ToString();
            description.Description = value.GetDescription();
            return description;
        }

        /// <summary>
        /// 获取当前枚举项所在的枚举的所有枚举项描述类
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>枚举项描述类集合EnumDescription</returns>
        public static List<EnumDescription> GetAllEnumDescriptions(this Enum value)
        {
            Type type = value.GetType();
            List<EnumDescription> list = new List<EnumDescription>();
            if (!type.IsEnum)
                return list;

            if (_cacheAll.TryGetValue(type.FullName, out list))
            {
                if (list != null && list.Count > 0)
                    return list;
            }
            list = new List<EnumDescription>();
            foreach (Enum e in Enum.GetValues(type))
            {
                list.Add(e.GetEnumDescription());
            }

            _cacheAll.TryAdd(type.FullName, list);
            return list;
        }
    }
}