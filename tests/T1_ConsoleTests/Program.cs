using System;
using System.Diagnostics;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Newcats.Utils.Extensions;
using Newcats.Utils.Helpers;

namespace T1_ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<TestContext>();
            Console.ReadLine();
            return;

            User u = new User()
            {
                Id = 1,
                Name = "newcats",
                CN = "皇权特许",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now.AddHours(3),
                Season = Season.Summer,
                IsMan = true
            };

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



    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CN { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public Season Season { get; set; }

        public bool IsMan { get; set; }
    }

    public enum Season
    {
        Spring = 0,

        Summer = 1,

        Fall = 2,

        Winter = 3
    }
}