/***************************************************************************
 *GUID: 3f27bdac-421a-4b58-8915-b23cdcb19e12
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-29 21:24:09
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newcats.Utils.Extensions;

namespace T1_ConsoleTests
{
    internal class UtilsTest
    {
        public static void Run()
        {
            //1.IEnumerable数据转为DataTable
            List<UserInfo> users = new List<UserInfo>();
            users.Add(new UserInfo() { Id = 1, Name = "Newcats" });
            users.Add(new UserInfo() { Id = 2, Name = "Huang" });
            DataTable r1 = users.ToDataTable();

            //2.DataTable数据转为List
            List<UserInfo> r2 = r1.ToList<UserInfo>();

            //3.根据condition条件，判断是否加入当前list集合
            users.AddIf(new UserInfo() { Id = 3, Name = "NewcatsHuang" }, "newcats".Length == 0);

            //4.元素不为null，则加入当前list集合
            UserInfo u1 = null;
            users.AddIfNotNull(u1);

            //5.若集合为空，则抛异常
            users.ThrowIfEmpty();

            //6.若集合为null或者空，则抛异常
            users.ThrowIfNullOrEmpty();

            //7.判断集合是否为null或者空
            bool r3 = users.IsNullOrEmpty();//=>false

            //8.获取枚举项的Description特性描述
            string r4 = Season.Spring.GetDescription();//=>春季

            //9.获取枚举项的枚举描述类
            Newcats.Utils.Models.EnumDescription r5 = Season.Spring.GetEnumDescription();//=>r5.Value=0,r5.Name="Spring",r5.Description="春季"

            //10.获取枚举的所有枚举描述类
            List<Newcats.Utils.Models.EnumDescription> r6 = Season.Spring.GetAllEnumDescriptions();//=>r6[0].Value=1,r6[0].Name="Spring"...r6[1].Value=1,r6[2].Description="夏季"

            //11.判断当前对象是否为null
            r4.ThrowIfNull();//=>false

            //12.Md5加密，返回32位结果
            string r7 = "Newcats".MD5By32();//=>A659F0864BCF9A600D048E6158482459

            //13.获取此字符串的Sha1值
            string r8 = "Newcats".Sha1();//=>F94EB89F28111FBCA123CE6318ED795AA59A244E

            //14.获取此字符串的Sha256值
            string r9 = "Newcats".Sha256();//=>CD787FAEBCA9E9E1FFA1F688F9CF5F221E0F703022807CBFF1FA8AA685A7C678

            //15.获取此字符串的DES加密结果(默认密钥)
            string r10 = "Newcats".DesEncrypt();//=>YTM3cMMHHT0=

            //16.DES解密
            string r11 = r10.DesDecrypt();//=>Newcats

            //16.获取此字符串的DES加密结果
            string r12 = "Newcats".DesEncrypt("newcatsnewcatsnewcats123");//=>DkPSMnDuLRM=

            //18.DES解密
            string r13 = r12.DesDecrypt("newcatsnewcatsnewcats123");//=>Newcats

            //19.转换对象为json字符串(使用System.Text.Json)
            string r14 = users[0].ToJson();//=>{"Id":1,"Name":"Newcats","UserId":null,"JoinTime":"0001-01-01 00:00:00.000"}

            //20.把json字符串反序列化为对象(使用System.Text.Json)
            UserInfo r15 = r14.Deserialize<UserInfo>();//=>r15.Id=1,r15.Name="Newcats",r15.UserId=null,r15.JoinTime=0001-01-01 00:00:00.000

            //21.转换为Unix时间戳(秒)
            long r16 = DateTime.Now.ToUnixTimestamp();//1640788430

        }
    }
}