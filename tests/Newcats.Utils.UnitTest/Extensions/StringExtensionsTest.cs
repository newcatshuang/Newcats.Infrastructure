/***************************************************************************
 *GUID: 935aa6e4-f476-45fa-a67b-08a1573395bd
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-15 18:29:25
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newcats.Utils.Extensions;
using Newcats.Utils.Helpers;

namespace Newcats.Utils.UnitTest.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void TestThrowIfNull()
        {
            UserInfo user = null;
            ArgumentNullException? result = Assert.ThrowsException<ArgumentNullException>(() => user.ThrowIfNull());
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ArgumentNullException), result.GetType());
        }

        [TestMethod]
        public void TestMD5By16()
        {
            string result = "Newcats".MD5By16();
            Assert.IsNotNull(result);
            Assert.AreEqual("4BCF9A600D048E61", result);
        }

        [TestMethod]
        public void TestMD5By32()
        {
            string result = "Newcats".MD5By32();
            Assert.IsNotNull(result);
            Assert.AreEqual("A659F0864BCF9A600D048E6158482459", result);
        }

        [TestMethod]
        public void TestSha1()
        {
            string result = "Newcats".Sha1();
            Assert.IsNotNull(result);
            Assert.AreEqual("F94EB89F28111FBCA123CE6318ED795AA59A244E", result);
        }

        [TestMethod]
        public void TestSha256()
        {
            string result = "Newcats".Sha256();
            Assert.IsNotNull(result);
            Assert.AreEqual("CD787FAEBCA9E9E1FFA1F688F9CF5F221E0F703022807CBFF1FA8AA685A7C678", result);
        }

        [TestMethod]
        public void TestDesEncrypt()
        {
            const string input = "Newcats";
            string result1 = input.DesEncrypt(false);
            Assert.IsNotNull(result1);
            Assert.AreEqual("Ea2x5guynvE=", result1);

            string result2 = result1.DesDecrypt(false);
            Assert.IsNotNull(result2);
            Assert.AreEqual("Newcats", result2);
            Assert.AreEqual(result2, input);

            string result3 = input.DesEncrypt(true);
            Assert.IsNotNull(result3);
            Assert.AreEqual("YTM3cMMHHT0=", result3);

            string result4 = result3.DesDecrypt(true);
            Assert.IsNotNull(result4);
            Assert.AreEqual("Newcats", result4);
            Assert.AreEqual(input, result4);
        }

        [TestMethod]
        public void TestAesEncrypt()
        {
            const string input = "Newcats";
            string result1 = input.AesEncrypt();
            Assert.IsNotNull(result1);
            Assert.AreEqual("ZqUCIPDI4auTqY8fMdFZag==", result1);

            string result2 = result1.AesDecrypt();
            Assert.IsNotNull(result2);
            Assert.AreEqual("Newcats", result2);
            Assert.AreEqual(input, result2);
        }

        [TestMethod]
        public void TestRsaEncrypt()
        {
            const string input = "Newcats";
            string result1 = input.RsaEncrypt();
            Assert.IsNotNull(result1);

            string result2 = result1.RsaDecrypt();
            Assert.IsNotNull(result2);
            Assert.AreEqual(result2, input);

            string result3 = input.RsaSignData();
            Assert.IsNotNull(result3);
            Assert.AreEqual("oSTh3fxX8p3+gy8uKI9BU0VkYixFk5+6TAqqIyVxLScScXw3omjSns7jROSS9gsY8IUx/gUVngchl5D2ioPIJrLV/Q6b/gagEGVBZ1C5GDM7fLORAz8QNM9jheMBUXoyo720xA6mnKotPW3nTBXknSOjEZvHCftJ5ednNU1k2NmTxzbxLenH8/l/ecxpkDAo+nilMBnbKbiaP6MTK1fZdFGss4d9qTwurql5Fncd89mp0L1zW+0c+3FqL31n/8t6xWyrzwo0OmiBhBldE2obyn3Au/0h+CS+qdGdtYRwV0wJkcQd0iAguaUvASPyGIN7nKVS0HjlfVu8V2+kIGwLV+lYTQZT8xb/0PrBHt7TNVNDPcRCwF6gwfjshtgOTL5F7cbwh2/ruzvvHoAxM9Vbr/VH8tpe5fNy3a9mAIWAg0brypqkLvFHSUioTtJsg9TQcYjonR3erUZBWJMeQ6e5UeALmNRE3rZ3qrtJ2wWCWgMFpu4Bzlaca3fh5fbdM+Cx3iMzkUkB4TsC5Zo+aRsSShHJTexnL45yb4+lfgxh3jMzB50FBZ5uqb0pbhW42B6uezlhGvxK2HajbNbPRo4bId9qT0Kao0JLGvXSEdMz3GuP3yYBy/liWs39zOzrB26FBUUyn9AZ5LbRd+1TrrkCjGKFkZAhhWk3ge1DrOQaDpw=", result3);

            bool result4 = input.RsaVerifyData(result3);
            Assert.IsTrue(result4);
        }

        [TestMethod]
        public void TestJsonConvert()
        {
            UserInfo user = new UserInfo
            {
                Id = 1,
                Name = "Newcats",
                Gender = GenderEnum.Man,
                CreateTime = DateTime.Now
            };
            string result1 = user.ToJson();
            Assert.IsNotNull(result1);
            StringAssert.Contains(result1, "Newcats");

            var result2 = result1.Deserialize<UserInfo>();
            Assert.IsNotNull(result2);
            Assert.AreEqual(result2.GetType(), user.GetType());
        }

        [TestMethod]
        public void TestPinYin()
        {
            string result1 = "秦始皇".FirstPinYin();
            Assert.IsNotNull(result1);
            Assert.AreEqual("qsh", result1);

            string result2 = "秦始皇".PinYin();
            Assert.IsNotNull(result2);
            Assert.AreEqual("QinShiHuang", result2);
        }

        [TestMethod]
        public void TestRandomString()
        {
            string result1 = EncryptHelper.GetRandomString(8);
            Assert.IsNotNull(result1);
            Assert.AreEqual(8, result1.Length);

            string result2 = EncryptHelper.GetRandomNumber(8);
            Assert.IsNotNull(result2);
            Assert.AreEqual(8, result2.Length);

            string result3 = EncryptHelper.GetRandomKey(8);
            Assert.IsNotNull(result3);
            Assert.AreEqual(8, result3.Length);
        }

        [TestMethod]
        public void TestCreateKey()
        {
            string desKey = EncryptHelper.CreateDesKey();
            Assert.IsNotNull(desKey);
            Assert.AreEqual(8, desKey.Length);

            string tripleDesKey = EncryptHelper.CreateTripleDesKey();
            Assert.IsNotNull(tripleDesKey);
            Assert.AreEqual(24, tripleDesKey.Length);

            string aseKey = EncryptHelper.CreateAesKey();
            Assert.IsNotNull(aseKey);
            Assert.AreEqual(32, aseKey.Length);

            string aesIv = EncryptHelper.CreateAesIv();
            Assert.IsNotNull(aesIv);
            Assert.AreEqual(16, aesIv.Length);

            Models.RsaKey rsaKey1 = EncryptHelper.CreateRsaKey(Models.RsaKeyFormatEnum.Pkcs1, 1024, true);
            Assert.IsNotNull(rsaKey1);
            StringAssert.Contains(rsaKey1.PrivateKey, "PRIVATE");
            StringAssert.Contains(rsaKey1.PublicKey, "PUBLIC");

            Models.RsaKey rsaKey2 = EncryptHelper.CreateRsaKey(Models.RsaKeyFormatEnum.Pkcs8, 1024, true);
            Assert.IsNotNull(rsaKey2);
            StringAssert.Contains(rsaKey2.PrivateKey, "PRIVATE");
            StringAssert.Contains(rsaKey2.PublicKey, "PUBLIC");

            Models.RsaKey rsaKey3 = EncryptHelper.CreateRsaKey(Models.RsaKeyFormatEnum.Xml, 1024, true);
            Assert.IsNotNull(rsaKey3);
            StringAssert.Contains(rsaKey3.PrivateKey, "<InverseQ>");
            StringAssert.Contains(rsaKey3.PublicKey, "<Exponent>");
        }

        [TestMethod]
        public void TestSnowflakeId()
        {
            long id = IdHelper.Create();
            Assert.IsNotNull(id);
            Assert.AreEqual(19, id.ToString().Length);

            string idStr = IdHelper.Create(true);
            Assert.IsNotNull(idStr);
            Assert.AreEqual(27, idStr.Length);
        }
    }
}