/***************************************************************************
 *GUID: 7c781a0c-9564-4755-aafb-0512355fab47
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-25 15:25:04
 *Author: NewcatsHuang
*****************************************************************************/
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MessagePack;
using KeyAttribute = MessagePack.KeyAttribute;

namespace SerializerTest
{
    [MessagePackObject(true)]
    public class Employee
    {
        /// <summary>
        /// 管理员Id，主键/唯一/自增
        /// </summary>
        [Key(0)]
        public long Id { get; set; }

        /// <summary>
        /// 邮箱，唯一
        /// </summary>
        [Key(1)]
        public string Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Key(2)]
        public string Phone { get; set; }

        /// <summary>
        /// 密码，存储的是哈希值
        /// Encrypt.Sha256(",$" + Password + salt + "+}")
        /// </summary>
        [Key(3)]
        public string Password { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        [Key(4)]
        public string Name { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        [Key(5)]
        public string ChineseName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Key(6)]
        public GenderEnum Gender { get; set; }

        /// <summary>
        /// 加入时间
        /// </summary>
        [Key(7)]
        public DateTime? JoinTime { get; set; }

        /// <summary>
        /// 加入时的ip
        /// </summary>
        [Key(8)]
        public string JoinIP { get; set; }

        /// <summary>
        /// 总登录次数
        /// </summary>
        [Key(9)]
        public int? LoginNum { get; set; }

        /// <summary>
        /// 今日登录错误次数
        /// </summary>
        [Key(10)]
        public int? TodayErrorCount { get; set; }

        /// <summary>
        /// 今日登录错误时间
        /// </summary>
        [Key(11)]
        public DateTime? TodayErrorTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Key(12)]
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最后登录ip
        /// </summary>
        [Key(13)]
        public string LastLoginIP { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        [Key(14)]
        public bool Disabled { get; set; }

        /// <summary>
        /// 禁用时间
        /// </summary>
        [Key(15)]
        public DateTime? DisabledTime { get; set; }

        /// <summary>
        /// 盐，参与密码哈希运算时的随机字符串
        /// Encrypt.GetRandomKey(128)
        /// </summary>
        [Key(16)]
        public string Salt { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [Key(17)]
        public long? CreateId { get; set; }

        /// <summary>
        /// 创建人Name
        /// </summary>
        [Key(18)]
        public string CreateName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Key(19)]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新人Id
        /// </summary>
        [Key(20)]
        public long? UpdateId { get; set; }

        /// <summary>
        /// 更新人Name
        /// </summary>
        [Key(21)]
        public string UpdateName { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Key(22)]
        public DateTime? UpdateTime { get; set; }
    }

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