/***************************************************************************
 *GUID: b23b00c7-2d05-472b-b513-5b9897e133dd
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-02 18:11:35
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newcats.Utils.Extensions;
using Newcats.Utils.Models;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// 加密操作
    /// 说明：
    /// 1.Rsa加密参考 http://tool.chacuo.net/cryptrsapubkey https://github.com/myloveCc/NETCore.Encrypt
    /// 2.AES加密参考 https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.descryptoserviceprovider?view=net-6.0
    /// 3.DES加密参考 https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.descryptoserviceprovider?view=net-6.0
    /// </summary>
    public static class EncryptHelper
    {
        #region RandomString/RandomNumber/RandomKey(Salt)
        /// <summary>
        /// 获取指定长度的随机数字/字母组成的字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        public static string GetRandomString(int length)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z'
          };

            StringBuilder num = new();

            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[Random.Shared.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }

        /// <summary>
        /// 获取指定长度的随机数字组成的字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        public static string GetRandomNumber(int length)
        {
            char[] arrChar = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder num = new();

            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[Random.Shared.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }

        /// <summary>
        /// 获取指定长度的随机数字/字母/特殊字符组成的字符串(Salt)
        /// </summary>
        /// <param name="length">字符串长度</param>
        public static string GetRandomKey(int length)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z',
           '!','@','#','$','%','^','&','*','(',')','_','+','=','|','.',',','/'
          };

            StringBuilder num = new();

            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[Random.Shared.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }
        #endregion

        #region Md5加密

        /// <summary>
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">值</param>
        public static string MD5By16(string value)
        {
            return MD5By16(value, Encoding.UTF8);
        }

        /// <summary>
        /// Md5加密，返回16位结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="encoding">字符编码</param>
        public static string MD5By16(string value, Encoding encoding)
        {
            return Md5(value, encoding, 4, 8);
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        private static string Md5(string value, Encoding encoding, int? startIndex, int? length)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var md5 = MD5.Create();// new MD5CryptoServiceProvider();
            string result;
            try
            {
                var hash = md5.ComputeHash(encoding.GetBytes(value));
                //result = startIndex == null ? BitConverter.ToString(hash) : BitConverter.ToString(hash, startIndex.SafeValue(), length.SafeValue());
                result = startIndex == null ? BitConverter.ToString(hash) : BitConverter.ToString(hash, startIndex ?? default(int), length ?? default(int));
            }
            finally
            {
                md5.Clear();
            }
            return result.Replace("-", "");
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">值</param>
        public static string MD5By32(string value)
        {
            return MD5By32(value, Encoding.UTF8);
        }

        /// <summary>
        /// Md5加密，返回32位结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="encoding">字符编码</param>
        public static string MD5By32(string value, Encoding encoding)
        {
            return Md5(value, encoding, null, null);
        }

        #endregion

        #region HMACMD5
        /// <summary>
        /// HMACMD5 hash
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string HMACMD5(string str, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACMD5 md5 = new(secrectKey))
            {
                byte[] bytes_md5_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_md5_out = md5.ComputeHash(bytes_md5_in);
                string str_md5_out = BitConverter.ToString(bytes_md5_out);
                str_md5_out = str_md5_out.Replace("-", "");
                return str_md5_out;
            }
        }

        /// <summary>
        /// HMACMD5 hash
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string HMACMD5(string str, string key, Encoding encoding)
        {
            byte[] secrectKey = encoding.GetBytes(key);
            using (HMACMD5 md5 = new(secrectKey))
            {
                byte[] bytes_md5_in = encoding.GetBytes(str);
                byte[] bytes_md5_out = md5.ComputeHash(bytes_md5_in);
                string str_md5_out = BitConverter.ToString(bytes_md5_out);
                str_md5_out = str_md5_out.Replace("-", "");
                return str_md5_out;
            }
        }
        #endregion

        #region SHA1
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns></returns>
        public static string Sha1(string str)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes_sha1_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
                string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
                str_sha1_out = str_sha1_out.Replace("-", "");
                return str_sha1_out;
            }
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string Sha1(string str, Encoding encoding)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes_sha1_in = encoding.GetBytes(str);
                byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
                string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
                str_sha1_out = str_sha1_out.Replace("-", "");
                return str_sha1_out;
            }
        }
        #endregion

        #region SHA256
        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns></returns>
        public static string Sha256(string str)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes_sha256_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_sha256_out = sha256.ComputeHash(bytes_sha256_in);
                string str_sha256_out = BitConverter.ToString(bytes_sha256_out);
                str_sha256_out = str_sha256_out.Replace("-", "");
                return str_sha256_out;
            }
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string Sha256(string str, Encoding encoding)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes_sha256_in = encoding.GetBytes(str);
                byte[] bytes_sha256_out = sha256.ComputeHash(bytes_sha256_in);
                string str_sha256_out = BitConverter.ToString(bytes_sha256_out);
                str_sha256_out = str_sha256_out.Replace("-", "");
                return str_sha256_out;
            }
        }
        #endregion

        #region SHA384
        /// <summary>
        /// SHA384加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns></returns>
        public static string Sha384(string str)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                byte[] bytes_sha384_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_sha384_out = sha384.ComputeHash(bytes_sha384_in);
                string str_sha384_out = BitConverter.ToString(bytes_sha384_out);
                str_sha384_out = str_sha384_out.Replace("-", "");
                return str_sha384_out;
            }
        }

        /// <summary>
        /// SHA384加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string Sha384(string str, Encoding encoding)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                byte[] bytes_sha384_in = encoding.GetBytes(str);
                byte[] bytes_sha384_out = sha384.ComputeHash(bytes_sha384_in);
                string str_sha384_out = BitConverter.ToString(bytes_sha384_out);
                str_sha384_out = str_sha384_out.Replace("-", "");
                return str_sha384_out;
            }
        }
        #endregion

        #region SHA512
        /// <summary>
        /// SHA512加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns></returns>
        public static string Sha512(string str)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes_sha512_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_sha512_out = sha512.ComputeHash(bytes_sha512_in);
                string str_sha512_out = BitConverter.ToString(bytes_sha512_out);
                str_sha512_out = str_sha512_out.Replace("-", "");
                return str_sha512_out;
            }
        }

        /// <summary>
        /// SHA512加密
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string Sha512(string str, Encoding encoding)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes_sha512_in = encoding.GetBytes(str);
                byte[] bytes_sha512_out = sha512.ComputeHash(bytes_sha512_in);
                string str_sha512_out = BitConverter.ToString(bytes_sha512_out);
                str_sha512_out = str_sha512_out.Replace("-", "");
                return str_sha512_out;
            }
        }
        #endregion

        #region HMACSHA1
        /// <summary>
        /// HMAC_SHA1
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string HMACSHA1(string str, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACSHA1 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }

        /// <summary>
        /// HMAC_SHA1
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string HMACSHA1(string str, string key, Encoding encoding)
        {
            byte[] secrectKey = encoding.GetBytes(key);
            using (HMACSHA1 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = encoding.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }
        #endregion

        #region HMACSHA256
        /// <summary>
        /// HMAC_SHA256 
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string HMACSHA256(string str, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACSHA256 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }

        /// <summary>
        /// HMAC_SHA256 
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string HMACSHA256(string str, string key, Encoding encoding)
        {
            byte[] secrectKey = encoding.GetBytes(key);
            using (HMACSHA256 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = encoding.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }
        #endregion

        #region HMACSHA384
        /// <summary>
        /// HMAC_SHA384
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string HMACSHA384(string str, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACSHA384 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);


                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }

        /// <summary>
        /// HMAC_SHA384
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string HMACSHA384(string str, string key, Encoding encoding)
        {
            byte[] secrectKey = encoding.GetBytes(key);
            using (HMACSHA384 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = encoding.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);


                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }
        #endregion

        #region HMACSHA512
        /// <summary>
        /// HMAC_SHA512
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <returns></returns>
        public static string HMACSHA512(string str, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACSHA512 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }

        /// <summary>
        /// HMAC_SHA512
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <param name="key">encrypte key</param>
        /// <param name="encoding">字符编码</param>
        /// <returns></returns>
        public static string HMACSHA512(string str, string key, Encoding encoding)
        {
            byte[] secrectKey = encoding.GetBytes(key);
            using (HMACSHA512 hmac = new(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = encoding.GetBytes(str);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = BitConverter.ToString(bytes_hamc_out);
                str_hamc_out = str_hamc_out.Replace("-", "");

                return str_hamc_out;
            }
        }
        #endregion

        #region DES加密
        /// <summary>
        /// 默认Des密钥,8位字符串
        /// </summary>
        public const string DefaultDesKey = "yxJe/E8_";

        /// <summary>
        /// 默认TripleDes密钥,24位字符串
        /// </summary>
        public const string DefaultTripleDesKey = "AU5f6ImsFb,3@6z57j%Y_g7&";

        /// <summary>
        /// 生成Des密钥
        /// </summary>
        public static string CreateDesKey() => GetRandomKey(8);

        /// <summary>
        /// 生成TripleDes密钥
        /// </summary>
        public static string CreateTripleDesKey() => GetRandomKey(24);

        /// <summary>
        /// DES加密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesEncrypt(object value, bool tripleDES = true)
        {
            return DesEncrypt(value, tripleDES ? DefaultTripleDesKey : DefaultDesKey, tripleDES);
        }

        /// <summary>
        /// DES加密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesEncrypt(object value, string key, bool tripleDES = true)
        {
            return DesEncrypt(value, key, Encoding.UTF8, tripleDES);
        }

        /// <summary>
        /// DES加密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
        /// <param name="encoding">编码</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesEncrypt(object value, string key, Encoding encoding, bool tripleDES = true)
        {
            string text = value == null ? string.Empty : value.ToString().Trim();
            if (ValidateDes(text, key, tripleDES) == false)
                return string.Empty;
            using (var transform = tripleDES ? CreateTripleDesProvider(key, encoding).CreateEncryptor() : CreateDesProvider(key, encoding).CreateEncryptor())
            {
                return GetEncryptResult(text, encoding, transform);
            }
        }

        /// <summary>
        /// 验证Des加密参数
        /// </summary>
        private static bool ValidateDes(string text, string key, bool tripleDES = true)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(key))
                return false;
            if (tripleDES && key.Length == 24)
                return true;
            if (!tripleDES && key.Length == 8)
                return true;
            return false;
        }

        /// <summary>
        /// 创建TripleDES加密服务提供程序
        /// </summary>
        private static TripleDES CreateTripleDesProvider(string key, Encoding encoding)
        {
            var des = TripleDES.Create();
            des.Key = encoding.GetBytes(key);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            return des;
        }

        /// <summary>
        /// 创建Des加密服务提供程序
        /// </summary>
        private static DES CreateDesProvider(string key, Encoding encoding)
        {
            var des = DES.Create();
            des.Key = encoding.GetBytes(key);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            return des;
        }

        /// <summary>
        /// 获取加密结果
        /// </summary>
        private static string GetEncryptResult(string value, Encoding encoding, ICryptoTransform transform)
        {
            var bytes = encoding.GetBytes(value);
            var result = transform.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// DES解密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesDecrypt(object value, bool tripleDES = true)
        {
            return DesDecrypt(value, tripleDES ? DefaultTripleDesKey : DefaultDesKey, tripleDES);
        }

        /// <summary>
        /// DES解密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesDecrypt(object value, string key, bool tripleDES = true)
        {
            return DesDecrypt(value, key, Encoding.UTF8, tripleDES);
        }

        /// <summary>
        /// DES解密(默认使用TripleDES算法)
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥(TripleDes密钥24位,Des密钥8位)</param>
        /// <param name="encoding">编码</param>
        /// <param name="tripleDES">是否使用TripleDES算法</param>
        public static string DesDecrypt(object value, string key, Encoding encoding, bool tripleDES = true)
        {
            string text = value == null ? string.Empty : value.ToString().Trim();
            if (!ValidateDes(text, key, tripleDES))
                return string.Empty;
            using (var transform = tripleDES ? CreateTripleDesProvider(key, encoding).CreateDecryptor() : CreateDesProvider(key, encoding).CreateDecryptor())
            {
                return GetDecryptResult(text, encoding, transform);
            }
        }

        /// <summary>
        /// 获取解密结果
        /// </summary>
        private static string GetDecryptResult(string value, Encoding encoding, ICryptoTransform transform)
        {
            var bytes = Convert.FromBase64String(value);
            var result = transform.TransformFinalBlock(bytes, 0, bytes.Length);
            return encoding.GetString(result);
        }
        #endregion

        #region AES加密
        /// <summary>
        /// 默认Aes密钥,32位字符串
        /// </summary>
        public const string DefaultAesKey = "T&t8C,(YaGyFSkB_fVE1,8(j0v#69At0";

        /// <summary>
        /// 默认Aes向量,16位字符串
        /// </summary>
        public const string DefaultAesIv = "wjm+E(qTg,t!mJ01";

        /// <summary>
        /// 生成Aes密钥
        /// </summary>
        /// <returns>32位Aes密钥</returns>
        public static string CreateAesKey() => GetRandomKey(32);

        /// <summary>
        /// 生成Aes向量
        /// </summary>
        /// <returns>16位Aes向量</returns>
        public static string CreateAesIv() => GetRandomKey(16);

        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        public static string AesEncrypt(string value)
        {
            return AesEncrypt(value, DefaultAesKey, DefaultAesIv);
        }

        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥,32位字符串</param>
        /// <param name="iv">向量,16位字符串</param>
        public static string AesEncrypt(string value, string key, string iv)
        {
            return AesEncrypt(value, key, iv, Encoding.UTF8);
        }

        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="value">待加密的值</param>
        /// <param name="key">密钥,32位字符串</param>
        /// <param name="iv">向量,16位字符串</param>
        /// <param name="encoding">编码</param>
        public static string AesEncrypt(string value, string key, string iv, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(key))
                return string.Empty;
            using (var transform = CreateAesProvider(key, iv, encoding).CreateEncryptor())
            {
                return GetEncryptResult(value, encoding, transform);
            }
        }

        /// <summary>
        /// 创建Aes加密提供程序
        /// </summary>
        private static Aes CreateAesProvider(string key, string iv, Encoding encoding, CipherMode cipherMode = CipherMode.CBC)
        {
            Aes aes = Aes.Create();
            aes.Key = encoding.GetBytes(key);
            aes.IV = encoding.GetBytes(iv);
            aes.Mode = cipherMode;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        public static string AesDecrypt(string value)
        {
            return AesDecrypt(value, DefaultAesKey, DefaultAesIv);
        }

        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥,32位字符串</param>
        /// <param name="iv">向量,16位字符串</param>
        public static string AesDecrypt(string value, string key, string iv)
        {
            return AesDecrypt(value, key, iv, Encoding.UTF8);
        }

        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="value">加密后的值</param>
        /// <param name="key">密钥,32位字符串</param>
        /// <param name="iv">向量,16位字符串</param>
        /// <param name="encoding">编码</param>
        public static string AesDecrypt(string value, string key, string iv, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(key))
                return string.Empty;
            using (var transform = CreateAesProvider(key, iv, encoding).CreateDecryptor())
            {
                return GetDecryptResult(value, encoding, transform);
            }
        }
        #endregion

        #region RSA生成密钥、转换密钥、加密、解密、签名、验签
        #region Rsa字段
        private static Regex _PEMCode = new(@"--+.+?--+|\s+");
        private static byte[] _SeqOID = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        private static byte[] _Ver = new byte[] { 0x02, 0x01, 0x00 };

        /// <summary>
        /// 默认Pkcs8格式4096位OpenSsl样式的Rsa公钥
        /// </summary>
        public const string DefaultRsaPublicKey = @"-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAv4SYt9K774CzpNjRCvAp
NcRDFwlX49vGICX3NCau3nni4VjqqVNE8bxa0gZ8rMsCC2FiiH1Sy6L7qguPtE9o
fMNWPCwupMzVyHnzR3/1jYkPCt6mOUDJdgCLAL/nDO2vGkDV48G57r1PBSNfOTmG
4IXFCMvmXOuU1VFlebUgHaVf/Wrz5fAxlbEEUP5q0kXqhDA16FWoiWCLj17hoX8q
peQiO8di5x+L0BBbpZWADyBVlbVZkFgYGQmv9bPXoAMwRx8exBez49uy/teOlOt6
XE/sqsT8HwY3HYYQ5Xv0dxT29YcFGnSc1TQR7XumhnXgYWT8DJcTgb+VHKOSmz3R
D/JpGj4e25o44a9hcicwHCwWXN8QHXGFypcM92kTYpALVH3VwJUvwS9Dq6Z7emnQ
CvXrGxnfnxiXX7B3jxecSi3N8Zs+s77XV8mjKgj297tb/sDKXfYG9FZpKH8iXYG8
KCqtgpWQLXiRp0XQhtTKbfCMtDqFFcO0FDtZimDc5ccMV8VFR0aRxNJW8f6MmNts
WtN1Tvqn1lgSgri+H49Q2lrmUZQLHgWzEKZnryCruCv5I9EGrZhEXx8XPqT8PmC5
x5gTLNcLr5Iru/hKChhVjRxHDoTTqYPGCAVtYQvztfyUIsN7L4NZRtr49PQO6HmI
HIS9G10WfFNaGg9pBDLS600CAwEAAQ==
-----END PUBLIC KEY-----";

        /// <summary>
        /// 默认Pkcs8格式4096位OpenSsl样式的Rsa私钥
        /// </summary>
        public const string DefaultRsaPrivateKey = @"-----BEGIN PRIVATE KEY-----
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
Q0Nyb6nNAir/bYg28PxRvEUalkSNNw==
-----END PRIVATE KEY-----";
        #endregion

        #region 处理Rsa密钥
        #region 暂时注释
        /*
        /// <summary>
        /// 随机生成一对Rsa密钥(此方法生成的密钥与其他语言的不一致，验证不通过 http://tool.chacuo.net/cryptrsakeyvalid)
        /// </summary>
        /// <param name="keyFormat">密钥格式</param>
        /// <param name="keySizeInBits">密钥大小(bit)</param>
        /// <param name="openSslStyle">是否格式化为OpenSsl样式(带BEGIN END)</param>
        /// <returns>Rsa密钥</returns>
        public static RsaKey GenerateRsaKey(RsaKeyFormatEnum keyFormat = RsaKeyFormatEnum.Pkcs8, int keySizeInBits = 4096, bool openSslStyle = true)
        {
            using (RSA rsa = RSAOpenSsl.Create()) //RSA.Create())
            {
                rsa.KeySize = keySizeInBits;
                string priKey = string.Empty;
                string pubKey = string.Empty;
                switch (keyFormat)
                {
                    case RsaKeyFormatEnum.Pkcs8:
                        pubKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                        priKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
                        break;
                    case RsaKeyFormatEnum.Pkcs1:
                        pubKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                        priKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                        break;
                    case RsaKeyFormatEnum.Xml:
                        pubKey = rsa.ToXmlString(false);
                        priKey = rsa.ToXmlString(true);
                        break;
                    default:
                        pubKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                        priKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
                        break;
                }
                if (openSslStyle)
                {
                    pubKey = RsaFormatToOpenSslStyle(pubKey, false, keyFormat);
                    priKey = RsaFormatToOpenSslStyle(priKey, true, keyFormat);
                }
                return new RsaKey { KeyFormat = keyFormat, PublicKey = pubKey, PrivateKey = priKey };
            }
        }
        */
        #endregion

        /// <summary>
        /// 随机生成一对Rsa密钥
        /// </summary>
        /// <param name="keyFormat">密钥格式</param>
        /// <param name="keySizeInBits">密钥大小(bit)</param>
        /// <param name="openSslStyle">是否格式化为OpenSsl样式(带BEGIN END)</param>
        /// <returns>Rsa密钥</returns>
        public static RsaKey CreateRsaKey(RsaKeyFormatEnum keyFormat = RsaKeyFormatEnum.Pkcs8, int keySizeInBits = 4096, bool openSslStyle = true)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySizeInBits;
                string priKey = string.Empty;
                string pubKey = string.Empty;

                if (keyFormat == RsaKeyFormatEnum.Xml)
                {
                    pubKey = rsa.ToXmlString(false);
                    priKey = rsa.ToXmlString(true);
                }
                else
                {
                    pubKey = GetPemPublicKey(rsa, keyFormat, openSslStyle);
                    priKey = GetPemPrivateKey(rsa, keyFormat, openSslStyle);
                }
                return new RsaKey { PublicKey = pubKey, PrivateKey = priKey, KeyFormat = keyFormat };
            }
        }

        /// <summary>
        /// 转换Rsa密钥
        /// </summary>
        /// <param name="key">Base64字符串的密钥(pem密钥必须包含BEGIN END字符串)</param>
        /// <param name="from">原密钥格式</param>
        /// <param name="to">目标密钥格式</param>
        /// <returns>Base64编码的密钥</returns>
        public static string ConvertRsaKey(string key, RsaKeyFormatEnum from, RsaKeyFormatEnum to)
        {
            key.ThrowIfNullOrWhiteSpace();
            if (from == to)
                return key;
            bool isPrivateKey = false;
            if (key.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) || key.Contains("<InverseQ>", StringComparison.OrdinalIgnoreCase))
                isPrivateKey = true;
            RSA rsa;
            if (from == RsaKeyFormatEnum.Xml)
            {
                using (rsa = CreateRsaFromXml(key))
                {
                    return isPrivateKey ? rsa.ToXmlString(true) : rsa.ToXmlString(false);
                }
            }
            else
            {
                using (rsa = CreateRsaFromPem(key))
                {
                    return isPrivateKey ? GetPemPrivateKey(rsa, to, true) : GetPemPublicKey(rsa, to, true);
                }
            }
        }

        /// <summary>
        /// 格式化pem密钥为OpenSsl样式(包含BEGIN END字符串且每64字符换行)
        /// </summary>
        /// <param name="base64Key">原密钥</param>
        /// <param name="privateKey">是否私钥(否则为公钥)</param>
        /// <param name="keyFormat">密钥格式</param>
        /// <returns>格式化之后的密钥</returns>
        public static string RsaFormatToOpenSslStyle(string base64Key, bool privateKey, RsaKeyFormatEnum keyFormat)
        {
            if (keyFormat == RsaKeyFormatEnum.Xml)
                return base64Key;

            if (privateKey)
            {
                string flag = " PRIVATE KEY";
                if (keyFormat == RsaKeyFormatEnum.Pkcs1)
                    flag = " RSA" + flag;
                base64Key = $"-----BEGIN{flag}-----\n{LineWrap(base64Key)}\n-----END{flag}-----";
            }
            else
            {
                base64Key = $"-----BEGIN PUBLIC KEY-----\n{LineWrap(base64Key)}\n-----END PUBLIC KEY-----";
            }
            return base64Key;
        }

        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="rsa">rsa实例</param>
        /// <param name="keyFormat">密钥格式</param>
        /// <param name="openSslStyle">是否格式化为OpenSsl样式(包含BEGIN END字符串且每64字符换行)</param>
        /// <returns>公钥Base64字符串</returns>
        private static string GetPemPublicKey(RSA rsa, RsaKeyFormatEnum keyFormat, bool openSslStyle = true)
        {
            using (var ms = new MemoryStream())
            {
                Action<int> writeLenByte = (len) =>
                {
                    if (len < 0x80)
                    {
                        ms.WriteByte((byte)len);
                    }
                    else if (len <= 0xff)
                    {
                        ms.WriteByte(0x81);
                        ms.WriteByte((byte)len);
                    }
                    else
                    {
                        ms.WriteByte(0x82);
                        ms.WriteByte((byte)(len >> 8 & 0xff));
                        ms.WriteByte((byte)(len & 0xff));
                    }
                };

                //write moudle data
                Action<byte[]> writeBlock = (byts) =>
                {
                    var addZero = (byts[0] >> 4) >= 0x8;
                    ms.WriteByte(0x02);
                    var len = byts.Length + (addZero ? 1 : 0);
                    writeLenByte(len);

                    if (addZero)
                    {
                        ms.WriteByte(0x00);
                    }
                    ms.Write(byts, 0, byts.Length);
                };

                Func<int, byte[], byte[]> writeLen = (index, byts) =>
                {
                    var len = byts.Length - index;

                    ms.SetLength(0);
                    ms.Write(byts, 0, index);
                    writeLenByte(len);
                    ms.Write(byts, index, len);

                    return ms.ToArray();
                };

                /****Create public key****/
                var param = rsa.ExportParameters(false);

                ms.WriteByte(0x30);
                var index1 = (int)ms.Length;

                // Encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
                ms.Write(_SeqOID, 0, _SeqOID.Length);

                //Start with 0x00 
                ms.WriteByte(0x03);
                var index2 = (int)ms.Length;
                ms.WriteByte(0x00);

                //Content length
                ms.WriteByte(0x30);
                var index3 = (int)ms.Length;

                //Write Modulus
                writeBlock(param.Modulus);

                //Write Exponent
                writeBlock(param.Exponent);

                var bytes = ms.ToArray();

                bytes = writeLen(index3, bytes);
                bytes = writeLen(index2, bytes);
                bytes = writeLen(index1, bytes);

                if (openSslStyle)
                    return RsaFormatToOpenSslStyle(Convert.ToBase64String(bytes), false, keyFormat);
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// 获取私钥
        /// </summary>
        /// <param name="rsa">rsa实例</param>
        /// <param name="keyFormat">密钥格式</param>
        /// <param name="openSslStyle">是否格式化为OpenSsl样式(包含BEGIN END字符串且每64字符换行)</param>
        /// <returns>私钥Base64字符串</returns>
        private static string GetPemPrivateKey(RSA rsa, RsaKeyFormatEnum keyFormat, bool openSslStyle = true)
        {
            using (var ms = new MemoryStream())
            {
                Action<int> writeLenByte = (len) =>
                {
                    if (len < 0x80)
                    {
                        ms.WriteByte((byte)len);
                    }
                    else if (len <= 0xff)
                    {
                        ms.WriteByte(0x81);
                        ms.WriteByte((byte)len);
                    }
                    else
                    {
                        ms.WriteByte(0x82);
                        ms.WriteByte((byte)(len >> 8 & 0xff));
                        ms.WriteByte((byte)(len & 0xff));
                    }
                };

                //write moudle data
                Action<byte[]> writeBlock = (byts) =>
                {
                    var addZero = (byts[0] >> 4) >= 0x8;
                    ms.WriteByte(0x02);
                    var len = byts.Length + (addZero ? 1 : 0);
                    writeLenByte(len);

                    if (addZero)
                    {
                        ms.WriteByte(0x00);
                    }
                    ms.Write(byts, 0, byts.Length);
                };

                Func<int, byte[], byte[]> writeLen = (index, byts) =>
                {
                    var len = byts.Length - index;

                    ms.SetLength(0);
                    ms.Write(byts, 0, index);
                    writeLenByte(len);
                    ms.Write(byts, index, len);

                    return ms.ToArray();
                };

                /****Create private key****/
                var param = rsa.ExportParameters(true);

                //Write total length
                ms.WriteByte(0x30);
                int index1 = (int)ms.Length;

                //Write version
                ms.Write(_Ver, 0, _Ver.Length);

                //PKCS8 
                int index2 = -1, index3 = -1;
                if (keyFormat == RsaKeyFormatEnum.Pkcs8)
                {
                    ms.Write(_SeqOID, 0, _SeqOID.Length);

                    ms.WriteByte(0x04);
                    index2 = (int)ms.Length;

                    ms.WriteByte(0x30);
                    index3 = (int)ms.Length;

                    ms.Write(_Ver, 0, _Ver.Length);
                }

                //Write data
                writeBlock(param.Modulus);
                writeBlock(param.Exponent);
                writeBlock(param.D);
                writeBlock(param.P);
                writeBlock(param.Q);
                writeBlock(param.DP);
                writeBlock(param.DQ);
                writeBlock(param.InverseQ);

                var bytes = ms.ToArray();

                if (index2 != -1)
                {
                    bytes = writeLen(index3, bytes);
                    bytes = writeLen(index2, bytes);
                }
                bytes = writeLen(index1, bytes);

                if (openSslStyle)
                    return RsaFormatToOpenSslStyle(Convert.ToBase64String(bytes), true, keyFormat);
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// 通过pem格式的公钥/私钥创建Rsa实例
        /// </summary>
        /// <param name="pem">Base64字符串的pem格式的公钥/私钥(密钥需包含BEGIN END字符串)</param>
        /// <returns>Rsa实例</returns>
        private static RSA CreateRsaFromPem(string pem)
        {
            RSA rsa = RSA.Create();
            var param = new RSAParameters();

            var base64 = _PEMCode.Replace(pem, "");
            var data = Convert.FromBase64String(base64);
            if (data == null)
            {
                throw new Exception("Pem content invalid ");
            }
            var idx = 0;

            //read  length
            Func<byte, int> readLen = (first) =>
            {
                if (data[idx] == first)
                {
                    idx++;
                    if (data[idx] == 0x81)
                    {
                        idx++;
                        return data[idx++];
                    }
                    else if (data[idx] == 0x82)
                    {
                        idx++;
                        return (((int)data[idx++]) << 8) + data[idx++];
                    }
                    else if (data[idx] < 0x80)
                    {
                        return data[idx++];
                    }
                }
                throw new Exception("Not found any content in pem file");
            };
            //read module length
            Func<byte[]> readBlock = () =>
            {
                var len = readLen(0x02);
                if (data[idx] == 0x00)
                {
                    idx++;
                    len--;
                }
                var val = data.Skip(idx).Take(len).ToArray();//data.Sub(idx, len);
                idx += len;
                return val;
            };

            Func<byte[], bool> eq = (byts) =>
            {
                for (var i = 0; i < byts.Length; i++, idx++)
                {
                    if (idx >= data.Length)
                    {
                        return false;
                    }
                    if (byts[i] != data[idx])
                    {
                        return false;
                    }
                }
                return true;
            };

            if (pem.Contains("PUBLIC", StringComparison.OrdinalIgnoreCase))
            {
                /****Use public key****/
                readLen(0x30);
                if (!eq(_SeqOID))
                {
                    throw new Exception("Unknown pem format");
                }

                readLen(0x03);
                idx++;
                readLen(0x30);

                //Modulus
                param.Modulus = readBlock();

                //Exponent
                param.Exponent = readBlock();
            }
            else if (pem.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase))
            {
                /****Use private key****/
                readLen(0x30);

                //Read version
                if (!eq(_Ver))
                {
                    throw new Exception("Unknown pem version");
                }

                //Check PKCS8
                var idx2 = idx;
                if (eq(_SeqOID))
                {
                    //Read one byte
                    readLen(0x04);

                    readLen(0x30);

                    //Read version
                    if (!eq(_Ver))
                    {
                        throw new Exception("Pem version invalid");
                    }
                }
                else
                {
                    idx = idx2;
                }

                //Reda data
                param.Modulus = readBlock();
                param.Exponent = readBlock();
                param.D = readBlock();
                param.P = readBlock();
                param.Q = readBlock();
                param.DP = readBlock();
                param.DQ = readBlock();
                param.InverseQ = readBlock();
            }
            else
            {
                throw new Exception("pem need 'BEGIN' and  'END'");
            }

            rsa.ImportParameters(param);
            return rsa;
        }

        /// <summary>
        /// 通过xml格式的公钥/私钥创建Rsa实例
        /// </summary>
        /// <param name="xml">Base64字符串的公钥/私钥</param>
        /// <returns>Rsa实例</returns>
        private static RSA CreateRsaFromXml(string xml)
        {
            RSA rsa = RSA.Create();
            rsa.FromXmlString(xml);
            return rsa;
        }

        /// <summary>
        /// 字符串换行(使用 \n 换行符)
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <param name="lineSize">每行大小</param>
        /// <returns>换行之后的字符串</returns>
        private static string LineWrap(string text, int lineSize = 64)
        {
            int index = 0;
            int strLength = text.Length;
            StringBuilder builder = new();
            while (index < strLength)
            {
                if (index > 0)
                {
                    builder.Append('\n');
                }
                if (index + lineSize >= strLength)
                {
                    builder.Append(text.Substring(index));
                }
                else
                {
                    builder.Append(text.Substring(index, lineSize));
                }
                index += lineSize;
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取不同填充模式Rsa加密的最大长度
        /// </summary>
        /// <param name="keySizeInBits">密钥大小(bit)</param>
        /// <param name="padding">填充模式</param>
        /// <returns>最大加密长度</returns>
        private static int GetMaxRsaEncryptLength(int keySizeInBits, RSAEncryptionPadding padding)
        {
            var offset = 0;
            if (padding.Mode == RSAEncryptionPaddingMode.Pkcs1)
            {
                offset = 11;
            }
            else
            {
                if (padding.Equals(RSAEncryptionPadding.OaepSHA1))
                {
                    offset = 42;
                }

                if (padding.Equals(RSAEncryptionPadding.OaepSHA256))
                {
                    offset = 66;
                }

                if (padding.Equals(RSAEncryptionPadding.OaepSHA384))
                {
                    offset = 98;
                }

                if (padding.Equals(RSAEncryptionPadding.OaepSHA512))
                {
                    offset = 130;
                }
            }
            var maxLength = keySizeInBits / 8 - offset;
            return maxLength;
        }
        #endregion

        #region Rsa加密
        /// <summary>
        /// 使用Rsa加密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充,默认公钥
        /// </summary>
        /// <param name="data">要加密的字符串</param>
        /// <returns>Base64编码的加密字符串</returns>
        public static string RsaEncrypt(string data)
        {
            return RsaEncrypt(data, DefaultRsaPublicKey);
        }

        /// <summary>
        /// 使用Rsa加密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
        /// </summary>
        /// <param name="data">要加密的字符串</param>
        /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
        /// <returns>Base64编码的加密字符串</returns>
        public static string RsaEncrypt(string data, string publicKey)
        {
            return RsaEncrypt(data, publicKey, RSAEncryptionPadding.Pkcs1, Encoding.UTF8);
        }

        /// <summary>
        /// 使用Rsa加密字符串
        /// </summary>
        /// <param name="data">要加密的字符串</param>
        /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
        /// <param name="padding">填充方式</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>Base64编码的加密字符串</returns>
        public static string RsaEncrypt(string data, string publicKey, RSAEncryptionPadding padding, Encoding encoding)
        {
            return Convert.ToBase64String(RsaEncrypt(encoding.GetBytes(data), publicKey, padding));
        }

        /// <summary>
        /// 使用Rsa加密数据
        /// </summary>
        /// <param name="data">要加密的数据字节码</param>
        /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
        /// <param name="padding">填充方式</param>
        /// <returns>加密之后的字节码</returns>
        public static byte[] RsaEncrypt(byte[] data, string publicKey, RSAEncryptionPadding padding)
        {
            data.ThrowIfNullOrEmpty();
            publicKey.ThrowIfNullOrWhiteSpace();
            padding.ThrowIfNull();

            bool isPem = false;
            if (publicKey.Contains("BEGIN", StringComparison.OrdinalIgnoreCase) && publicKey.Contains("END", StringComparison.OrdinalIgnoreCase) && !publicKey.Contains("<RSAKeyValue>", StringComparison.OrdinalIgnoreCase))
                isPem = true;
            using (RSA rsa = isPem ? CreateRsaFromPem(publicKey) : CreateRsaFromXml(publicKey))
            {
                int maxLength = GetMaxRsaEncryptLength(rsa.KeySize, padding);

                byte[] buffer = new byte[maxLength];//每片大小
                using (MemoryStream input = new(data), output = new())
                {
                    while (true)
                    {
                        int readSize = input.Read(buffer, 0, maxLength);//读取到buffer
                        if (readSize <= 0)//实际读取的数量
                            break;
                        byte[] temp = new byte[readSize];//实际的分片
                        Array.Copy(buffer, 0, temp, 0, readSize);//把buffer复制到temp
                        byte[] res = rsa.Encrypt(temp, padding);//加密temp
                        output.Write(res, 0, res.Length);//把当前片段的写入输出流
                    }
                    return output.ToArray();
                }
            }
        }
        #endregion

        #region Rsa解密
        /// <summary>
        /// 使用Rsa解密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充.默认私钥
        /// </summary>
        /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
        /// <returns>解密之后的UTF8编码字符串</returns>
        public static string RsaDecrypt(string data)
        {
            return RsaDecrypt(data, DefaultRsaPrivateKey, RSAEncryptionPadding.Pkcs1, Encoding.UTF8);
        }

        /// <summary>
        /// 使用Rsa解密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
        /// </summary>
        /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
        /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <returns>解密之后的UTF8编码字符串</returns>
        public static string RsaDecrypt(string data, string privateKey)
        {
            return RsaDecrypt(data, privateKey, RSAEncryptionPadding.Pkcs1, Encoding.UTF8);
        }

        /// <summary>
        /// 使用Rsa解密字符串
        /// </summary>
        /// <param name="data">要解密的字符串(密文)(Base64编码)</param>
        /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <param name="padding">填充方式</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>解密之后的指定编码字符串</returns>
        public static string RsaDecrypt(string data, string privateKey, RSAEncryptionPadding padding, Encoding encoding)
        {
            return encoding.GetString(RsaDecrypt(Convert.FromBase64String(data), privateKey, padding));
        }

        /// <summary>
        /// 使用Rsa解密数据
        /// </summary>
        /// <param name="data">要解密的数据(密文)</param>
        /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <param name="padding">填充方式</param>
        /// <returns>解密之后的字节码</returns>
        public static byte[] RsaDecrypt(byte[] data, string privateKey, RSAEncryptionPadding padding)
        {
            data.ThrowIfNullOrEmpty();
            privateKey.ThrowIfNullOrWhiteSpace();
            padding.ThrowIfNull();

            bool isPem = false;
            if (privateKey.Contains("BEGIN", StringComparison.OrdinalIgnoreCase) && privateKey.Contains("END", StringComparison.OrdinalIgnoreCase) && !privateKey.Contains("<RSAKeyValue>", StringComparison.OrdinalIgnoreCase))
                isPem = true;
            using (RSA rsa = isPem ? CreateRsaFromPem(privateKey) : CreateRsaFromXml(privateKey))
            {
                int maxLength = rsa.KeySize / 8;

                byte[] buffer = new byte[maxLength];//每片大小
                using (MemoryStream input = new(data), output = new())
                {
                    while (true)
                    {
                        int readSize = input.Read(buffer, 0, maxLength);//读取到buffer
                        if (readSize <= 0)//实际读取的数量
                            break;
                        byte[] temp = new byte[readSize];//实际的分片
                        Array.Copy(buffer, 0, temp, 0, readSize);//把buffer复制到temp
                        byte[] res = rsa.Decrypt(temp, padding);//解密temp
                        output.Write(res, 0, res.Length);//把当前片段的写入输出流
                    }
                    return output.ToArray();
                }
            }
        }
        #endregion

        #region Rsa签名
        /// <summary>
        /// 使用Rsa对数据签名,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据,默认私钥
        /// </summary>
        /// <param name="data">要加签的数据</param>
        /// <returns>签名之后的Base64编码字符串</returns>
        public static string RsaSignData(string data)
        {
            return RsaSignData(data, DefaultRsaPrivateKey);
        }

        /// <summary>
        /// 使用Rsa对数据签名,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据
        /// </summary>
        /// <param name="data">要加签的数据</param>
        /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <returns>签名之后的Base64编码字符串</returns>
        public static string RsaSignData(string data, string privateKey)
        {
            return RsaSignData(data, privateKey, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1, Encoding.UTF8);
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
        public static string RsaSignData(string data, string privateKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding, Encoding encoding)
        {
            return Convert.ToBase64String(RsaSignData(encoding.GetBytes(data), privateKey, hashAlgorithm, padding));
        }

        /// <summary>
        /// 使用Rsa对数据签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <param name="hashAlgorithm">指定Hash算法</param>
        /// <param name="padding">填充方式</param>
        /// <returns>签名之后的字节码</returns>
        public static byte[] RsaSignData(byte[] data, string privateKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            data.ThrowIfNullOrEmpty();
            privateKey.ThrowIfNullOrWhiteSpace();
            padding.ThrowIfNull();

            bool isPem = false;
            if (privateKey.Contains("BEGIN", StringComparison.OrdinalIgnoreCase) && privateKey.Contains("END", StringComparison.OrdinalIgnoreCase) && !privateKey.Contains("<RSAKeyValue>", StringComparison.OrdinalIgnoreCase))
                isPem = true;
            using (RSA rsa = isPem ? CreateRsaFromPem(privateKey) : CreateRsaFromXml(privateKey))
            {
                return rsa.SignData(data, hashAlgorithm, padding);
            }
        }
        #endregion

        #region Rsa验签
        /// <summary>
        /// Rsa验签,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据,默认公钥
        /// </summary>
        /// <param name="data">要验证的数据</param>
        /// <param name="signature">Base64编码签名</param>
        /// <returns>数据签名是否一致</returns>
        public static bool RsaVerifyData(string data, string signature)
        {
            return RsaVerifyData(data, signature, DefaultRsaPublicKey);
        }

        /// <summary>
        /// Rsa验签,默认使用Sha1算法签名,RSASignaturePadding.Pkcs1填充,UTF8编码数据
        /// </summary>
        /// <param name="data">要验证的数据</param>
        /// <param name="signature">Base64编码签名</param>
        /// <param name="publicKey">Rsa公钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <returns>数据签名是否一致</returns>
        public static bool RsaVerifyData(string data, string signature, string publicKey)
        {
            return RsaVerifyData(data, signature, publicKey, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1, Encoding.UTF8);
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
        public static bool RsaVerifyData(string data, string signature, string publicKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding, Encoding encoding)
        {
            return RsaVerifyData(encoding.GetBytes(data), Convert.FromBase64String(signature), publicKey, hashAlgorithm, padding);
        }

        /// <summary>
        /// Rsa验签
        /// </summary>
        /// <param name="data">要验证的数据</param>
        /// <param name="signature">签名</param>
        /// <param name="publicKey">Rsa公钥(pem私钥必须包含BEGIN END字符串)</param>
        /// <param name="hashAlgorithm">指定Hash算法</param>
        /// <param name="padding">填充方式</param>
        /// <returns>数据签名是否一致</returns>
        public static bool RsaVerifyData(byte[] data, byte[] signature, string publicKey, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            data.ThrowIfNullOrEmpty();
            signature.ThrowIfNullOrEmpty();
            publicKey.ThrowIfNullOrWhiteSpace();
            padding.ThrowIfNull();

            bool isPem = false;
            if (publicKey.Contains("BEGIN", StringComparison.OrdinalIgnoreCase) && publicKey.Contains("END", StringComparison.OrdinalIgnoreCase) && !publicKey.Contains("<RSAKeyValue>", StringComparison.OrdinalIgnoreCase))
                isPem = true;
            using (RSA rsa = isPem ? CreateRsaFromPem(publicKey) : CreateRsaFromXml(publicKey))
            {
                return rsa.VerifyData(data, signature, hashAlgorithm, padding);
            }
        }
        #endregion 
        #endregion
    }
}