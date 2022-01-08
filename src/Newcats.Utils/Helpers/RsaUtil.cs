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
            var val = data.Skip(idx + 1).Take(len).ToArray();//data.Sub(idx, len);
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

    /// <summary>
    /// 使用Rsa加密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要加密的字符串</param>
    /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
    /// <returns>Base64编码的加密字符串</returns>
    public static string RsaEncrypt(string data, string publicKey)
    {
        return RsaEncrypt(data, publicKey, Encoding.UTF8, RSAEncryptionPadding.Pkcs1);
    }

    /// <summary>
    /// 使用Rsa加密字符串
    /// </summary>
    /// <param name="data">要加密的字符串</param>
    /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
    /// <param name="encoding">编码方式</param>
    /// <param name="padding">填充方式</param>
    /// <returns>Base64编码的加密字符串</returns>
    /// <exception cref="OutOfMemoryException">当要加密的字符串的字节码大于可加密的最大长度时，发生异常</exception>
    public static string RsaEncrypt(string data, string publicKey, Encoding encoding, RSAEncryptionPadding padding)
    {
        return Convert.ToBase64String(RsaEncrypt(encoding.GetBytes(data), publicKey, padding));
    }

    /// <summary>
    /// 使用Rsa加密数据,默认RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要加密的数据字节码</param>
    /// <param name="publicKey">Rsa公钥(pem公钥必须包含BEGIN END字符串)</param>
    /// <returns>加密之后的字节码</returns>
    public static byte[] RsaEncrypt(byte[] data, string publicKey)
    {
        return RsaEncrypt(data, publicKey, RSAEncryptionPadding.Pkcs1);
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
            int maxLength = GetMaxRsaEncryptLength(rsa, padding);
            if (data.Length > maxLength)
                throw new OutOfMemoryException($"The data to encrpty is out of max encrypt length {maxLength}");
            return rsa.Encrypt(data, padding);
        }
    }

    /// <summary>
    /// 使用Rsa解密字符串,默认UTF8编码,RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要解密的字符串(密文)</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <returns>解密之后的Base64字符串</returns>
    public static string RsaDecrypt(string data, string privateKey)
    {
        return RsaDecrypt(data, privateKey, RSAEncryptionPadding.Pkcs1, Encoding.UTF8);
    }

    /// <summary>
    /// 使用Rsa解密字符串
    /// </summary>
    /// <param name="data">要解密的字符串(密文)</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <param name="padding">填充方式</param>
    /// <param name="encoding">编码方式</param>
    /// <returns>解密之后的Base64字符串</returns>
    public static string RsaDecrypt(string data, string privateKey, RSAEncryptionPadding padding, Encoding encoding)
    {
        return Convert.ToBase64String(RsaDecrypt(encoding.GetBytes(data), privateKey, padding));
    }

    /// <summary>
    /// 使用Rsa解密数据,默认RSAEncryptionPadding.Pkcs1填充
    /// </summary>
    /// <param name="data">要解密的数据(密文)</param>
    /// <param name="privateKey">Rsa私钥(pem私钥必须包含BEGIN END字符串)</param>
    /// <returns>解密之后的字节码</returns>
    public static byte[] RsaDecrypt(byte[] data, string privateKey)
    {
        return RsaDecrypt(data, privateKey, RSAEncryptionPadding.Pkcs1);
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

    /// <summary>
    /// 获取不同填充模式Rsa加密的最大长度
    /// </summary>
    /// <param name="rsa">Rsa实例</param>
    /// <param name="padding">填充模式</param>
    /// <returns>最大加密长度</returns>
    private static int GetMaxRsaEncryptLength(RSA rsa, RSAEncryptionPadding padding)
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
        var keySize = rsa.KeySize;
        var maxLength = keySize / 8 - offset;
        return maxLength;
    }
}