using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace Newcats.Utils.Extension
{
    public enum SpanColor
    {
        /// <summary>
        /// Primary(#5867dd)
        /// </summary>
        [Description("#5867dd")]
        Primary = 0,

        /// <summary>
        /// Success(#34bfa3)
        /// </summary>
        [Description("#34bfa3")]
        Success = 1,

        /// <summary>
        /// Warning(#ffb822)
        /// </summary>
        [Description("#ffb822")]
        Warning = 2,

        /// <summary>
        /// Danger(#f4516c)
        /// </summary>
        [Description("#f4516c")]
        Danger = 3,

        /// <summary>
        /// Metal(#c4c5d6)
        /// </summary>
        [Description("#c4c5d6")]
        Metal = 4,

        /// <summary>
        /// Brand(#716aca)
        /// </summary>
        [Description("#716aca")]
        Brand = 5,

        /// <summary>
        /// Info(#36a3f7)
        /// </summary>
        [Description("#36a3f7")]
        Info = 6,

        /// <summary>
        /// Focus(#9816f4)
        /// </summary>
        [Description("#9816f4")]
        Focus = 7
    }

    public static class StringExtensions
    {
        #region Html
        /// <summary>
        /// 根据枚举颜色，生成字符串的Span类型html标签
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="color">颜色枚举</param>
        /// <returns>生成的span标签</returns>
        public static string GetSpanHtml(this string str, SpanColor color)
        {
            switch (color)
            {
                case SpanColor.Primary:
                    str = $"<span class='label label-sm' style='background-color:#5867dd'>{str}</span>";
                    break;
                case SpanColor.Success:
                    str = $"<span class='label label-sm' style='background-color:#34bfa3'>{str}</span>";
                    break;
                case SpanColor.Warning:
                    str = $"<span class='label label-sm' style='background-color:#ffb822'>{str}</span>";
                    break;
                case SpanColor.Danger:
                    str = $"<span class='label label-sm' style='background-color:#f4516c'>{str}</span>";
                    break;
                case SpanColor.Metal:
                    str = $"<span class='label label-sm' style='background-color:#c4c5d6'>{str}</span>";
                    break;
                case SpanColor.Brand:
                    str = $"<span class='label label-sm' style='background-color:#716aca'>{str}</span>";
                    break;
                case SpanColor.Info:
                    str = $"<span class='label label-sm' style='background-color:#36a3f7'>{str}</span>";
                    break;
                case SpanColor.Focus:
                    str = $"<span class='label label-sm' style='background-color:#9816f4'>{str}</span>";
                    break;
                default:
                    break;
            }
            return str;
        }

        /// <summary>
        /// 根据给定的16进制颜色，生成字符串的Span类型html标签
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="colorString">16进制颜色（#00c5dc）</param>
        /// <returns>生成的span标签</returns>
        public static string GetSpanHtml(this string str, string colorString)
        {
            return $"<span class='label label-sm' style='background-color:{colorString}'>{str}</span>";
        }
        #endregion

        #region Encrypt
        /// <summary>
        /// 获取此字符串的Sha256值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>此字符串的Sha256值</returns>
        public static string Sha256(this string str)
        {
            return Encrypt.EncryptHelper.Sha256(str);
        }

        /// <summary>
        /// 获取此字符串的Sha256值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>此字符串的Sha256值</returns>
        public static string Sha256(this string str, Encoding encoding)
        {
            return Encrypt.EncryptHelper.Sha256(str, encoding);
        }
        #endregion

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

        #region Newtonsoft.Json
        /*
            /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using formatting.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string ToJson(this object value, Formatting formatting)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, formatting);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object value, params JsonConverter[] converters)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, converters);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using formatting and a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object value, Formatting formatting, params JsonConverter[] converters)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, formatting, converters);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string ToJson(this object value, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string ToJson(this object value, Type type, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, type, settings);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string ToJson(this object value, Formatting formatting, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, formatting, settings);
        }

        /// <summary>
        /// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string ToJson(this object value, Type type, Formatting formatting, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, type, formatting, settings);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value);
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="settings">
        /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string value, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value, settings);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The <see cref="T:System.Type" /> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string value, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeJson<T>(this string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Deserializes the JSON to the given anonymous type.
        /// </summary>
        /// <typeparam name="T">
        /// The anonymous type to deserialize to. This can't be specified
        /// traditionally and must be inferred from the anonymous type passed
        /// as a parameter.
        /// </typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <returns>The deserialized anonymous type from the JSON string.</returns>
        public static T DeserializeJson<T>(this string value, T anonymousTypeObject)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeAnonymousType<T>(value, anonymousTypeObject);
        }

        /// <summary>
        /// Deserializes the JSON to the given anonymous type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <typeparam name="T">
        /// The anonymous type to deserialize to. This can't be specified
        /// traditionally and must be inferred from the anonymous type passed
        /// as a parameter.
        /// </typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <param name="settings">
        /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized anonymous type from the JSON string.</returns>
        public static T DeserializeJson<T>(this string value, T anonymousTypeObject, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeAnonymousType<T>(value, anonymousTypeObject, settings);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeJson<T>(this string value, params JsonConverter[] converters)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value, converters);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The object to deserialize.</param>
        /// <param name="settings">
        /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeJson<T>(this string value, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value, settings);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string value, Type type, params JsonConverter[] converters)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type, converters);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
        /// </summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeJson(this string value, Type type, JsonSerializerSettings settings)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value, type, settings);
        }
        */
        #endregion

        #region DateTime
        /// <summary>
        /// Converts the value of the current System.DateTime object to a default Chinese format string (yyyy-MM-dd HH:mm:ss).
        /// </summary>
        /// <param name="value">The date and time value</param>
        /// <returns>A string representation of value of the current System.DateTime object as specified by format (yyyy-MM-dd HH:mm:ss).</returns>
        public static string ToChinaString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
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
}