/***************************************************************************
 *GUID: 1180c01a-813e-4522-813f-40292a04d5aa
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-08 17:19:09
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

using System.Buffers;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Newcats.Utils.Extensions;

/// <summary>
/// 16进制颜色枚举
/// </summary>
public enum SpanColorEnum
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
    public static string GetSpanHtml(this string str, SpanColorEnum color)
    {
        switch (color)
        {
            case SpanColorEnum.Primary:
                str = $"<span class='label label-sm' style='background-color:#5867dd'>{str}</span>";
                break;
            case SpanColorEnum.Success:
                str = $"<span class='label label-sm' style='background-color:#34bfa3'>{str}</span>";
                break;
            case SpanColorEnum.Warning:
                str = $"<span class='label label-sm' style='background-color:#ffb822'>{str}</span>";
                break;
            case SpanColorEnum.Danger:
                str = $"<span class='label label-sm' style='background-color:#f4516c'>{str}</span>";
                break;
            case SpanColorEnum.Metal:
                str = $"<span class='label label-sm' style='background-color:#c4c5d6'>{str}</span>";
                break;
            case SpanColorEnum.Brand:
                str = $"<span class='label label-sm' style='background-color:#716aca'>{str}</span>";
                break;
            case SpanColorEnum.Info:
                str = $"<span class='label label-sm' style='background-color:#36a3f7'>{str}</span>";
                break;
            case SpanColorEnum.Focus:
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
    /// 获取此字符串的DES加密结果(默认使用TripleDES算法)(使用默认的24位密钥)
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES加密结果</returns>
    public static string DesEncrypt(this string value, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesEncrypt(value, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的DES加密结果(默认使用TripleDES算法)
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES加密结果</returns>
    public static string DesEncrypt(this string value, string key, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesEncrypt(value, key, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的DES加密结果(默认使用TripleDES算法)
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
    /// <param name="encoding">编码</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES加密结果</returns>
    public static string DesEncrypt(this string value, string key, Encoding encoding, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesEncrypt(value, key, encoding, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的DES解密结果(默认使用TripleDES算法)(使用默认的24位密钥)
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES解密结果</returns>
    public static string DesDecrypt(this string value, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesDecrypt(value, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的DES解密结果(默认使用TripleDES算法)
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES解密结果</returns>
    public static string DesDecrypt(this string value, string key, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesDecrypt(value, key, Encoding.UTF8, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的DES解密结果(默认使用TripleDES算法)
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
    /// <param name="encoding">编码</param>
    /// <param name="tripleDES">是否使用TripleDES算法</param>
    /// <returns>DES解密结果</returns>
    public static string DesDecrypt(this string value, string key, Encoding encoding, bool tripleDES = true)
    {
        return Helpers.EncryptHelper.DesDecrypt(value, key, encoding, tripleDES);
    }

    /// <summary>
    /// 获取此字符串的Aes加密结果(使用默认密钥和向量)
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <returns>Aes加密结果</returns>
    public static string AesEncrypt(this string value)
    {
        return Helpers.EncryptHelper.AesEncrypt(value);
    }

    /// <summary>
    /// 获取此字符串的Aes加密结果
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <param name="key">Aes密钥,32位字符串</param>
    /// <param name="iv">Aes向量,16位字符串</param>
    /// <returns>Aes加密结果</returns>
    public static string AesEncrypt(this string value, string key, string iv)
    {
        return Helpers.EncryptHelper.AesEncrypt(value, key, iv, Encoding.UTF8);
    }

    /// <summary>
    /// 获取此字符串的Aes加密结果
    /// </summary>
    /// <param name="value">字符串(明文)</param>
    /// <param name="key">Aes密钥,32位字符串</param>
    /// <param name="iv">Aes向量,16位字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns>Aes加密结果</returns>
    public static string AesEncrypt(this string value, string key, string iv, Encoding encoding)
    {
        return Helpers.EncryptHelper.AesEncrypt(value, key, iv, encoding);
    }

    /// <summary>
    /// 获取此字符串的Aes解密结果(使用默认密钥和向量)
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <returns>Aes解密结果</returns>
    public static string AesDecrypt(this string value)
    {
        return Helpers.EncryptHelper.AesDecrypt(value);
    }

    /// <summary>
    /// 获取此字符串的Aes解密结果
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <param name="key">Aes密钥,32位字符串</param>
    /// <param name="iv">Aes向量,16位字符串</param>
    /// <returns>Aes解密结果</returns>
    public static string AesDecrypt(this string value, string key, string iv)
    {
        return Helpers.EncryptHelper.AesDecrypt(value, key, iv, Encoding.UTF8);
    }

    /// <summary>
    /// 获取此字符串的Aes解密结果
    /// </summary>
    /// <param name="value">字符串(密文)</param>
    /// <param name="key">Aes密钥,32位字符串</param>
    /// <param name="iv">Aes向量,16位字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns>Aes解密结果</returns>
    public static string AesDecrypt(this string value, string key, string iv, Encoding encoding)
    {
        return Helpers.EncryptHelper.AesDecrypt(value, key, iv, encoding);
    }

    /// <summary>
    /// 使用Rsa加密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充,默认公钥
    /// </summary>
    /// <param name="data">要加密的字符串</param>
    /// <returns>Base64编码的加密字符串</returns>
    public static string RsaEncrypt(this string data)
    {
        return Helpers.EncryptHelper.RsaEncrypt(data);
    }

    /// <summary>
    /// 使用Rsa加密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要加密的字符串</param>
    /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
    /// <returns>Base64编码的加密字符串</returns>
    public static string RsaEncrypt(this string data, string publicKey)
    {
        return Helpers.EncryptHelper.RsaEncrypt(data, publicKey);
    }

    /// <summary>
    /// 使用Rsa加密字符串
    /// </summary>
    /// <param name="data">要加密的字符串</param>
    /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
    /// <param name="padding">填充方式</param>
    /// <param name="encoding">编码方式</param>
    /// <returns>Base64编码的加密字符串</returns>
    public static string RsaEncrypt(this string data, string publicKey, RSAEncryptionPadding padding, Encoding encoding)
    {
        return Helpers.EncryptHelper.RsaEncrypt(data, publicKey, padding, encoding);
    }

    /// <summary>
    /// 使用Rsa解密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充.默认私钥
    /// </summary>
    /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
    /// <returns>解密之后的UTF8编码字符串</returns>
    public static string RsaDecrypt(this string data)
    {
        return Helpers.EncryptHelper.RsaDecrypt(data);
    }

    /// <summary>
    /// 使用Rsa解密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <returns>解密之后的UTF8编码字符串</returns>
    public static string RsaDecrypt(this string data, string privateKey)
    {
        return Helpers.EncryptHelper.RsaDecrypt(data, privateKey);
    }

    /// <summary>
    /// 使用Rsa解密字符串
    /// </summary>
    /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <param name="padding">填充方式</param>
    /// <param name="encoding">编码方式</param>
    /// <returns>解密之后的指定编码字符串</returns>
    public static string RsaDecrypt(this string data, string privateKey, RSAEncryptionPadding padding, Encoding encoding)
    {
        return Helpers.EncryptHelper.RsaDecrypt(data, privateKey, padding, encoding);
    }

    /// <summary>
    /// 使用Rsa对数据签名,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据,默认私钥
    /// </summary>
    /// <param name="data">要加签的数据</param>
    /// <returns>签名之后的Base64编码字符串</returns>
    public static string RsaSignData(this string data)
    {
        return Helpers.EncryptHelper.RsaSignData(data);
    }

    /// <summary>
    /// 使用Rsa对数据签名,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据
    /// </summary>
    /// <param name="data">要加签的数据</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <returns>签名之后的Base64编码字符串</returns>
    public static string RsaSignData(this string data, string privateKey)
    {
        return Helpers.EncryptHelper.RsaSignData(data, privateKey);
    }

    /// <summary>
    /// 使用Rsa对数据签名
    /// </summary>
    /// <param name="data">要加签的数据</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <param name="hashAlgorithm">指定Hash算法</param>
    /// <param name="padding">填充方式</param>
    /// <param name="encoding">数据编码方式</param>
    /// <returns>签名之后的Base64编码字符串</returns>
    public static string RsaSignData(this string data, string privateKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding, Encoding encoding)
    {
        return Helpers.EncryptHelper.RsaSignData(data, privateKey, hashAlgorithm, padding, encoding);
    }

    /// <summary>
    /// Rsa验签,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据,默认公钥
    /// </summary>
    /// <param name="data">要验证的数据</param>
    /// <param name="signature">Base64编码签名</param>
    /// <returns>数据签名是否一致</returns>
    public static bool RsaVerifyData(this string data, string signature)
    {
        return Helpers.EncryptHelper.RsaVerifyData(data, signature);
    }

    /// <summary>
    /// Rsa验签,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据
    /// </summary>
    /// <param name="data">要验证的数据</param>
    /// <param name="signature">Base64编码签名</param>
    /// <param name="publicKey">Rsa公钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <returns>数据签名是否一致</returns>
    public static bool RsaVerifyData(this string data, string signature, string publicKey)
    {
        return Helpers.EncryptHelper.RsaVerifyData(data, signature, publicKey);
    }

    /// <summary>
    /// Rsa验签
    /// </summary>
    /// <param name="data">要验证的数据</param>
    /// <param name="signature">Base64编码签名</param>
    /// <param name="publicKey">Rsa公钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <param name="hashAlgorithm">指定Hash算法</param>
    /// <param name="padding">填充方式</param>
    /// <param name="encoding">数据编码方式</param>
    /// <returns>数据签名是否一致</returns>
    public static bool RsaVerifyData(this string data, string signature, string publicKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding, Encoding encoding)
    {
        return Helpers.EncryptHelper.RsaVerifyData(data, signature, publicKey, hashAlgorithm, padding, encoding);
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
        JsonSerializerOptions opt = new()
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
        JsonSerializerOptions opt = new()
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
        JsonSerializerOptions opt = new()
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
        JsonSerializerOptions opt = new()
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
        JsonSerializerOptions opt = new()
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
        const long std = 621355968000000000L;//new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        long give = time.ToUniversalTime().Ticks;
        return (give - std) / TimeSpan.TicksPerSecond;
    }
    #endregion

    #region System.Int64
    /// <summary>
    /// 从Unix时间戳获取时间(秒)(默认返回北京时间)
    /// </summary>
    /// <param name="timestamp">UTC时间的Unix时间戳(秒)</param>
    /// <param name="beijingTimeZone">是否转换成北京时间</param>
    /// <returns>DateTime时间</returns>
    public static DateTime GetTimeFromUnixTimestamp(this long timestamp, bool beijingTimeZone = true)
    {
        //var start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);
        //TimeSpan span = new(long.Parse(timestamp + "0000000"));
        //return start.Add(span).Add(new TimeSpan(8, 0, 0));

        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan span = new(long.Parse(timestamp + "0000000"));
        return beijingTimeZone ? start.Add(span).Add(new TimeSpan(8, 0, 0)) : start.Add(span);
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

    #region PinYin(获取汉字的拼音和拼音简码)
    /// <summary>
    /// 获取中文文本的拼音首字母(例如:中国=>zg)
    /// </summary>
    /// <param name="chineseText">编码为UTF8的中文文本</param>
    /// <returns>中文文本的拼音首字母</returns>
    public static string FirstPinYin(this string chineseText)
    {
        return Helpers.PinYinHelper.GetFirstPinYin(chineseText);
    }

    /// <summary>
    /// 获取中文文本的拼音(例如:中国=>ZhongGuo)
    /// </summary>
    /// <param name="chineseText">编码为UTF8的中文文本</param>
    /// <param name="separator">分隔符</param>
    /// <param name="upperFirst">首字母是否返回大写</param>
    /// <returns>中文文本的拼音</returns>
    public static string PinYin(this string chineseText, string separator = "", bool upperFirst = true)
    {
        return Helpers.PinYinHelper.GetPinYin(chineseText, separator, upperFirst);
    }

    /// <summary>
    /// 返回单个字符的汉字拼音(例如:中=>Zhong)
    /// </summary>
    /// <param name="ch">编码为UTF8的中文字符</param>
    /// <param name="separator">分隔符</param>
    /// <param name="upperFirst">首字母是否返回大写</param>
    /// <returns>字符对应的拼音</returns>
    public static string PinYin(this char ch, string separator = "", bool upperFirst = true)
    {
        return Helpers.PinYinHelper.GetPinYin(ch, separator, upperFirst);
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

        return phoneNumber.HideMiddle();
        //return $"{phoneNumber.Substring(0, 3)}*****{phoneNumber.Substring(phoneNumber.Length - 3)}";
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

    /// <summary>
    /// 隐藏中间的字符串，用*替代
    /// </summary>
    /// <param name="str">需要处理的字符串</param>
    /// <param name="middlePercent">隐藏百分比的整数值</param>
    /// <returns>中间*号替代的字符串</returns>
    public static string HideMiddle(this string str, int middlePercent = 50)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        if (middlePercent < 0)
            middlePercent = 0;
        if (middlePercent > 100)
            middlePercent = 100;

        int middleLength = (int)(Math.Floor(str.Length * 1.0 * middlePercent / 100));
        int firstLength = (str.Length - middleLength) / 2;
        string midStr = string.Empty;
        for (int i = 0; i < middleLength; i++)
            midStr += "*";
        return string.Concat(str.AsSpan(0, firstLength), midStr, str.AsSpan(firstLength + middleLength));
    }
    #endregion

    #region Exception
    /// <summary>
    /// Throws an System.ArgumentNullException if the string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string</param>
    /// <exception cref="ArgumentNullException">the string is null, empty, or consists only of white-space characters</exception>
    public static void ThrowIfNullOrWhiteSpace(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
    }
    #endregion

    #region Decimal
    /// <summary>
    /// 四舍五入（默认保留2位小数）
    /// </summary>
    /// <param name="value">值</param>
    /// <returns>返回值</returns>
    public static decimal ToRound(this decimal value)
    {
        return value.ToRound(2);
    }

    /// <summary>
    /// 四舍五入（指定保留小数位数）
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="decimals">小数位数</param>
    /// <returns>返回值</returns>
    public static decimal ToRound(this decimal value, int decimals)
    {
        return value.ToRound(decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 四舍五入（指定保留小数位数和策略）
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="decimals">小数位数</param>
    /// <param name="mode">策略</param>
    /// <returns>返回值</returns>
    public static decimal ToRound(this decimal value, int decimals, MidpointRounding mode)
    {
        return Math.Round(value, decimals, mode);
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