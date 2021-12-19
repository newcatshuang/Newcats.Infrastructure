/***************************************************************************
 *GUID: d54700b3-12b4-427c-9e60-137cbcf51048
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-15 22:46:34
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Collections.Concurrent;
using System.Reflection;
using NpgsqlTypes;

namespace Newcats.DataAccess.PostgreSql
{
    /// <summary>
    /// NpgsqlType枚举项描述类
    /// </summary>
    internal class NpgsqlTypeDescription
    {
        /// <summary>
        /// 枚举本身
        /// </summary>
        public NpgsqlDbType NpgType { get; set; }

        /// <summary>  
        /// 枚举项的值  
        /// </summary>  
        public int Value { get; set; }

        /// <summary>  
        /// 枚举项的名称  
        /// </summary>  
        public string Name { get; set; }

        /// <summary>  
        /// BuiltInPostgresTypeAttribute.Name  
        /// </summary>  
        public string PostgresType { get; set; }
    }

    /// <summary>
    /// NpgsqlTypes帮助类
    /// </summary>
    internal static class NpgsqlTypeHelper
    {
        /// <summary>
        /// 缓存，键为类的全名
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<NpgsqlTypeDescription>> _cache = new();

        /// <summary>
        /// 获取描述,使用System.ComponentModel.Description特性设置描述
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>Description特性描述</returns>
        private static string GetPostgresType(this Enum value)
        {
            string r = string.Empty;
            Type type = value.GetType();

            string memberName = Enum.GetName(type, value);
            MemberInfo memberInfo = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            var attrs = memberInfo.GetCustomAttributes();
            if (attrs != null && attrs.Any())
            {
                var a1 = attrs.First();
                if (a1.ToString().Equals("NpgsqlTypes.BuiltInPostgresType", StringComparison.OrdinalIgnoreCase))
                {
                    r = a1.GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(a1).ToString();
                }
            }
            return string.IsNullOrWhiteSpace(r) ? memberInfo.Name : r;
        }

        /// <summary>
        /// 获取枚举项的枚举描述类
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>枚举项描述类EnumDescription</returns>
        private static NpgsqlTypeDescription GetNpgsqlTypeDescription(this Enum value)
        {
            NpgsqlTypeDescription description = new()
            {
                Value = Convert.ToInt32(value),
                Name = value.ToString(),
                PostgresType = value.GetPostgresType(),
                NpgType = (NpgsqlDbType)value
            };
            return description;
        }

        /// <summary>
        /// 获取当前枚举项所在的枚举的所有枚举项描述类
        /// </summary>
        /// <param name="type">当前枚举项</param>
        /// <returns>枚举项描述类集合EnumDescription</returns>
        internal static List<NpgsqlTypeDescription> GetAllNpgsqlTypes()
        {
            Type type = typeof(NpgsqlDbType);
            List<NpgsqlTypeDescription> list = new();
            if (_cache.TryGetValue(type.FullName, out list))
            {
                if (list != null && list.Count > 0)
                    return list;
            }
            list = new List<NpgsqlTypeDescription>();
            foreach (Enum e in Enum.GetValues(type))
            {
                list.Add(e.GetNpgsqlTypeDescription());
            }

            _cache.TryAdd(type.FullName, list);
            return list;
        }
    }
}