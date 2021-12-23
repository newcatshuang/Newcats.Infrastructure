using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Newcats.Utils.Helpers;

namespace SqlBulkCopyTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("1.使用for循环测试SqlServer、MySql、ostgreSql");
        Console.WriteLine("2.使用BenchmarkDotNet测试SqlServer的InsertForEach、SqlBulkCopy");
        Console.Write("请选择1或者2：");
        string select = Console.ReadLine();
        if (select == "1")
        {
            int TotalCount = 50;
            Console.Write("输入需要测试的数据量：");
            string countStr = Console.ReadLine();
            Console.WriteLine("=========================================");
            int outer = 0;
            if (int.TryParse(countStr, out outer))
                TotalCount = outer;

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
            RunMySqlTest(list);
            RunPostgreSqlTest(list);
        }
        else if (select == "2")
        {
            Summary summary = BenchmarkRunner.Run<BulkCopyContext>();
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Hello World");
        }
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

        Console.WriteLine("\r\nSqlServer测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopy方法耗时:{t5}ms");
        Console.WriteLine("=========================================");
    }

    static void RunMySqlTest(List<NewcatsUserInfoTest> list)
    {
        MySqlTest mySql = new MySqlTest();
        mySql.Init();

        var t1 = mySql.InsertForEach(list);

        var t2 = mySql.InsertForEachNative(list);

        var t3 = mySql.InsertBulk(list);

        var t4 = mySql.InsertAppend(list);

        var t5 = mySql.SqlBulkCopy(list);

        Console.WriteLine("\r\nMySql测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopy方法耗时:{t5}ms");
        Console.WriteLine("=========================================");
    }

    static void RunPostgreSqlTest(List<NewcatsUserInfoTest> list)
    {
        PostgreSqlTest pgSql = new PostgreSqlTest();
        pgSql.Init();

        var t1 = pgSql.InsertForEach(list);

        var t2 = pgSql.InsertForEachNative(list);

        var t3 = pgSql.InsertBulk(list);

        var t4 = pgSql.InsertAppend(list);

        var t5 = pgSql.SqlBulkCopy(list);

        Console.WriteLine("\r\nPostgreSql测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopy方法耗时:{t5}ms");
        Console.WriteLine("=========================================");
    }


}

/// <summary>
/// SqlBulkCopy测试
/// </summary>
public class BulkCopyContext
{
    /// <summary>
    /// ForEach
    /// </summary>
    [Benchmark]
    public void InsertForEach()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < 10; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        SqlServerTest sqlServer = new SqlServerTest();
        sqlServer.Init();
        sqlServer.InsertForEach(list);
    }

    /// <summary>
    /// SqlBulkCopy
    /// </summary>
    [Benchmark]
    public void SqlBulkCopy()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < 10; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        SqlServerTest sqlServer = new SqlServerTest();
        sqlServer.Init();
        sqlServer.SqlBulkCopy(list);
    }
}