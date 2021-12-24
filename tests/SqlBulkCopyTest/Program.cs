using System.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Newcats.Utils.Helpers;

namespace SqlBulkCopyTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("1.使用for循环测试");
        Console.WriteLine("2.使用BenchmarkDotNet测试");
        Console.Write("请选择（1 or 2）：");
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
            DataTable dt = new DataTable("NewcatsUserInfoTest");
            dt.Columns.Add("Id", typeof(long));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("CreateTime", typeof(DateTime));
            for (int i = 0; i < TotalCount; i++)
            {
                long id = IdHelper.Create();
                string name = EncryptHelper.GetRandomString(Random.Shared.Next(20));
                DateTime now = DateTime.Now;
                list.Add(new NewcatsUserInfoTest { Id = id, Name = name, CreateTime = now });
                dt.Rows.Add(id, name, now);
            }

            RunSqlServerTest(list, dt);
            RunMySqlTest(list, dt);
            RunPostgreSqlTest(list, dt);
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

    static void RunSqlServerTest(List<NewcatsUserInfoTest> list, DataTable dt)
    {
        SqlServerTest sqlServer = new SqlServerTest();
        sqlServer.Init();

        var t1 = sqlServer.InsertForEach(list);

        var t2 = sqlServer.InsertForEachNative(list);

        var t3 = sqlServer.InsertBulk(list);

        var t4 = sqlServer.InsertAppend(list);

        var t5 = sqlServer.SqlBulkCopyFromList(list);

        var t6 = sqlServer.SqlBulkCopyFromDataTable(dt);

        Console.WriteLine("\r\nSqlServer测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopyFromList方法耗时:{t5}ms\r\n\n6.SqlBulkCopyFromDataTable方法耗时:{t6}ms");
        Console.WriteLine("=========================================");
    }

    static void RunMySqlTest(List<NewcatsUserInfoTest> list, DataTable dt)
    {
        MySqlTest mySql = new MySqlTest();
        mySql.Init();

        var t1 = mySql.InsertForEach(list);

        var t2 = mySql.InsertForEachNative(list);

        var t3 = mySql.InsertBulk(list);

        var t4 = mySql.InsertAppend(list);

        var t5 = mySql.SqlBulkCopyFromList(list);

        var t6 = mySql.SqlBulkCopyFromDataTable(dt);

        Console.WriteLine("\r\nMySql测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopyFromList方法耗时:{t5}ms\r\n\n6.SqlBulkCopyFromDataTable方法耗时:{t6}ms");
        Console.WriteLine("=========================================");
    }

    static void RunPostgreSqlTest(List<NewcatsUserInfoTest> list, DataTable dt)
    {
        PostgreSqlTest pgSql = new PostgreSqlTest();
        pgSql.Init();

        var t1 = pgSql.InsertForEach(list);

        var t2 = pgSql.InsertForEachNative(list);

        var t3 = pgSql.InsertBulk(list);

        var t4 = pgSql.InsertAppend(list);

        var t5 = pgSql.SqlBulkCopyFromList(list);

        var t6 = pgSql.SqlBulkCopyFromDataTable(dt);

        Console.WriteLine("\r\nPostgreSql测试结果如下：");
        Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertForEachNative方法耗时:{t2}ms\r\n\n3.InsertBulk方法耗时:{t3}ms\r\n\n4.InsertAppend方法耗时:{t4}ms\r\n\n5.SqlBulkCopyFromList方法耗时:{t5}ms\r\n\n6.SqlBulkCopyFromDataTable方法耗时:{t6}ms");
        Console.WriteLine("=========================================");
    }
}

/// <summary>
/// SqlBulkCopy测试
/// </summary>
public class BulkCopyContext
{
    const int totalCount = 5000;

