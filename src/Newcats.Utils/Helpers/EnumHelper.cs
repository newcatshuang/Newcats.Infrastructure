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
        private static readonly ConcurrentDictionary<string, List<EnumDescription>> _cache = new ConcurrentDictionary<string, List<EnumDescription>>();

        /// <summary>
        /// 把枚举对象的每一项转换成对应的类
        /// </summary>
        /// <typeparam name="T">要转换的枚举对象</typeparam>
        /// <returns></returns>
        public static List<EnumDescription> ConvertToList<T>() where T : Enum
        {
            Type type = typeof(T);
            List<EnumDescription> list = new List<EnumDescription>();
            if (!type.IsEnum)
                return list;

            if (_cache.TryGetValue(type.FullName, out list))
            {
                if (list != null && list.Count > 0)
                    return list;
            }

            foreach (var e in Enum.GetValues(typeof(T)))
            {
                EnumDescription m = new EnumDescription();

                m.Description = (e as Enum).GetDescription();
                m.Value = Convert.ToInt32(e);
                m.Name = e.ToString();
                list.Add(m);
            }

            _cache.TryAdd(type.FullName, list);
            return list;
        }
    }
}