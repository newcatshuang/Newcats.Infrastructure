/***************************************************************************
 *GUID: 8b5fe8b2-c4c1-44f0-ba98-5f71f727befd
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-15 13:01:48
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.ComponentModel;

namespace Newcats.Utils.UnitTest
{
    public class UserInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public GenderEnum Gender { get; set; }

        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum GenderEnum
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("未知")]
        Unknown = 0,

        /// <summary>
        /// 男
        /// </summary>
        [Description("男")]
        Man = 1,

        /// <summary>
        /// 女
        /// </summary>
        [Description("女")]
        Female = 2
    }
}