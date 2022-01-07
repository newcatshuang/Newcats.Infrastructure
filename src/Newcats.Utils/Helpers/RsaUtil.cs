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

namespace Newcats.Utils.Helpers;

public class RsaUtil
{
    private static Regex _PEMCode = new Regex(@"--+.+?--+|\s+");
    private static byte[] _SeqOID = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
    private static byte[] _Ver = new byte[] { 0x02, 0x01, 0x00 };

    #region 处理密钥
    /// <summary>
    /// 随机生成一对Rsa密钥
    /// </summary>
    /// <param name="keyFormat">密钥格式</param>
    /// <param name="keySizeInBits">密钥大小(bit)</param>
    /// <returns>Rsa密钥</returns>
    public static RsaKey CreateRsaKey(RsaKeyFormatEnum keyFormat = RsaKeyFormatEnum.Pkcs8, int keySizeInBits = 4096)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.KeySize = keySizeInBits;

            if (keyFormat == RsaKeyFormatEnum.Xml)
            {
                return new RsaKey { PublicKey = rsa.ToXmlString(false), PrivateKey = rsa.ToXmlString(true), KeyFormat = keyFormat };
            }

            return new RsaKey { PublicKey = GetPublicKey(rsa), PrivateKey = GetPrivateKey(rsa, keyFormat), KeyFormat = keyFormat };
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
                return isPrivateKey ? GetPrivateKey(rsa, to) : GetPublicKey(rsa);
            }
        }
    }

    /// <summary>
    /// 获取公钥
    /// </summary>
    /// <param name="rsa">rsa实例</param>
    /// <returns>公钥Base64字符串</returns>
    private static string GetPublicKey(RSA rsa)
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

            return $"-----BEGIN PUBLIC KEY-----\n{TextBreak(Convert.ToBase64String(bytes), 64)}\n-----END PUBLIC KEY-----";
        }
    }

    /// <summary>
    /// 获取私钥
    /// </summary>
    /// <param name="rsa">rsa实例</param>
    /// <param name="keyFormat">密钥格式</param>
    /// <returns>私钥Base64字符串</returns>
    private static string GetPrivateKey(RSA rsa, RsaKeyFormatEnum keyFormat)
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


            var flag = " PRIVATE KEY";
            if (keyFormat == RsaKeyFormatEnum.Pkcs1)
            {
                flag = " RSA" + flag;
            }

            return $"-----BEGIN{flag}-----\n{TextBreak(Convert.ToBase64String(bytes), 64)}\n-----END{flag}-----";
        }
    }

    /// <summary>
    /// 通过pem格式的公钥/私钥创建Rsa实例
    /// </summary>
    /// <param name="pem">Base64字符串的pem格式的公钥/私钥(密钥需包含BEGIN END字符串)</param>
    /// <returns>Rsa实例</returns>
    private static RSA CreateRsaFromPem(string pem)
    {
        using (var rsa = RSA.Create())
        {
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
    }

    /// <summary>
    /// 通过xml格式的公钥/私钥创建Rsa实例
    /// </summary>
    /// <param name="xml">Base64字符串的公钥/私钥</param>
    /// <returns>Rsa实例</returns>
    private static RSA CreateRsaFromXml(string xml)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.FromXmlString(xml);
            return rsa;
        }
    }

    /// <summary>
    /// Text break method
    /// </summary>
    private static string TextBreak(string text, int line)
    {
        var idx = 0;
        var len = text.Length;
        var str = new StringBuilder();
        while (idx < len)
        {
            if (idx > 0)
            {
                str.Append('\n');
            }
            if (idx + line >= len)
            {
                str.Append(text.Substring(idx));
            }
            else
            {
                str.Append(text.Substring(idx, line));
            }
            idx += line;
        }
        return str.ToString();
    }
    #endregion
}