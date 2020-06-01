using System;
using System.Buffers;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Newcats.Utils.Extensions
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

    /// <summary>
    /// 字符串类型扩展方法
    /// </summary>
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
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>16位Md5加密结果</returns>
        public static string MD5By16(this string value)
        {
            return Helpers.EncryptHelper.MD5By16(value);
        }

        /// <summary>
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>16位Md5加密结果</returns>
        public static string MD5By16(this string value, Encoding encoding)
        {
            return Helpers.EncryptHelper.MD5By16(value, encoding);
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>32位Md5加密结果</returns>
        public static string MD5By32(this string value)
        {
            return Helpers.EncryptHelper.MD5By32(value);
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>32位Md5加密结果</returns>
        public static string MD5By32(this string value, Encoding encoding)
        {
            return Helpers.EncryptHelper.MD5By32(value, encoding);
        }

        /// <summary>
        /// 获取此字符串的Sha1值
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>此字符串的Sha1值</returns>
        public static string Sha1(this string value)
        {
            return Helpers.EncryptHelper.Sha1(value);
        }

        /// <summary>
        /// 获取此字符串的Sha1值
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>此字符串的Sha1值</returns>
        public static string Sha1(this string value, Encoding encoding)
        {
            return Helpers.EncryptHelper.Sha1(value, encoding);
        }

        /// <summary>
        /// 获取此字符串的Sha256值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>此字符串的Sha256值</returns>
        public static string Sha256(this string str)
        {
            return Helpers.EncryptHelper.Sha256(str);
        }

        /// <summary>
        /// 获取此字符串的Sha256值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>此字符串的Sha256值</returns>
        public static string Sha256(this string str, Encoding encoding)
        {
            return Helpers.EncryptHelper.Sha256(str, encoding);
        }

        /// <summary>
        /// 获取此字符串的DES加密结果
        /// </summary>
        /// <param name="value">字符串(明文)</param>
        /// <param name="key">密钥(24位)</param>
        /// <returns>DES加密结果</returns>
        public static string DesEncrypt(this string value, string key)
        {
            return Helpers.EncryptHelper.DESEncrypt(value, key);
        }

        /// <summary>
        /// 获取此字符串的DES加密结果
        /// </summary>
        /// <param name="value">字符串(明文)</param>
        /// <param name="key">密钥(24位)</param>
        /// <param name="encoding">编码</param>
        /// <returns>DES加密结果</returns>
        public static string DesEncrypt(this string value, string key, Encoding encoding)
        {
            return Helpers.EncryptHelper.DESEncrypt(value, key, encoding);
        }

        /// <summary>
        /// 获取此字符串的DES解密结果
        /// </summary>
        /// <param name="value">字符串(密文)</param>
        /// <param name="key">密钥(24位)</param>
        /// <returns>DES解密结果</returns>
        public static string DesDecrypt(this string value, string key)
        {
            return Helpers.EncryptHelper.DESDecrypt(value, key);
        }

        /// <summary>
        /// 获取此字符串的DES解密结果
        /// </summary>
        /// <param name="value">字符串(密文)</param>
        /// <param name="key">密钥(24位)</param>
        /// <param name="encoding">编码</param>
        /// <returns>DES解密结果</returns>
        public static string DesDecrypt(this string value, string key, Encoding encoding)
        {
            return Helpers.EncryptHelper.DESDecrypt(value, key, encoding);
        }
        #endregion

        #region System.Text.Json
        /// <summary>
        /// Converts the value of a specified type into a JSON string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The JSON string representation of the value.</returns>
        public static string ToJson(this object value)
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
        /// Converts the value of a specified type into a JSON string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>The JSON string representation of the value.</returns>
        public static string ToJson(this object value, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(value, value?.GetType(), options);
        }

        /// <summary>
        /// Converts the value of a specified type into a JSON string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the value to convert.</param>
        /// <returns>The JSON string representation of the value.</returns>
        public static string ToJson(this object value, Type inputType)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            return JsonSerializer.Serialize(value, inputType, opt);
        }

        /// <summary>
        /// Converts the value of a specified type into a JSON string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="inputType">The type of the value to convert.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>The JSON string representation of the value.</returns>
        public static string ToJson(this object value, Type inputType, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(value, inputType, options);
        }

        /// <summary>
        /// Converts the value of a type specified by a generic type parameter into a JSON string.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A JSON string representation of the value.</returns>
        public static string ToJson<TValue>(this TValue value)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            return JsonSerializer.Serialize<TValue>(value, opt);
        }

        /// <summary>
        /// Converts the value of a type specified by a generic type parameter into a JSON string.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">Options to control serialization behavior.</param>
        /// <returns>A JSON string representation of the value.</returns>
        public static string ToJson<TValue>(this TValue value, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize<TValue>(value, options);
        }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of a specified type.
        /// </summary>
        /// <param name="json">The JSON text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <returns>A returnType representation of the JSON value.</returns>
        /// <exception cref="ArgumentNullException">json or returnType is null.</exception>
        /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
        public static object Deserialize(this string json, Type returnType)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true//大小写不敏感
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            opt.Converters.Add(new LongToStringConverter());//反序列化
            opt.Converters.Add(new IntToStringConverter());//反序列化
            opt.Converters.Add(new BoolConverter());//反序列化

            return JsonSerializer.Deserialize(json, returnType, opt);
        }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of a specified type.
        /// </summary>
        /// <param name="json">The JSON text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <returns>A returnType representation of the JSON value.</returns>
        /// <exception cref="ArgumentNullException">json or returnType is null.</exception>
        /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
        public static object Deserialize(this string json, Type returnType, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(json, returnType, options);
        }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of the type specified by a generic type parameter.
        /// </summary>
        /// <param name="json">The JSON text to parse.</param>
        /// <returns>A TValue representation of the JSON value.</returns>
        /// <exception cref="ArgumentNullException">json is null.</exception>
        /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
        public static TValue Deserialize<TValue>(this string json)
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true//大小写不敏感
            };
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeNullConverter());
            opt.Converters.Add(new LongToStringConverter());//反序列化
            opt.Converters.Add(new IntToStringConverter());//反序列化
            opt.Converters.Add(new BoolConverter());//反序列化

            return JsonSerializer.Deserialize<TValue>(json, opt);
        }

        /// <summary>
        /// Parses the text representing a single JSON value into an instance of the type specified by a generic type parameter.
        /// </summary>
        /// <param name="json">The JSON text to parse.</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <returns>A TValue representation of the JSON value.</returns>
        /// <exception cref="ArgumentNullException">json is null.</exception>
        /// <exception cref="JsonException">The JSON is invalid. -or- TValue is not compatible with the JSON. -or- There is remaining data in the string beyond a single JSON value.</exception>
        public static TValue Deserialize<TValue>(this string json, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TValue>(json, options);
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Converts the value of the current System.DateTime object to a default Chinese format string (yyyy-MM-dd HH:mm:ss.fff).
        /// </summary>
        /// <param name="value">The date and time value</param>
        /// <returns>A string representation of value of the current System.DateTime object as specified by format (yyyy-MM-dd HH:mm:ss.fff).</returns>
        public static string ToChinaString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 转换为Unix时间戳(秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>Unix时间戳(秒)</returns>
        public static long ToUnixTimestamp(this DateTime time)
        {
            var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            long ticks = (time - start.Add(new TimeSpan(8, 0, 0))).Ticks;
            return Convert.ToInt64(ticks / TimeSpan.TicksPerSecond);
        }
        #endregion

        #region System.Int64
        /// <summary>
        /// 从Unix时间戳获取时间(秒)
        /// </summary>
        /// <param name="timestamp">Unix时间戳(秒)</param>
        /// <returns>DateTime时间</returns>
        public static DateTime GetTimeFromUnixTimestamp(this long timestamp)
        {
            var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            TimeSpan span = new TimeSpan(long.Parse(timestamp + "0000000"));
            return start.Add(span).Add(new TimeSpan(8, 0, 0));
        }
        #endregion

        #region Substring
        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or System.String.Empty, or if value consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

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

        #region PinYin(获取汉字的拼音简码)
        /// <summary>
        /// 获取汉字的拼音简码，即首字母缩写,范例：中国,返回zg
        /// </summary>
        /// <param name="chineseText">汉字文本,范例： 中国</param>
        /// <returns>拼音简码,范例：中国,返回zg</returns>
        public static string PinYin(this string chineseText)
        {
            if (string.IsNullOrWhiteSpace(chineseText))
                return string.Empty;
            var result = new StringBuilder();
            foreach (char text in chineseText)
                result.AppendFormat("{0}", ResolvePinYin(text));
            return result.ToString().ToLower();
        }

        /// <summary>
        /// 解析单个汉字的拼音简码
        /// </summary>
        private static string ResolvePinYin(char text)
        {
            byte[] charBytes = Encoding.UTF8.GetBytes(text.ToString());
            if (charBytes[0] <= 127)
                return text.ToString();
            var unicode = (ushort)(charBytes[0] * 256 + charBytes[1]);
            string pinYin = ResolveByCode(unicode);
            if (!string.IsNullOrWhiteSpace(pinYin))
                return pinYin;
            return ResolveByConst(text.ToString());
        }

        /// <summary>
        /// 使用字符编码方式获取拼音简码
        /// </summary>
        private static string ResolveByCode(ushort unicode)
        {
            if (unicode >= '\uB0A1' && unicode <= '\uB0C4')
                return "A";
            if (unicode >= '\uB0C5' && unicode <= '\uB2C0' && unicode != 45464)
                return "B";
            if (unicode >= '\uB2C1' && unicode <= '\uB4ED')
                return "C";
            if (unicode >= '\uB4EE' && unicode <= '\uB6E9')
                return "D";
            if (unicode >= '\uB6EA' && unicode <= '\uB7A1')
                return "E";
            if (unicode >= '\uB7A2' && unicode <= '\uB8C0')
                return "F";
            if (unicode >= '\uB8C1' && unicode <= '\uB9FD')
                return "G";
            if (unicode >= '\uB9FE' && unicode <= '\uBBF6')
                return "H";
            if (unicode >= '\uBBF7' && unicode <= '\uBFA5')
                return "J";
            if (unicode >= '\uBFA6' && unicode <= '\uC0AB')
                return "K";
            if (unicode >= '\uC0AC' && unicode <= '\uC2E7')
                return "L";
            if (unicode >= '\uC2E8' && unicode <= '\uC4C2')
                return "M";
            if (unicode >= '\uC4C3' && unicode <= '\uC5B5')
                return "N";
            if (unicode >= '\uC5B6' && unicode <= '\uC5BD')
                return "O";
            if (unicode >= '\uC5BE' && unicode <= '\uC6D9')
                return "P";
            if (unicode >= '\uC6DA' && unicode <= '\uC8BA')
                return "Q";
            if (unicode >= '\uC8BB' && unicode <= '\uC8F5')
                return "R";
            if (unicode >= '\uC8F6' && unicode <= '\uCBF9')
                return "S";
            if (unicode >= '\uCBFA' && unicode <= '\uCDD9')
                return "T";
            if (unicode >= '\uCDDA' && unicode <= '\uCEF3')
                return "W";
            if (unicode >= '\uCEF4' && unicode <= '\uD188')
                return "X";
            if (unicode >= '\uD1B9' && unicode <= '\uD4D0')
                return "Y";
            if (unicode >= '\uD4D1' && unicode <= '\uD7F9')
                return "Z";
            return string.Empty;
        }

        /// <summary>
        /// 通过拼音简码常量获取
        /// </summary>
        private static string ResolveByConst(string text)
        {
            int index = ConstContent.ChinesePinYin.IndexOf(text, StringComparison.Ordinal);
            if (index < 0)
                return string.Empty;
            return ConstContent.ChinesePinYin.Substring(index + 1, 1);
        }

        #endregion

        #region Regex
        /// <summary>
        /// 是否为数字(默认判断为纯数字，不包含小数点和负号)
        /// </summary>
        /// <param name="input">输入值</param>
        /// <param name="pure">true:只包含数字，false:包含小数和负数</param>
        /// <returns>true or false</returns>
        public static bool IsNumber(this string input, bool pure = true)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            if (input[0] == '.')//不能开头
                return false;
            if (input.LastIndexOf('.') == input.Length - 1)//不能结尾
                return false;
            if (input.IndexOf('.') != input.LastIndexOf('.'))//只能有一个
                return false;
            if (input.LastIndexOf('-') > 0)//只能开头
                return false;
            if (input.IndexOf('-') != input.LastIndexOf('-'))//只能有一个
                return false;
            if (input.Length > 2 && input[0] == '-' && input[1] == '.')
                return false;
            return pure ? Regex.IsMatch(input, "^[0-9]*$") : Regex.IsMatch(input, @"^(-?\d*)(\.\d+)?$");
        }

        /// <summary>
        /// 是否为中国大陆手机号码
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>true or false</returns>
        public static bool IsPhoneNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return value.Length == 11 && value.IsNumber() &&
                (value.StartsWith("13") || value.StartsWith("14") || value.StartsWith("15") || value.StartsWith("16")
                || value.StartsWith("17") || value.StartsWith("18") || value.StartsWith("19"));
        }

        /// <summary>
        /// 加密中国大陆手机号码（例：13000000000->130*****000）（手机号码验证不通过则不加密）
        /// </summary>
        /// <param name="phoneNumber">中国大陆手机号码</param>
        /// <returns>例：13000000000->130*****000</returns>
        public static string EncryptPhoneNumber(this string phoneNumber)
        {
            if (!phoneNumber.IsPhoneNumber())
                return phoneNumber;

            return $"{phoneNumber.Substring(0, 3)}*****{phoneNumber.Substring(phoneNumber.Length - 3)}";
        }

        /// <summary>
        /// 是否为中国大陆身份证号码
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>true or false</returns>
        public static bool IsIDCardNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (value.Length != 15 && value.Length != 18)
                return false;
            return value.Length == 15 ?
                Regex.IsMatch(value, @"^[1-9]\d{5}\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}$") :
                Regex.IsMatch(value, @"^[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$");
        }

        /// <summary>
        /// 是否中国汉字
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>true or false</returns>
        public static bool IsChineseCharacter(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, "^[\u4e00-\u9fa5]{0,}$");
        }

        /// <summary>
        /// 是否Email地址
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>true or false</returns>
        public static bool IsEmail(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// <summary>
        /// 是否IPv4地址
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>true or false</returns>
        public static bool IsIPv4(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Regex.IsMatch(value, @"\d+\.\d+\.\d+\.\d+");
        }
        #endregion
    }

    #region System.Text.Json的自定义转换器
    /// <summary>
    /// System.Text.Json的自定义DateTime转换器(序列号和反序列化)
    /// </summary>
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

    /// <summary>
    /// System.Text.Json的自定义DateTime?转换器(序列号和反序列化)
    /// </summary>
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

    /// <summary>
    /// System.Text.Json的自定义long转换器(反序列化)
    /// </summary>
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

    /// <summary>
    /// System.Text.Json的自定义int转换器(反序列化)
    /// </summary>
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

    /// <summary>
    /// System.Text.Json的自定义bool转换器(反序列化)
    /// </summary>
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
    #endregion
}