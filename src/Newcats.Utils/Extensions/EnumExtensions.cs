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
        private static readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 获取描述,使用System.ComponentModel.Description特性设置描述
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>Description特性描述</returns>
        public static string GetDescription(this Enum value)
        {
            string des = string.Empty;
            string key = $"{type.FullName}:{value}";
            Type type = value.GetType();
            if (_cache.TryGetValue(key, out des))
            {
                if (!string.IsNullOrWhiteSpace(des))
                    return des;
            }

            string memberName = Enum.GetName(type, value);
            MemberInfo memberInfo = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            des = memberInfo.GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute attribute ? attribute.Description : memberInfo.Name;
            _cache.TryAdd(key, des);
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
    }
}