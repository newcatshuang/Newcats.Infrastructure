/***************************************************************************
 *GUID: 85712df5-cd39-4b2d-9ff8-ce0690635645
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-15 12:28:54
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newcats.Utils.Extensions;

namespace Newcats.Utils.UnitTest.Extensions
{
    [TestClass]
    public class CollectionExtensionsTest
    {
        [TestMethod]
        public void TestToDataTable()
        {
            List<UserInfo> input = new List<UserInfo>();
            input.Add(new UserInfo() { Id = 1, Name = "Newcats", CreateTime = DateTime.Now });
            input.Add(new UserInfo() { Id = 2, Name = "Huang", CreateTime = DateTime.Now });
            input.Add(new UserInfo() { Id = 3, Name = "NewcatsHuang", CreateTime = DateTime.Now });
            DataTable result = input.ToDataTable();
            Assert.IsNotNull(result);
            Assert.AreEqual(input.Count, result.Rows.Count);
            Assert.AreEqual(input[0].Id, result.Rows[0]["Id"]);
            Assert.AreEqual(input[0].Name, result.Rows[0]["Name"]);
            Assert.AreEqual(input[0].CreateTime, result.Rows[0]["CreateTime"]);
            Assert.AreNotEqual(input[0].Id, result.Rows[1]["Id"]);
        }

        [TestMethod]
        public void TestToList()
        {
            DataTable input = new DataTable();
            input.Columns.Add("Id", typeof(long));
            input.Columns.Add("Name", typeof(string));
            input.Columns.Add("CreateTime", typeof(DateTime));
            input.Rows.Add(1, "Newcats", DateTime.Now);
            input.Rows.Add(2, "Huang", DateTime.Now);
            List<UserInfo> result = input.ToList<UserInfo>();
            Assert.IsNotNull(result);
            Assert.AreEqual(input.Rows.Count, result.Count);
            Assert.AreEqual(Convert.ToInt64(input.Rows[0]["Id"]), result[0].Id);
            Assert.AreEqual(input.Rows[0]["Name"].ToString(), result[0].Name);
            Assert.AreEqual(Convert.ToDateTime(input.Rows[0]["CreateTime"]), result[0].CreateTime);
            Assert.AreNotEqual(input.Rows[0]["Name"], result[1].Name);
        }
    }
}