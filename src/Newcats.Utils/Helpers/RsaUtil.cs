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
using System.Threading.Tasks;
using Newcats.Utils.Models;

namespace Newcats.Utils.Helpers
{
    public class RsaUtil
    {
        public static RsaKey CreateRsaKey(int keySizeInBits = 2048)
        {
            RSA rsa = RSA.Create();
            rsa.KeySize = 2048;
            var pri = rsa.ExportRSAPrivateKey();
            var pk8 = Convert.ToBase64String(pri);

            var pub = rsa.ExportRSAPublicKey();

            var pk1 = Convert.ToBase64String(pub);

            return new RsaKey { PublicKey = TextBreak(pk1, 64), PrivateKey = TextBreak(pk8, 64) };
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