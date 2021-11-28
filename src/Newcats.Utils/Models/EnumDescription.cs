/***************************************************************************
 *GUID: 0dbc1c22-5851-4924-bdf9-c708e5f15722
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-11-28 22:13:31
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

namespace Newcats.Utils.Models
{
    /// <summary>
    /// 枚举项描述类
    /// </summary>
    public class EnumDescription
    {
        /// <summary>  
        /// 枚举项的值  
        /// </summary>  
        public int Value { get; set; }

        /// <summary>  
        /// 枚举项的名称  
        /// </summary>  
        public string Name { get; set; }

        /// <summary>  
        /// 枚举项的描述  
        /// </summary>  
        public string Description { get; set; }
    }
}