/***************************************************************************
 *GUID: 3544718c-d3d1-4120-b355-5bc55a695c62
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-15 13:10:16
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newcats.Utils.Extensions;
using Newcats.Utils.Models;

namespace Newcats.Utils.UnitTest.Extensions
{
    [TestClass]
    public class EnumExtensionsTest
    {
        [TestMethod]
        public void TestGetDescription()
        {
            GenderEnum input = GenderEnum.Unknown;
            string result = input.GetDescription();
            Assert.IsNotNull(result);
            Assert.AreEqual("未知", result);
        }

        [TestMethod]
        public void TestGetEnumDescription()
        {
            EnumDescription? result = GenderEnum.Female.GetEnumDescription();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EnumDescription), result.GetType());
            Assert.AreEqual("Female", result.Name);
            Assert.AreEqual("女", result.Description);
            Assert.AreEqual(2, result.Value);
        }

        [TestMethod]
        public void TestGetAllEnumDescriptions()
        {
            List<EnumDescription>? result = GenderEnum.Female.GetAllEnumDescriptions();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(List<EnumDescription>), result.GetType());
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(0, result[0].Value);
            Assert.AreEqual("Unknown", result[0].Name);
            Assert.AreEqual("未知", result[0].Description);

            Assert.AreEqual(1, result[1].Value);
            Assert.AreEqual("Man", result[1].Name);
            Assert.AreEqual("男", result[1].Description);
        }
    }
}