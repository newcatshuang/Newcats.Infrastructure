using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace T1_ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            User u = new User()
            {
                Id = 1,
                Name = "newcats",
                CN = "皇权特许",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now.AddHours(3),
                Season = Season.Summer
            };

            string rawJson = "{\"Id\":1,\"Name\":\"newcats\",\"CN\":\"皇权特许\",\"CreateTime\":\"2020-05-10 23:36:03\",\"UpdateTime\":\"2020-05-11 02:36:03\",\"Season\":1}";

            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };
            opt.Converters.Add(new DateTimeJsonConverter());
            string json = JsonSerializer.Serialize<User>(u, opt);
            Console.WriteLine(json);

            User n = JsonSerializer.Deserialize<User>(rawJson);

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
    }

    public enum Season
    {
        Spring = 0,

        Summer = 1,

        Fall = 2,

        Winter = 3
    }

    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public string DateTimeFormat { get; set; }

        public DateTimeJsonConverter(string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff")
        {
            DateTimeFormat = dateTimeFormat;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeFormat));
        }
    }
}
