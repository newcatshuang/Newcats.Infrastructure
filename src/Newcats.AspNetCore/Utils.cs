using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Newcats.AspNetCore
{
    public static class StringExtensions
    {
        #region System.Text.Json
        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object value)
        {
            return JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Json Serializer options</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object value, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="returnType">return type</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string json, Type returnType)
        {
            return JsonSerializer.Deserialize(json, returnType);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="returnType">return type</param>
        /// <param name="options">Json Serializer options</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string json, Type returnType, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(json, returnType, options);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="options">Json Serializer options</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeJson<T>(this string json, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        #endregion

        #region Substring
        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length length that begins at startIndex in this instance, or System.String.Empty if startIndex is equal to the length of this instance and length is zero.</returns>
        public static string ToSubstring(this string str, int length)
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
        public static string ToSubstring(this string str, int startIndex, int length)
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

    public class HttpHelper
    {
        /// <summary>
        /// 获取当前页面客户端的IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP(HttpContext context, bool tryUseXForwardHeader = true)
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

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }
    }
}