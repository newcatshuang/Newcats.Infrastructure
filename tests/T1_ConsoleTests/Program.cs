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
            const string pubKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsAG3NuBhYj5qnTgnCZtS
u8Nk+iW66pPGk3ppIJPgq70VStkgJzdNb1ZpVTXYYHLG8snTJfhx8jOnArmQyDcU
ti15ZVzkP8yYhSv9fVVpvn80EVvyxOYLZhzLFLZk+8XVhi/k4DxANuBW9HdlxqYG
AvpJYBzM46V6An9gcPoOD8RXZpiH/U4AKd0TohcCdns00YvY++G3I7mjp5uZtdgP
fnAX88uOMeYUIOlmla1Y83Tk/+CHWUmTgq3oSAKHZ8662mF0NNkOfElUeUH+1113
9tbMckl0vCOeXs3ogwhpRTgEYZF8nB4KjUs7Tnwkrhl+kE+IacknkSgQuoV6QJ+R
AQIDAQAB
-----END PUBLIC KEY-----";

            const string privKey = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCwAbc24GFiPmqd
OCcJm1K7w2T6Jbrqk8aTemkgk+CrvRVK2SAnN01vVmlVNdhgcsbyydMl+HHyM6cC
uZDINxS2LXllXOQ/zJiFK/19VWm+fzQRW/LE5gtmHMsUtmT7xdWGL+TgPEA24Fb0
d2XGpgYC+klgHMzjpXoCf2Bw+g4PxFdmmIf9TgAp3ROiFwJ2ezTRi9j74bcjuaOn
m5m12A9+cBfzy44x5hQg6WaVrVjzdOT/4IdZSZOCrehIAodnzrraYXQ02Q58SVR5
Qf7XXXf21sxySXS8I55ezeiDCGlFOARhkXycHgqNSztOfCSuGX6QT4hpySeRKBC6
hXpAn5EBAgMBAAECggEAKh7+/b8NDznowd9iWEY3sn+8drD43pKm/zxYVFePeQoz
QkpcC4aMnYyMgLv2IH7XZdsvEPM4McJywZAvOfsUldSkLMHiTfJkvdodPBVozRIc
H9tlagGz0KgrYbWUzTw3MXncyF0i8P8XUEIStUuePLAxRzMdRY2geWCKb/8nxlXM
3B009Vq6XAo3ShpBjB0BFxQKvXT/I+nXNWDokf2ivsht6AOvAA4Y74v/6vw/4tV+
TSLjBUDMBMRJn3kFt7ZrXRhE98eVfXW0n2JRMmi0SanbR/mBwviXi7iud1T3h+7w
YtuNlI8hj3FVnvDE8bIqzd2tcIJQOvh2vOsDILpkwQKBgQDhdXPaKFXMF/MdHTKs
l8hesr0j46u/Ckn4cWQU6FX6PHRRajiCtJ2uvP5KcFW34Eoon8REM2Iivxodys5q
qJDljyvcVXwHCTnYtKmcUL5Ji8aqMrjXLQPEG2gwt2fUHY6G/AkgdnWON2hQRzdd
XjeN4VHsO8mQjHFiWDUfkkmzZQKBgQDH2VgcEGNZG/WHt7oqBaJ+kNmECafTAAXI
mtN2pyWvuc2ayUVd0tv/1p3hmQhSJ6UzSjgSs3RhMxYzSFdZY5Koxjh/Y16j/x6Y
vRqxcvBKx83qMzZbab2NBU2n3VR5Qvpg1fZM3hbbB0vBbzxbrL0b1t/qKjc0WFaM
FaKu+VQDbQKBgGNsrVNmeDeR1Ddhmeg84zLHtdsu2p1bxzUVpCIIN2or4MvKgPM6
/VKCq81d7p8w/OMfWakN09go2DaNKiwk/AkP8zKuTAy6R9VGDooNnWzHhCuoRJU3
l7KSt4bMSrBi/GiQmuHC+6Jk0s6cKVE2bF9YHw2DbCcfmBzbc0nh9Dh1AoGAB6j9
B5ZZOIEp2BniuNmecNt8euMj26KUlivZDyM4/pNQni44ym/anuPLCWqkNwHuAxlF
LPJT86XRpAWR04tNg8qVP8y/Q+nzckdNTp/pNfSSn/d2jepvqYgageSp6Dv4/N02
o5ufpKWS8cchuSHV3ctOqdsUYp1AM/5gTfSgk8ECgYEAjzNR+pSbeVzunLERAcSx
+sKll8dfP4wXGA+qclhXtSStkptql7uclQWv5xgcBYVucdOQ7QVpTkqNFTTGPcKc
piUvAD/Un0jtDl3KUMrl8Hki1mXirOK5BGWjdw/1T2PlFtr3YUrz4cGh+kA/KnEh
INH+0iAiwYvrcjG4lzgXlo8=
-----END PRIVATE KEY-----";
            RsaHelper rsa = new RsaHelper(RSAType.SHA1, Encoding.UTF8, privKey, pubKey);
            var r1 = rsa.Encrypt("Newcats");
            Console.WriteLine(r1);

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