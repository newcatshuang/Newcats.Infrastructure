using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
            var l1 = DateTime.Now.ToUnixTimestamp();
            //long std = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            //long l2 = (DateTime.UtcNow.Ticks - std) / 10000000L;
            var l2 = new DateTime(1992, 1, 23, 0, 0, 0, DateTimeKind.Local).ToUnixTimestamp();

            Console.WriteLine(l1);
            //Console.WriteLine(std);
            Console.WriteLine(l2);
            long lt1 = 1640792882L;
            long lt2 = 696096000L;

            Console.WriteLine(lt1.GetTimeFromUnixTimestamp().ToChinaString());
            Console.WriteLine(lt2.GetTimeFromUnixTimestamp().ToChinaString());


            //var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            //long ticks = (new DateTime(1992, 1, 23) - start.Add(new TimeSpan(8, 0, 0))).Ticks;
            //Console.WriteLine(Convert.ToInt64(ticks / TimeSpan.TicksPerSecond));
            //ReadExcel.Read();

            return;

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


            User u = new()
            {
                Id = 1,
                Name = "newcats",
                CN = "皇权特许",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now.AddHours(3),
                Season = Season.Summer,
                IsMan = true
            };

            //var sss1 = u.Season.GetAllEnumDescriptions();
            //var s2222 = Season.Summer.GetAllEnumDescriptions();
            var s = u.Season.GetDescription();
            var s2 = u.Season.GetEnumDescription();

            Console.WriteLine(u.ToJson());
            return;

            string rawJson = "{\"Id\":1,\"Name\":\"newcats\",\"CN\":\"皇权特许\",\"CreateTime\":\"2020-05-10 23:36:03\",\"UpdateTime\":\"2020-05-11 02:36:03\",\"Season\":1,\"IsMan\":\"true\"}";

            //Console.WriteLine(u.ToJson());

            var sss = rawJson.Deserialize<User>();

            //Console.WriteLine(sss.CreateTime.ToChinaString());

            //Console.WriteLine(u.CN.PinYin());

            DateTime date = DateTime.Now;
            //Console.WriteLine(date.ToChinaString());
            //Console.WriteLine(date.ToUnixTimestamp());

            long ss = date.ToUnixTimestamp();



            //Console.WriteLine(ss.GetTimeFromUnixTimestamp());

            //Console.WriteLine("-11.11345678900".IsNumber(false));

            Console.WriteLine("127.0x.0.1".IsIPv4());

            Console.WriteLine("17000000000".EncryptPhoneNumber());


            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(IdHelper.Create());
            }
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