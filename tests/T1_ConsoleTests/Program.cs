using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Newcats.DataAccess.Core;
using Newcats.Utils.Extensions;
using Newcats.Utils.Helpers;
using Newcats.Utils.Models;
using NpgsqlTypes;

namespace T1_ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pub = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDAw++BrLzvOzCxuyNikLBpFRJ5
z3ZyKLqknQjfyUwhW+AW+N/mc5fv86/eH9GePDMB44TTqat1Cs+MdWXQqgWNBEVi
CEspGKg3hrA24zy9izeT2oXlnInvA9cZioIjNmH7TN4+iA2Y+nm+EypnUhLyAa+5
Ca4Gw3kUdfk0PRKXsQIDAQAB
-----END PUBLIC KEY-----";

            const string pri = @"-----BEGIN PRIVATE KEY-----
MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAMDD74GsvO87MLG7
I2KQsGkVEnnPdnIouqSdCN/JTCFb4Bb43+Zzl+/zr94f0Z48MwHjhNOpq3UKz4x1
ZdCqBY0ERWIISykYqDeGsDbjPL2LN5PaheWcie8D1xmKgiM2YftM3j6IDZj6eb4T
KmdSEvIBr7kJrgbDeRR1+TQ9EpexAgMBAAECgYABfEEHNcaK7WDVrcR7CPRjCueG
isO143x0skUUPF/azfNvGeHXy8I69hy/0Gz0FDCsbg1i+Ca7EydARNNRBU5foVkI
i+RQ8JDOFEiDhmg8Y6WCdqvgc3mqtK1cLd2b41h5RTAhwjPQyXqrsXV++7dYHYEi
A3QLPEWY2URRf/ljBQJBAP72+buMiCrXZTcEPBdNjuJfEnRsO99wLQ3SJFjcbtYD
8srP8/dRpAKYje28HGb8FhngsVifZAgXn1yptwnv5HcCQQDBjE579XZoHYxPXwX9
Ok+rboBLD3ZcN5MmJPedOVilIGHmOWhOCQQG2wR8BSJBm8KivYgnpl2hGu7UykjW
bLcXAkA66IlOwet8KoQiMAJKSAspVloHODKlL1/Zf6ISPewF2qewIFf33o8MYn74
XKwbR0c792RCW6FbFUomuLenvhuFAkEAg7HlgWEPFV7tpuAgYiK4LQy+TNSTk8HY
5T6IIbnwgEMdehqbx2VJESUb0wtnOL80W6mdKGWCFeoOkkml8Aj/NwJAafPLEeXo
HtO7tkC2awxl58r6s6gg2EpNONJwftnwKDSAkJo+LW02eMbDeCapMlD0ZupzAO2b
5fjuS4glGJ4Muw==
-----END PRIVATE KEY-----";

            //var s = EncryptHelper.RsaSignData("NewcatsHuang");
            //Console.WriteLine(s);

            //var r = EncryptHelper.RsaVerifyData("NewcatsHuang", s);
            //Console.WriteLine(r);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                stringBuilder.Append("Newcats");
            }

            var enStr = EncryptHelper.RsaEncrypt(stringBuilder.ToString() + "Huang", pub);
            Console.WriteLine(enStr);
            Console.WriteLine("\n");
            var deStr = EncryptHelper.RsaDecrypt(enStr, pri);
            Console.WriteLine(deStr);
            return;

            //Console.WriteLine("\r\n\r\n");
            //return;

            #region MyRegion
            //DES测试
            //var r1 = EncryptHelper.DesEncrypt("Newcats");
            //Console.WriteLine(r1);
            //var r2 = EncryptHelper.DesDecrypt(r1);
            //Console.WriteLine(r2);

            //var r3 = EncryptHelper.DesEncrypt("Newcats", false);
            //Console.WriteLine(r3);
            //var r4 = EncryptHelper.DesDecrypt(r3, false);
            //Console.WriteLine(r4);
            //return;
            #endregion

            #region 归档1

            //            string pubKey = @"-----BEGIN PUBLIC KEY-----
            //MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuAU3oT1i7ojq1EDhGLS2
            //hD1avO1vWP90Wf0AlxC99f4D/3LdXpnUdasV5adogbEAYSIr0t9Yyipdzz5+VP97
            //+M4ug4Zv0wG7RgyJWL2YJsAwOEPOzgYez/rykfuDbW6zIvy+lv0qvHGmlJT4D1WM
            //Ni3mO0QjGpmlHXvuVzxx5ypjqkq/SCDaItArGuihpZlPYi04K9lcFp7MvZXvXndu
            //5yvpBbpTFvW6k97DDYph+BZWm4Vw/vTGw3ym7m3f2duIHGFfa3OdRg53f+j66ObM
            //1/8D3aW9+lxvd4vSQk1Xgfs33wGEpRpSNiS3foAsIaPXtn7mBYm3+i/1zkoiipBs
            //twIDAQAB
            //-----END PUBLIC KEY-----";

            //string enStr = Newcats.Utils.Helpers.EncryptHelper.RSASign("newcats", pubKey);

            //Console.WriteLine(IdHelper.Create(true));

            //Console.WriteLine(Newcats.Utils.Helpers.EncryptHelper.AESDecrypt("dIuVIledkcP0Ron1gwBdCA=="));

            //var l1 = DateTime.Now.ToUnixTimestamp();
            //long std = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            //long l2 = (DateTime.UtcNow.Ticks - std) / 10000000L;
            //var l2 = new DateTime(1992, 1, 23, 0, 0, 0, DateTimeKind.Local).ToUnixTimestamp();

            //Console.WriteLine(l1);
            //Console.WriteLine(std);
            //Console.WriteLine(l2);
            //long lt1 = 1640792882L;
            //long lt2 = 696096000L;

            //Console.WriteLine(lt1.GetTimeFromUnixTimestamp().ToChinaString());
            //Console.WriteLine(lt2.GetTimeFromUnixTimestamp().ToChinaString());


            //var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            //long ticks = (new DateTime(1992, 1, 23) - start.Add(new TimeSpan(8, 0, 0))).Ticks;
            //Console.WriteLine(Convert.ToInt64(ticks / TimeSpan.TicksPerSecond));
            //ReadExcel.Read();

            //return;

            //var tn = RepositoryHelper.GetTableName(typeof(UserInfo));
            //RepositoryHelper.GetTableName(typeof(UserInfo));
            //Console.WriteLine(tn);

            //return;

            //var type = NpgsqlDbType.Bigint.GetType();
            //string memberName = Enum.GetName(NpgsqlDbType.Bigint);
            //MemberInfo memberInfo = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            //var attrs = memberInfo.GetCustomAttributes();
            ////if (attrs != null && attrs.Any())
            //var a1 = attrs.First();
            //var a2 = attrs.First().GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(a1);

            //return;


            //ReadExcel.Read();

            //return;

            //EnumHelper.ConvertToList<Season>();
            //EnumHelper.ConvertToList<Season>();

            //Console.WriteLine(GetLastMonthCurrent());
            //return;

            //var name = RepositoryHelper.GetTableName(typeof(UserInfo));
            //var r = RepositoryHelper.GetInsertSqlText(typeof(UserInfo));
            //return;

            //Summary summary = BenchmarkRunner.Run<TestContext>();
            //Console.ReadLine();
            //return;

            //var list = EnumHelper.ConvertToList<Season>();
            //Console.WriteLine(list.ToJson());


            //User u = new()
            //{
            //    Id = 1,
            //    Name = "newcats",
            //    CN = "皇权特许",
            //    CreateTime = DateTime.Now,
            //    UpdateTime = DateTime.Now.AddHours(3),
            //    Season = Season.Summer,
            //    IsMan = true
            //};

            //var sss1 = u.Season.GetAllEnumDescriptions();
            //var s2222 = Season.Summer.GetAllEnumDescriptions();
            //var s = u.Season.GetDescription();
            //var s2 = u.Season.GetEnumDescription();

            //Console.WriteLine(u.ToJson());
            //return;

            //string rawJson = "{\"Id\":1,\"Name\":\"newcats\",\"CN\":\"皇权特许\",\"CreateTime\":\"2020-05-10 23:36:03\",\"UpdateTime\":\"2020-05-11 02:36:03\",\"Season\":1,\"IsMan\":\"true\"}";

            //Console.WriteLine(u.ToJson());

            //var sss = rawJson.Deserialize<User>();

            //Console.WriteLine(sss.CreateTime.ToChinaString());

            //Console.WriteLine(u.CN.PinYin());

            //DateTime date = DateTime.Now;
            //Console.WriteLine(date.ToChinaString());
            //Console.WriteLine(date.ToUnixTimestamp());

            //long ss = date.ToUnixTimestamp();



            //Console.WriteLine(ss.GetTimeFromUnixTimestamp());

            //Console.WriteLine("-11.11345678900".IsNumber(false));

            //Console.WriteLine("127.0x.0.1".IsIPv4());

            //Console.WriteLine("17000000000".EncryptPhoneNumber());


            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine(IdHelper.Create());
            //} 
            #endregion
        }

        /// <summary>
        /// 获取上月同期的日期，若今天比上月最后一天大，则返回本月1日
        /// </summary>
        /// <returns></returns>
        private static DateTime GetLastMonthCurrent()
        {
            DateTime now = DateTime.Now;// new DateTime(2021, 3, 30);
            int day = now.Day;//今天的日期数
            int nowDays = DateTime.DaysInMonth(now.Year, now.Month);//本月总天数
            int lastDays = DateTime.DaysInMonth(now.AddMonths(-1).Year, now.AddMonths(-1).Month);//上月总天数

            if ((lastDays < nowDays) && (day > lastDays))
            {
                return new DateTime(now.Year, now.Month, 1);//返回当月1号
            }

            return new DateTime(now.AddMonths(-1).Year, now.AddMonths(-1).Month, now.Day);
        }

    }

    public class TestContext
    {
        [Benchmark]
        public void Method1()
        {
            RandomNumberGenerator.GetInt32(10, 20);
        }

        [Benchmark]
        public void Method2()
        {
            Random.Shared.Next(10, 20);
        }
    }


    [Table("UserInfo", Schema = "public")]
    public class UserInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public long? UserId { get; set; }

        [Column("CreateTime")]
        public DateTime JoinTime { get; set; }
    }
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CN { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public Season Season { get; set; }

        public EnumDescription SeasonDescription { get { return Season.GetEnumDescription(); } }

        public bool IsMan { get; set; }
    }

    public enum Season
    {
        Spring = 0,

        [Description("夏天")]
        Summer = 1,

        Fall = 2,

        Winter = 3
    }
}