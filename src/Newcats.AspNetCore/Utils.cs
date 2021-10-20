using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Newcats.AspNetCore
{
    internal static class StringExtensions
    {
        #region System.Text.Json
        /// <summary>
        /// Converts the value of a specified type into a JSON string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The JSON string representation of the value.</returns>
        internal static string ToJson(this object value)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            return JsonSerializer.Serialize(value, value?.GetType(), opt);
        }

        /// <summary>
        /// Converts the value of a type specified by a generic type parameter into a JSON string.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A JSON string representation of the value.</returns>
        internal static string ToJson<TValue>(this TValue value)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            return JsonSerializer.Serialize<TValue>(value, opt);
        }
        #endregion

        #region Substring
        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length length that begins at startIndex in this instance, or System.String.Empty if startIndex is equal to the length of this instance and length is zero.</returns>
        internal static string ToSubstring(this string str, int length)
        {
            return str.ToSubstring(0, length);
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length length that begins at startIndex in this instance, or System.String.Empty if startIndex is equal to the length of this instance and length is zero.</returns>
        internal static string ToSubstring(this string str, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            if (startIndex >= str.Length)
                return string.Empty;
            if (startIndex + length >= str.Length)
                return str.Substring(startIndex);
            return str.Substring(startIndex, length);
        }
        #endregion
    }

    #region System.Text.Json的自定义转换器
    /// <summary>
    /// System.Text.Json的自定义DateTime转换器(序列号和反序列化)
    /// </summary>
    internal class DateTimeConverter : JsonConverter<DateTime>
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

    /// <summary>
    /// System.Text.Json的自定义DateTime?转换器(序列号和反序列化)
    /// </summary>
    internal class DateTimeNullConverter : JsonConverter<DateTime?>
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
    #endregion

    internal class HttpHelper
    {
        /// <summary>
        /// 获取当前页面客户端的IP地址
        /// </summary>
        /// <returns></returns>
        internal static string GetIP(HttpContext context, bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
            {
                ip = GetHeaderValueAs<string>(context, "X-Forwarded-For");//.TrimEnd(',').Split(',').AsEnumerable().Select(s => s.Trim()).ToList().FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ip))
                    ip = ip.TrimEnd(',').Split(',').AsEnumerable().Select(s => s.Trim()).ToList().FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(ip) && context.Connection?.RemoteIpAddress != null)
                ip = context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");

            if (string.IsNullOrWhiteSpace(ip))
                ip = "0.0.0.0";

            return ip;
        }

        private static T GetHeaderValueAs<T>(HttpContext context, string headerName)
        {
            if (context.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrWhiteSpace(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }
    }
}