﻿using System;
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
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC8fhcmX+ZiGMu64JGKVucGIgkL
14exq8U/tIaNbFr/5tlmfj9QM3v1X1OvJpc8zkQ+oGKHURdum5mEA4WfQy53N6e0
o1++qomomAbMmO+vBjzk9oMrfE6pdB72PBcXzBHZ9xbpuTLHmzgQX0TKVrbPTMDB
nG/AVQKNf8PEPLGYYwIDAQAB
-----END PUBLIC KEY-----";

            const string pri = @"-----BEGIN PRIVATE KEY-----
MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBALx+FyZf5mIYy7rg
kYpW5wYiCQvXh7GrxT+0ho1sWv/m2WZ+P1Aze/VfU68mlzzORD6gYodRF26bmYQD
hZ9DLnc3p7SjX76qiaiYBsyY768GPOT2gyt8Tql0HvY8FxfMEdn3Fum5MsebOBBf
RMpWts9MwMGcb8BVAo1/w8Q8sZhjAgMBAAECgYBR4KPFs5qrugGlZ69Q3Hb2Hwq2
7iSvdOH1zkE0ZYER8AE3mFeASxzpdUMwrh679w2F9r1V8k+vaD/DLJR6ngAsYf1s
44+wK4pHAbEQw4V6gvUNiojaWFvmGjIkgNklL28FIztBK6rQw45Ws3pFMI9Z8gJE
lTNYPNrO7aWG2AGBoQJBAOk55vKOhqeXp1EnNzZqmlBcJzUwMsjCsbbjiqSALgw1
G+3YpKzjqaupUH79rUBnBH0+zeqNFQDGHSQq1HZYNb0CQQDO5fQ97Fe+ggrz/1K6
DRoaIXuufXr5fRcDsg/OJDjFU/FF/k+iDZx+Xg+/5Vcwh3ObRubJTXzjxAxtr7OM
AJifAkA9QXz8Bh0t1XIXqYIh47vmEV4m5SLhgellTLw0Woo9oJFWVglic2Uz9gNz
ZzNi7+vI7X7t9OIoUuCCiFFHSAHNAkEAw05NbjrxZVPK1SObSyfdEPe0kjW/ZU4Q
8Jsg0l5c/OFNq93x/C7PIHbYdTQgRx5GIjke2L39+9/wP4aRfcJ+TwJBAN4IIPjm
exAURqbzW+Auh7ScCqIL4BXa+tObXvtnk7kR9neggX/ZmCY4v69Q+RKiZa0vqZ70
LafQ7LG0Jv8FL8E=
-----END PRIVATE KEY-----";

            string res = RsaUtil.RsaEncrypt("Newcats", pub);
            Console.WriteLine(res);

            return;
            //var r = RsaUtil.CreateRsaKey(RsaKeyFormatEnum.Xml, 2048, true);
            //Console.WriteLine(r.PublicKey);
            //Console.WriteLine("\r\n\r\n");
            //Console.WriteLine(r.PrivateKey);

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