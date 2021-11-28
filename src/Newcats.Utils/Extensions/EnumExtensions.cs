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
        /// 获取描述,使用System.ComponentModel.Description特性设置描述
        /// </summary>
        /// <param name="value">当前枚举项</param>
        /// <returns>Description特性描述</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string memberName = Enum.GetName(type, value);
            MemberInfo memberInfo = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            return memberInfo.GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute attribute ? attribute.Description : memberInfo.Name;
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