    #region SqlServer
    [Benchmark]
    public void SqlServer_InsertForEach()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        SqlServerTest test = new SqlServerTest();
        test.Init();
        test.InsertForEach(list);
    }

    [Benchmark]
    public void SqlServer_InsertAppend()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        SqlServerTest test = new SqlServerTest();
        test.Init();
        test.InsertAppend(list);
    }

    [Benchmark]
    public void SqlServer_SqlBulkCopy_FromList()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        SqlServerTest test = new SqlServerTest();
        test.Init();
        test.SqlBulkCopyFromList(list);
    }

    [Benchmark]
    public void SqlServer_SqlBulkCopy_FromDataTable()
    {
        DataTable dt = new DataTable("NewcatsUserInfoTest");
        dt.Columns.Add("Id", typeof(long));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("CreateTime", typeof(DateTime));
        for (int i = 0; i < totalCount; i++)
        {
            var id = IdHelper.Create();
            var name = EncryptHelper.GetRandomString(Random.Shared.Next(20));
            var now = DateTime.Now;
            dt.Rows.Add(id, name, now);
        }
        SqlServerTest test = new SqlServerTest();
        test.Init();
        test.SqlBulkCopyFromDataTable(dt);
    }
    #endregion

    #region MySql
    [Benchmark]
    public void MySql_InsertForEach()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        MySqlTest test = new MySqlTest();
        test.Init();
        test.InsertForEach(list);
    }

    [Benchmark]
    public void MySql_InsertAppend()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        MySqlTest test = new MySqlTest();
        test.Init();
        test.InsertAppend(list);
    }

    [Benchmark]
    public void MySql_SqlBulkCopy_FromList()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        MySqlTest test = new MySqlTest();
        test.Init();
        test.SqlBulkCopyFromList(list);
    }

    [Benchmark]
    public void MySql_SqlBulkCopy_FromDataTable()
    {
        DataTable dt = new DataTable("NewcatsUserInfoTest");
        dt.Columns.Add("Id", typeof(long));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("CreateTime", typeof(DateTime));
        for (int i = 0; i < totalCount; i++)
        {
            var id = IdHelper.Create();
            var name = EncryptHelper.GetRandomString(Random.Shared.Next(20));
            var now = DateTime.Now;
            dt.Rows.Add(id, name, now);
        }
        MySqlTest test = new MySqlTest();
        test.Init();
        test.SqlBulkCopyFromDataTable(dt);
    }
    #endregion

    #region PostgreSql
    [Benchmark]
    public void PostgreSql_InsertForEach()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        PostgreSqlTest test = new PostgreSqlTest();
        test.Init();
        test.InsertForEach(list);
    }

    [Benchmark]
    public void PostgreSql_InsertAppend()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        PostgreSqlTest test = new PostgreSqlTest();
        test.Init();
        test.InsertAppend(list);
    }

    [Benchmark]
    public void PostgreSql_SqlBulkCopy_FromList()
    {
        List<NewcatsUserInfoTest> list = new List<NewcatsUserInfoTest>();
        for (int i = 0; i < totalCount; i++)
        {
            NewcatsUserInfoTest u = new NewcatsUserInfoTest()
            {
                Id = IdHelper.Create(),
                Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                CreateTime = DateTime.Now
            };
            list.Add(u);
        }
        PostgreSqlTest test = new PostgreSqlTest();
        test.Init();
        test.SqlBulkCopyFromList(list);
    }

    [Benchmark]
    public void PostgreSql_SqlBulkCopy_FromDataTable()
    {
        DataTable dt = new DataTable("NewcatsUserInfoTest");
        dt.Columns.Add("Id", typeof(long));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("CreateTime", typeof(DateTime));
        for (int i = 0; i < totalCount; i++)
        {
            var id = IdHelper.Create();
            var name = EncryptHelper.GetRandomString(Random.Shared.Next(20));
            var now = DateTime.Now;
            dt.Rows.Add(id, name, now);
        }
        PostgreSqlTest test = new PostgreSqlTest();
        test.Init();
        test.SqlBulkCopyFromDataTable(dt);
    }
    #endregion
}