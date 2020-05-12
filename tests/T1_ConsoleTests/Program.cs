using System;
using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newcats.Utils.Extension;

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
                Season = Season.Summer,
                IsMan = true
            };

            string rawJson = "{\"Id\":1,\"Name\":\"newcats\",\"CN\":\"皇权特许\",\"CreateTime\":\"2020-05-10 23:36:03\",\"UpdateTime\":\"2020-05-11 02:36:03\",\"Season\":1,\"IsMan\":\"true\"}";

            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                PropertyNameCaseInsensitive = true//反序列化
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            opt.Converters.Add(new LongToStringConverter());//反序列化
            opt.Converters.Add(new IntToStringConverter());//反序列化
            opt.Converters.Add(new BoolConverter());//反序列化
            string json = JsonSerializer.Serialize<User>(u, opt);
            //Console.WriteLine(json + "\r\n");

            //string time = "\"2020-05-11 23:36:03\"";
            //var ss = DateTime.Parse(time);

            //Console.WriteLine(DateTime.Now.ToShortDateString());

            //DateTime d = JsonSerializer.Deserialize<DateTime>(time, opt);

            //Console.WriteLine(d.ToString());

            //User n = JsonSerializer.Deserialize<User>(rawJson, opt);

            //Console.WriteLine(n.CreateTime.ToString());


            Console.WriteLine(u.ToJson());

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


    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public string DateTimeFormat { get; set; }

        public DateTimeConverter(string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff")
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

    public class DateTimeNullConverter : JsonConverter<DateTime?>
    {
        public string DateTimeFormat { get; set; }

        public DateTimeNullConverter(string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff")
        {
            DateTimeFormat = dateTimeFormat;
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return string.IsNullOrWhiteSpace(reader.GetString()) ? default(DateTime?) : DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString(DateTimeFormat));
        }
    }

    public class LongToStringConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (System.Buffers.Text.Utf8Parser.TryParse(span, out long number, out int bytesConsumed) && span.Length == bytesConsumed)
                    return number;

                if (Int64.TryParse(reader.GetString(), out number))
                    return number;
            }

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long longValue, JsonSerializerOptions options)
        {
            writer.WriteStringValue(longValue.ToString());
        }
    }

    public class IntToStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (System.Buffers.Text.Utf8Parser.TryParse(span, out int number, out int bytesConsumed) && span.Length == bytesConsumed)
                    return number;

                if (Int32.TryParse(reader.GetString(), out number))
                    return number;
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int intValue, JsonSerializerOptions options)
        {
            writer.WriteStringValue(intValue.ToString());
        }
    }

    public class BoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
                return reader.GetBoolean();

            return bool.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
