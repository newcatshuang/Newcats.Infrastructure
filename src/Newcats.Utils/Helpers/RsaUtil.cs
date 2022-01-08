/***************************************************************************
 *GUID: b23b00c7-2d05-472b-b513-5b9897e133dd
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-02 18:11:35
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newcats.Utils.Models;
using System.IO;
using Newcats.Utils.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace Newcats.Utils.Helpers;

public class RsaUtil
{
    private static Regex _PEMCode = new Regex(@"--+.+?--+|\s+");
    private static byte[] _SeqOID = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
    private static byte[] _Ver = new byte[] { 0x02, 0x01, 0x00 };

    /// <summary>
    /// 默认Pkcs8格式4096位OpenSsl样式的Rsa公钥
    /// </summary>
    const string DefaultRsaPublicKey = @"-----BEGIN PUBLIC KEY-----
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
    const string DefaultRsaPrivateKey = @"-----BEGIN PRIVATE KEY-----
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
Q0Nyb6nNAir/bYg28PxRvEUalkSNNw==
-----END PRIVATE KEY-----";

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
        StringBuilder builder = new StringBuilder();
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
    /// <exception cref="OutOfMemoryException">当要加密的字符串的字节码大于可加密的最大长度时，发生异常</exception>
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
    /// <exception cref="OutOfMemoryException">当要加密的字节码大于可加密的最大长度时，发生异常</exception>
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
            if (data.Length > maxLength)
                throw new OutOfMemoryException($"The data to encrpty is out of max encrypt length {maxLength}");
            return rsa.Encrypt(data, padding);
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
            return rsa.Decrypt(data, padding);
        }
    }
    #endregion

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
}