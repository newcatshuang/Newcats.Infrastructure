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

namespace Newcats.Utils.Helpers
{
    public class RsaUtil
    {
        static private Regex _PEMCode = new Regex(@"--+.+?--+|\s+");
        static private byte[] _SeqOID = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        static private byte[] _Ver = new byte[] { 0x02, 0x01, 0x00 };

        public static RsaKey CreateRsaKey(RsaKeyFormatEnum keyFormat = RsaKeyFormatEnum.Pkcs8, int keySizeInBits = 2048)
        {
            if (keySizeInBits < 2048)
                throw new ArgumentException("Key size min value is 2048.");

            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySizeInBits;

                return new RsaKey { PublicKey = GetPublicKey(rsa), PrivateKey = GetPrivateKey(rsa, keyFormat), KeyFormat = keyFormat };
            }
        }

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

                return "-----BEGIN PUBLIC KEY-----\n" + TextBreak(Convert.ToBase64String(bytes), 64) + "\n-----END PUBLIC KEY-----";
            }
        }

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
                if (keyFormat != RsaKeyFormatEnum.Pkcs8)
                {
                    flag = " RSA" + flag;
                }

                return "-----BEGIN" + flag + "-----\n" + TextBreak(Convert.ToBase64String(bytes), 64) + "\n-----END" + flag + "-----";
            }
        }

        public static byte[] Encrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] Decrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
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
    }
}