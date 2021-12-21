using System.Collections.Generic;
using System.Diagnostics;
using Newcats.Utils.Helpers;
using SqlBulkCopyTest;

namespace SqlBulkCopyTest;

class Program
{

    const int TotalCount = 50;

    static void Main(string[] args)
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < TotalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }

        RunSqlServerTest(list);

    }

    static void RunSqlServerTest(List<NewcatsUserInfoTest> list)
    {
        SqlServerTest sqlServer = new SqlServerTest();
        sqlServer.Init();

        var t1 = sqlServer.InsertForEach(list);

        var t2 = sqlServer.InsertForEachNative(list);

        var t3 = sqlServer.InsertBulk(list);

        var t4 = sqlServer.InsertAppend(list);

        var t5 = sqlServer.SqlBulkCopy(list);

        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopy方法耗时:{t5}ms");
    }
}