using System;
using Newcats.Utils.Extension;

namespace T1_ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
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

            Console.WriteLine(u.ToJson());

            var sss = rawJson.Deserialize<User>();

            Console.WriteLine(sss.CreateTime.ToChinaString());
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