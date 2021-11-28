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
        /// 把枚举对象的每一项转换成对应的类
        /// </summary>
        /// <typeparam name="T">要转换的枚举对象</typeparam>
        /// <returns></returns>
        public static List<EnumDescription> ConvertToList<T>() where T : Enum
        {
            List<EnumDescription> list = new List<EnumDescription>();
            if (!typeof(T).IsEnum)
                return list;

            foreach (var e in Enum.GetValues(typeof(T)))
            {
                EnumDescription m = new EnumDescription();

                m.Description = (e as Enum).GetDescription();
                m.Value = Convert.ToInt32(e);
                m.Name = e.ToString();
                list.Add(m);
            }
            return list;
        }
    }
}