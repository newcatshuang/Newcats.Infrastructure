# 批量插入之SqlBulkCopy

作者：NewcatsHuang  
时间：2021-12-25  
完整代码：[Github传送门](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/tests/SqlBulkCopyTest)

## 一.目录

* [批量插入的几种方法](#批量插入的几种方法)
* [SqlBulkCopy介绍](#SqlBulkCopy介绍)
* [For循环测试及Benchmark测试](#For循环测试及Benchmark测试)
* [使用时的注意事项](#使用时的注意事项)

## <span id="批量插入的几种方法">二.方法介绍</span>

### 1.for循环插入  
对集合数据进行遍历，每次只插入集合的一条数据，对应的SQL语句为：

```sql
insert into UserInfo(Id,Name) values (@Id,@Name);
```

### 2.拼接sql  
也需要for循环遍历，只是一条语句能插入多个数据，对应的SQL语句为：

```sql
insert into UserInfo(Id,Name) values (@Id1,@Name1),(@Id2,@Name2)...
```

### 3.SqlBulkCopy批量插入  
利用各个数据库的特性，直接从文件复制到表

## <span id="SqlBulkCopy介绍">三.SqlBulkCopy介绍</span>

### 1.SqlServer数据库

* 使用  Microsoft.Data.SqlClient.SqlBulkCopy 类封装的方法

* BULK INSERT Northwind.dbo.[OrderDetails] FROM 'f:\mydata\data.tbl' WITH (FORMATFILE='f:\mydata\data.fmt');

> Microsoft SQL Server 包含一个名为 bcp 的受欢迎的命令行实用工具，以便将较大文件快速大容量复制到 SQL Server 数据库的表或视图中 。 SqlBulkCopy 类允许你编写可提供类似功能的托管代码解决方案。 还可通过其他方法将数据加载到 SQL Server 表（例如 INSERT 语句），但 SqlBulkCopy 可提供显著的性能优势。SqlBulkCopy 类可用于只将数据写入 SQLServer 表。但是， 数据源不限于 SQL Server；可以使用任何数据源，只要数据可以加载到 DataTable 实例或使用IDataReader 实例读取即可。  
使用 SqlBulkCopy 类，你可以执行以下操作：  
单次大容量复制操作  
多次大容量复制操作  
事务中的大容量复制操作

以上内容来自微软官方文档： [Bulk Copy Operations in SQL Server](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/bulk-copy-operations-in-sql-server)  

### 2.MySql数据库  

* 使用 MySqlConnector.MySqlBulkCopy 类封装的方法

* LOAD DATA LOCAL INFILE '/tmp/test.txt' INTO TABLE test;

> LOAD DATA语句以非常高的速度将文本文件中的行读取到表中。可以从服务器主机或客户端主机读取文件，具体取决于是否给出了修饰符。还会影响数据解释和错误处理。
  
以上内容来自MySql官网：[LOAD DATA Statement](https://dev.mysql.com/doc/refman/8.0/en/load-data.html)  

### 3.PostgreSql数据库

* COPY public.userinfo (id,name,createtime) FROM STDIN (FORMAT BINARY);  

> COPY moves data between PostgreSQL tables and standard file-system files. COPY TO copies the contents of a table to a file, while COPY FROM copies data from a file to a table (appending the data to whatever is in the table already). COPY TO can also copy the results of a SELECT query.
If a column list is specified, COPY TO copies only the data in the specified columns to the file. For COPY FROM, each field in the file is inserted, in order, into the specified column. Table columns not specified in the COPY FROM column list will receive their default values.
COPY with a file name instructs the PostgreSQL server to directly read from or write to a file. The file must be accessible by the PostgreSQL user (the user ID the server runs as) and the name must be specified from the viewpoint of the server. When PROGRAM is specified, the server executes the given command and reads from the standard output of the program, or writes to the standard input of the program. The command must be specified from the viewpoint of the server, and be executable by the PostgreSQL user. When STDIN or STDOUT is specified, data is transmitted via the connection between the client and the server.
Each backend running COPY will report its progress in the pg_stat_progress_copy view. See Section 28.4.6 for details.
  
> COPY在 PostgreSQL表和标准文件系统文件之间移动数据。 将表的内容复制到文件，同时将数据从文件复制到表（将数据追加到表中已有的任何内容）。 还可以复制查询的结果。COPY TOCOPY FROMCOPY TOSELECT
如果指定了列列表，则 仅将指定列中的数据复制到文件中。对于 ，文件中的每个字段将按顺序插入到指定的列中。列列表中未指定的表列将收到其默认值。COPY TOCOPY FROMCOPY FROM
COPY与文件名指示PostgreSQL服务器直接读取或写入文件。该文件必须可由PostgreSQL用户（服务器运行时的用户 ID）访问，并且必须从服务器的角度指定名称。当指定时，服务器执行给定的命令并从程序的标准输出中读取，或写入程序的标准输入。该命令必须从服务器的角度指定，并且可由PostgreSQL用户执行。指定 或 时，数据通过客户端和服务器之间的连接传输。PROGRAMSTDINSTDOUT
每个正在运行的后端都将在视图中报告其进度。有关详细信息，请参见第 28.4.6 节。COPYpg_stat_progress_copy  

以上内容来自PostgreSql官网：[sql copy](https://www.postgresql.org/docs/current/sql-copy.html
)

## <span id="For循环测试及Benchmark测试">四.性能测试</span>

### 1.环境

|数据库|版本|OS|CPU|RAM|说明|
|-------|-------|-------|-------|-------|-------|
|SqlServer|v2017|Win10 21H2|i7-9700k|64GB|VMWare宿主机|
|MySql|v8.0.27|Ubuntu Server 21.10|i7-9700k 2core|4GB|VMWare虚拟机1|
|PostgreSql|v13.5|Ubuntu Server 21.10|i7-9700k 2core|4GB|VMWare虚拟机2|

### 2.代码

#### 2.1 for循环-dapper执行foreach循环插入数据  
SqlServer版本

```csharp
/// <summary>
/// dapper执行foreach循环插入数据
/// </summary>
internal long InsertForEach(List<NewcatsUserInfoTest> list)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    using (SqlConnection conn = new SqlConnection(ConnectionString))
    {
        foreach (NewcatsUserInfoTest test in list)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            result += conn.Execute(SqlText, test, commandType: System.Data.CommandType.Text);
        }
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.2 for循环-ADO.NET的foreach循环插入数据  
MySql版本

```csharp
/// <summary>
/// ADO.NET的foreach循环插入数据
/// </summary>
internal long InsertForEachNative(List<NewcatsUserInfoTest> list)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    using (MySqlConnection conn = new MySqlConnection(ConnectionString))
    {
        if (conn.State == ConnectionState.Closed)
            conn.Open();
        foreach (NewcatsUserInfoTest item in list)
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                string sqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id{result},@Name{result},@CreateTime{result})";
                cmd.Connection = conn;
                cmd.CommandText = sqlText;
                cmd.Parameters.Add(new MySqlParameter("@Id" + result.ToString(), item.Id));
                cmd.Parameters.Add(new MySqlParameter("@Name" + result.ToString(), item.Name));
                cmd.Parameters.Add(new MySqlParameter("@CreateTime" + result.ToString(), item.CreateTime));
                result += cmd.ExecuteNonQuery();
            }
        }
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.3 for循环-dapper直接传list参数  
PostgreSql版本

```csharp
/// <summary>
/// dapper直接传list参数
/// </summary>
internal long InsertBulk(List<NewcatsUserInfoTest> list)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
    {
        if (conn.State == ConnectionState.Closed)
            conn.Open();
        result = conn.Execute(SqlText, list, commandType: System.Data.CommandType.Text);
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.4 for循环-dapper拼接sql语句  
SqlServer版本

```csharp
/// <summary>
/// dapper拼接sql语句
/// </summary>
internal long InsertAppend(List<NewcatsUserInfoTest> list)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    const int perCount = 500;
    int times = Convert.ToInt32(Math.Ceiling(list.Count * 1.0 / perCount));
    for (int i = 0; i < times; i++)
    {
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            StringBuilder sb = new StringBuilder($"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES");
            var perList = list.Skip(i * perCount).Take(perCount);
            int index = 0;
            DynamicParameters dp = new DynamicParameters();
            foreach (NewcatsUserInfoTest test in perList)
            {
                sb.Append($"(@Id{index},@Name{index},@CreateTime{index}),");
                dp.Add($"@Id{index}", test.Id);
                dp.Add($"@Name{index}", test.Name);
                dp.Add($"@CreateTime{index}", test.CreateTime);
                index++;
            }
            result += conn.Execute(sb.ToString().TrimEnd(','), dp, commandType: System.Data.CommandType.Text);
        }
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.5 for循环-SqlBulkCopy插入-FromList  
MySql版本

```csharp
/// <summary>
/// SqlBulkCopy插入-FromList
/// </summary>
internal long SqlBulkCopyFromList(List<NewcatsUserInfoTest> list)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    using (MySqlConnection conn = new MySqlConnection(ConnectionString))
    {
        if (conn.State == ConnectionState.Closed)
            conn.Open();
        MySqlBulkCopy copy = new MySqlBulkCopy(conn);
        copy.DestinationTableName = TableName;
        var r = copy.WriteToServer(list.ToDataTable());
        result = r.RowsInserted;
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.6 for循环-SqlBulkCopy插入-FromDataTable  
PostgreSql版本

```csharp
/// <summary>
/// SqlBulkCopy插入-FromDataTable
/// </summary>
internal long SqlBulkCopyFromDataTable(DataTable dt)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    int result = 0;
    using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
    {
        if (conn.State == ConnectionState.Closed)
            conn.Open();
        using (NpgSqlBulkCopy copy = new NpgSqlBulkCopy(conn, TableName))
        {
            copy.WriteToServer(dt);
        }
    }
    sw.Stop();
    return sw.ElapsedMilliseconds;
}
```

#### 2.7 Benchmark测试(SqlServer示例)

```csharp
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
}
```

### 3.for循环测试结果

| Database/Method         | Counts | InsertForEach(ms) | InsertForEachNative(ms) | InsertBulk(ms) | InsertAppend(ms) | SqlBulkCopyFromList(ms) | SqlBulkCopyFromDataTable(ms) |
|------------------|-----------|---------------|---------------------|------------|--------------|---------------------|--------------------------|
| SqlServer        | 1 | 35 | 1 |            | 6 | 27 | 2 |
| MySql            | 1 | 26 | 1 | 1 | 1 | 19 | 2 |
| PostgreSql       | 1 | 10 | 2 |            | 3 | 42 | 1 |
| SqlServer        | 10 | 42 | 5 | 1 | 6 | 25 | 1 |
| MySql            | 10 | 35 | 6 | 6 | 1 | 20 | 2 |
| PostgreSql       | 10 | 14 | 6 | 3 | 3 | 42 | 1 |
| SqlServer        | 100 | 55 | 48 | 13 | 13 | 27 | 1 |
| MySql            | 100 | 88 | 54 | 53 | 3 | 20 | 1 |
| PostgreSql       | 100 | 37 | 52 | 68 | 4 | 43 | 2 |
| SqlServer        | 1000 | 185 | 475 | 122 | 256 | 29 | 3 |
| MySql            | 1000 | 630 | 582 | 560 | 19 | 25 | 7 |
| PostgreSql       | 1000 | 549 | 474 | 521 | 14 | 45 | 4 |
| SqlServer(1w)    | 10000 | 1330 | 4506 | 1216 | 2041 | 50 | 16 |
| MySql            | 10000 | 6033 | 6815 | 6461 | 113 | 75 | 45 |
| PostgreSql       | 10000 | 5310 | 5254 | 5131 | 80 | 72 | 26 |
| SqlServer(10w)   | 100000 | 12500 | 43793 | 12065 | 19543 | 264 | 119 |
| MySql            | 100000 | 61492 | 60210 | 61099 | 1201 | 534 | 394 |
| PostgreSql       | 100000 | 53870 | 53407 | 55238 | 749 | 345 | 208 |
| SqlServer(100w)  | 1000000 | 128015 | 452346 | 122330 | 194935 | 2253 | 1123 |
| MySql            | 1000000 |               |                     |            |              | 4766 | 3873 |
| PostgreSql       | 1000000 |               |                     |            |              | 2657 | 1351 |
| SqlServer(1000w) | 10000000 |               |                     |            |              | 25721 | 15312 |
| MySql            | 10000000 |               |                     |            | 115881 | 49502 | 31687 |
| PostgreSql       | 10000000 |               |                     |            | 71680 | 25139 | 13041 |

<details>
<summary>展开查看详细的for循环测试结果</summary>

<pre><code>
---
SqlServer测试结果如下：
集合大小:1

1.InsertForEach方法耗时:35ms

2.InsertForEachNative方法耗时:1ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:6ms

5.SqlBulkCopyFromList方法耗时:27ms

6.SqlBulkCopyFromDataTable方法耗时:2ms

MySql测试结果如下：
集合大小:1

1.InsertForEach方法耗时:26ms

2.InsertForEachNative方法耗时:1ms

3.InsertBulk方法耗时:1ms

4.InsertAppend方法耗时:1ms

5.SqlBulkCopyFromList方法耗时:19ms

6.SqlBulkCopyFromDataTable方法耗时:2ms

PostgreSql测试结果如下：
集合大小:1

1.InsertForEach方法耗时:10ms

2.InsertForEachNative方法耗时:2ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:3ms

5.SqlBulkCopyFromList方法耗时:42ms

6.SqlBulkCopyFromDataTable方法耗时:1ms

---

SqlServer测试结果如下：
集合大小:10

1.InsertForEach方法耗时:42ms

2.InsertForEachNative方法耗时:5ms

3.InsertBulk方法耗时:1ms

4.InsertAppend方法耗时:6ms

5.SqlBulkCopyFromList方法耗时:25ms

6.SqlBulkCopyFromDataTable方法耗时:1ms

MySql测试结果如下：
集合大小:10

1.InsertForEach方法耗时:35ms

2.InsertForEachNative方法耗时:6ms

3.InsertBulk方法耗时:6ms

4.InsertAppend方法耗时:1ms

5.SqlBulkCopyFromList方法耗时:20ms

6.SqlBulkCopyFromDataTable方法耗时:2ms

PostgreSql测试结果如下：
集合大小:10

1.InsertForEach方法耗时:14ms

2.InsertForEachNative方法耗时:6ms

3.InsertBulk方法耗时:3ms

4.InsertAppend方法耗时:3ms

5.SqlBulkCopyFromList方法耗时:42ms

6.SqlBulkCopyFromDataTable方法耗时:1ms

---

SqlServer测试结果如下：
集合大小:100

1.InsertForEach方法耗时:55ms

2.InsertForEachNative方法耗时:48ms

3.InsertBulk方法耗时:13ms

4.InsertAppend方法耗时:13ms

5.SqlBulkCopyFromList方法耗时:27ms

6.SqlBulkCopyFromDataTable方法耗时:1ms

MySql测试结果如下：
集合大小:100

1.InsertForEach方法耗时:88ms

2.InsertForEachNative方法耗时:54ms

3.InsertBulk方法耗时:53ms

4.InsertAppend方法耗时:3ms

5.SqlBulkCopyFromList方法耗时:20ms

6.SqlBulkCopyFromDataTable方法耗时:1ms

PostgreSql测试结果如下：
集合大小:100

1.InsertForEach方法耗时:37ms

2.InsertForEachNative方法耗时:52ms

3.InsertBulk方法耗时:68ms

4.InsertAppend方法耗时:4ms

5.SqlBulkCopyFromList方法耗时:43ms

6.SqlBulkCopyFromDataTable方法耗时:2ms

---

SqlServer测试结果如下：
集合大小:1000

1.InsertForEach方法耗时:185ms

2.InsertForEachNative方法耗时:475ms

3.InsertBulk方法耗时:122ms

4.InsertAppend方法耗时:256ms

5.SqlBulkCopyFromList方法耗时:29ms

6.SqlBulkCopyFromDataTable方法耗时:3ms

MySql测试结果如下：
集合大小:1000

1.InsertForEach方法耗时:630ms

2.InsertForEachNative方法耗时:582ms

3.InsertBulk方法耗时:560ms

4.InsertAppend方法耗时:19ms

5.SqlBulkCopyFromList方法耗时:25ms

6.SqlBulkCopyFromDataTable方法耗时:7ms

PostgreSql测试结果如下：
集合大小:1000

1.InsertForEach方法耗时:549ms

2.InsertForEachNative方法耗时:474ms

3.InsertBulk方法耗时:521ms

4.InsertAppend方法耗时:14ms

5.SqlBulkCopyFromList方法耗时:45ms

6.SqlBulkCopyFromDataTable方法耗时:4ms

---

SqlServer测试结果如下：
集合大小:10000

1.InsertForEach方法耗时:1330ms

2.InsertForEachNative方法耗时:4506ms

3.InsertBulk方法耗时:1216ms

4.InsertAppend方法耗时:2041ms

5.SqlBulkCopyFromList方法耗时:50ms

6.SqlBulkCopyFromDataTable方法耗时:16ms

MySql测试结果如下：
集合大小:10000

1.InsertForEach方法耗时:6033ms

2.InsertForEachNative方法耗时:6815ms

3.InsertBulk方法耗时:6461ms

4.InsertAppend方法耗时:113ms

5.SqlBulkCopyFromList方法耗时:75ms

6.SqlBulkCopyFromDataTable方法耗时:45ms

PostgreSql测试结果如下：
集合大小:10000

1.InsertForEach方法耗时:5310ms

2.InsertForEachNative方法耗时:5254ms

3.InsertBulk方法耗时:5131ms

4.InsertAppend方法耗时:80ms

5.SqlBulkCopyFromList方法耗时:72ms

6.SqlBulkCopyFromDataTable方法耗时:26ms

---

SqlServer测试结果如下：
集合大小:100000

1.InsertForEach方法耗时:12500ms

2.InsertForEachNative方法耗时:43793ms

3.InsertBulk方法耗时:12065ms

4.InsertAppend方法耗时:19543ms

5.SqlBulkCopyFromList方法耗时:264ms

6.SqlBulkCopyFromDataTable方法耗时:119ms

MySql测试结果如下：
集合大小:100000

1.InsertForEach方法耗时:61492ms

2.InsertForEachNative方法耗时:60210ms

3.InsertBulk方法耗时:61099ms

4.InsertAppend方法耗时:1201ms

5.SqlBulkCopyFromList方法耗时:534ms

6.SqlBulkCopyFromDataTable方法耗时:394ms

PostgreSql测试结果如下：
集合大小:100000

1.InsertForEach方法耗时:53870ms

2.InsertForEachNative方法耗时:53407ms

3.InsertBulk方法耗时:55238ms

4.InsertAppend方法耗时:749ms

5.SqlBulkCopyFromList方法耗时:345ms

6.SqlBulkCopyFromDataTable方法耗时:208ms

---

SqlServer测试结果如下：
集合大小:1000000

1.InsertForEach方法耗时:128015ms

2.InsertForEachNative方法耗时:452346ms

3.InsertBulk方法耗时:122330ms

4.InsertAppend方法耗时:194935ms

5.SqlBulkCopyFromList方法耗时:2253ms

6.SqlBulkCopyFromDataTable方法耗时:1123ms

MySql测试结果如下：
集合大小:1000000

1.InsertForEach方法耗时:0ms

2.InsertForEachNative方法耗时:0ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:11982ms

5.SqlBulkCopyFromList方法耗时:4766ms

6.SqlBulkCopyFromDataTable方法耗时:3873ms

PostgreSql测试结果如下：
集合大小:1000000

1.InsertForEach方法耗时:0ms

2.InsertForEachNative方法耗时:0ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:7101ms

5.SqlBulkCopyFromList方法耗时:2657ms

6.SqlBulkCopyFromDataTable方法耗时:1351ms

---

SqlServer测试结果如下：
集合大小:10000000

1.InsertForEach方法耗时:0ms

2.InsertForEachNative方法耗时:0ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:0ms

5.SqlBulkCopyFromList方法耗时:25721ms

6.SqlBulkCopyFromDataTable方法耗时:15312ms

MySql测试结果如下：
集合大小:10000000

1.InsertForEach方法耗时:0ms

2.InsertForEachNative方法耗时:0ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:115881ms

5.SqlBulkCopyFromList方法耗时:49502ms

6.SqlBulkCopyFromDataTable方法耗时:31687ms

PostgreSql测试结果如下：
集合大小:10000000

1.InsertForEach方法耗时:0ms

2.InsertForEachNative方法耗时:0ms

3.InsertBulk方法耗时:0ms

4.InsertAppend方法耗时:71680ms

5.SqlBulkCopyFromList方法耗时:25139ms

6.SqlBulkCopyFromDataTable方法耗时:13041ms

---
</code></pre>
</details>
  
<br>
<br>

### 4.Benchmark测试结果

```cmd
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1415 (21H2)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
```
  
---
totalCount=1
|                               Method |     Mean |     Error |    StdDev |
|------------------------------------- |---------:|----------:|----------:|
|              SqlServer_InsertForEach | 1.451 ms | 0.0046 ms | 0.0043 ms |
|               SqlServer_InsertAppend | 1.450 ms | 0.0074 ms | 0.0066 ms |
|       SqlServer_SqlBulkCopy_FromList | 2.060 ms | 0.0322 ms | 0.0285 ms |
|  SqlServer_SqlBulkCopy_FromDataTable | 2.054 ms | 0.0407 ms | 0.0381 ms |
|                  MySql_InsertForEach | 7.352 ms | 0.1453 ms | 0.3033 ms |
|                   MySql_InsertAppend | 7.553 ms | 0.1501 ms | 0.2338 ms |
|           MySql_SqlBulkCopy_FromList | 7.875 ms | 0.1539 ms | 0.2735 ms |
|      MySql_SqlBulkCopy_FromDataTable | 7.969 ms | 0.1549 ms | 0.1722 ms |
|             PostgreSql_InsertForEach | 2.091 ms | 0.0416 ms | 0.1082 ms |
|              PostgreSql_InsertAppend | 2.095 ms | 0.0416 ms | 0.1005 ms |
|      PostgreSql_SqlBulkCopy_FromList | 4.061 ms | 0.0804 ms | 0.1449 ms |
| PostgreSql_SqlBulkCopy_FromDataTable | 4.097 ms | 0.0808 ms | 0.1436 ms |
---

totalCount=10
|                               Method |      Mean |     Error |    StdDev |
|------------------------------------- |----------:|----------:|----------:|
|              SqlServer_InsertForEach |  2.476 ms | 0.0091 ms | 0.0085 ms |
|               SqlServer_InsertAppend |  1.776 ms | 0.0129 ms | 0.0121 ms |
|       SqlServer_SqlBulkCopy_FromList |  2.079 ms | 0.0175 ms | 0.0146 ms |
|  SqlServer_SqlBulkCopy_FromDataTable |  2.081 ms | 0.0264 ms | 0.0220 ms |
|                  MySql_InsertForEach | 13.359 ms | 0.2609 ms | 0.4212 ms |
|                   MySql_InsertAppend |  7.787 ms | 0.1556 ms | 0.3350 ms |
|           MySql_SqlBulkCopy_FromList |  8.336 ms | 0.1659 ms | 0.3157 ms |
|      MySql_SqlBulkCopy_FromDataTable |  8.437 ms | 0.1682 ms | 0.2946 ms |
|             PostgreSql_InsertForEach |  7.144 ms | 0.1386 ms | 0.1898 ms |
|              PostgreSql_InsertAppend |  2.115 ms | 0.0418 ms | 0.0775 ms |
|      PostgreSql_SqlBulkCopy_FromList |  4.020 ms | 0.0799 ms | 0.1684 ms |
| PostgreSql_SqlBulkCopy_FromDataTable |  4.082 ms | 0.0772 ms | 0.1373 ms |
---

totalCount=100
|                               Method |      Mean |     Error |    StdDev |
|------------------------------------- |----------:|----------:|----------:|
|              SqlServer_InsertForEach | 13.920 ms | 0.1864 ms | 0.1652 ms |
|               SqlServer_InsertAppend |  8.570 ms | 0.0443 ms | 0.0393 ms |
|       SqlServer_SqlBulkCopy_FromList |  2.432 ms | 0.0477 ms | 0.0567 ms |
|  SqlServer_SqlBulkCopy_FromDataTable |  2.339 ms | 0.0361 ms | 0.0338 ms |
|                  MySql_InsertForEach | 63.129 ms | 1.2535 ms | 2.6983 ms |
|                   MySql_InsertAppend |  8.332 ms | 0.1603 ms | 0.1646 ms |
|           MySql_SqlBulkCopy_FromList |  8.549 ms | 0.1457 ms | 0.2589 ms |
|      MySql_SqlBulkCopy_FromDataTable |  8.567 ms | 0.1697 ms | 0.2434 ms |
|             PostgreSql_InsertForEach | 55.740 ms | 1.1029 ms | 2.3503 ms |
|              PostgreSql_InsertAppend |  2.853 ms | 0.0550 ms | 0.0752 ms |
|      PostgreSql_SqlBulkCopy_FromList |  4.393 ms | 0.0870 ms | 0.1591 ms |
| PostgreSql_SqlBulkCopy_FromDataTable |  4.373 ms | 0.0872 ms | 0.1528 ms |

---
totalCount=1000
|                               Method |       Mean |      Error |     StdDev |
|------------------------------------- |-----------:|-----------:|-----------:|
|              SqlServer_InsertForEach | 127.432 ms |  0.6471 ms |  0.5737 ms |
|               SqlServer_InsertAppend | 230.073 ms |  0.4374 ms |  0.3877 ms |
|       SqlServer_SqlBulkCopy_FromList |   5.425 ms |  0.0194 ms |  0.0162 ms |
|  SqlServer_SqlBulkCopy_FromDataTable |   5.283 ms |  0.0263 ms |  0.0233 ms |
|                  MySql_InsertForEach | 606.386 ms | 11.0222 ms | 16.8320 ms |
|                   MySql_InsertAppend |  20.716 ms |  0.3511 ms |  0.3285 ms |
|           MySql_SqlBulkCopy_FromList |  15.641 ms |  0.3126 ms |  0.4065 ms |
|      MySql_SqlBulkCopy_FromDataTable |  15.161 ms |  0.2908 ms |  0.3461 ms |
|             PostgreSql_InsertForEach | 540.577 ms | 10.7744 ms | 23.4226 ms |
|              PostgreSql_InsertAppend |   8.047 ms |  0.1173 ms |  0.1097 ms |
|      PostgreSql_SqlBulkCopy_FromList |   7.259 ms |  0.1192 ms |  0.1057 ms |
| PostgreSql_SqlBulkCopy_FromDataTable |   7.232 ms |  0.1189 ms |  0.0993 ms |
---
totalCount=10000
|                               Method |        Mean |      Error |     StdDev |
|------------------------------------- |------------:|-----------:|-----------:|
|              SqlServer_InsertForEach | 1,253.01 ms |   2.507 ms |   2.345 ms |
|               SqlServer_InsertAppend | 1,965.34 ms |   2.126 ms |   1.884 ms |
|       SqlServer_SqlBulkCopy_FromList |    30.06 ms |   0.228 ms |   0.202 ms |
|  SqlServer_SqlBulkCopy_FromDataTable |    27.53 ms |   0.332 ms |   0.310 ms |
|                  MySql_InsertForEach | 5,967.03 ms | 116.450 ms | 114.369 ms |
|                   MySql_InsertAppend |   149.48 ms |   2.252 ms |   1.758 ms |
|           MySql_SqlBulkCopy_FromList |    84.15 ms |   1.662 ms |   3.358 ms |
|      MySql_SqlBulkCopy_FromDataTable |    82.42 ms |   1.648 ms |   3.476 ms |
|             PostgreSql_InsertForEach | 5,357.35 ms | 106.063 ms | 155.466 ms |
|              PostgreSql_InsertAppend |    72.69 ms |   0.354 ms |   0.296 ms |
|      PostgreSql_SqlBulkCopy_FromList |    31.36 ms |   0.623 ms |   0.718 ms |
| PostgreSql_SqlBulkCopy_FromDataTable |    31.37 ms |   0.619 ms |   0.737 ms |
---
totalCount=100000
|                               Method |        Mean |       Error |      StdDev |
|------------------------------------- |------------:|------------:|------------:|
|              SqlServer_InsertForEach | 12,633.6 ms |   216.57 ms |   202.58 ms |
|               SqlServer_InsertAppend | 19,484.5 ms |    35.34 ms |    29.51 ms |
|       SqlServer_SqlBulkCopy_FromList |    263.1 ms |     5.13 ms |     6.67 ms |
|  SqlServer_SqlBulkCopy_FromDataTable |    239.3 ms |     4.53 ms |     5.56 ms |
|                  MySql_InsertForEach | 59,331.8 ms | 1,135.02 ms | 1,261.57 ms |
|                   MySql_InsertAppend |  1,227.6 ms |    23.60 ms |    25.25 ms |
|           MySql_SqlBulkCopy_FromList |    540.4 ms |    10.71 ms |    21.64 ms |
|      MySql_SqlBulkCopy_FromDataTable |    515.3 ms |    10.16 ms |    20.28 ms |
|             PostgreSql_InsertForEach | 53,797.8 ms |   740.99 ms |   693.12 ms |
|              PostgreSql_InsertAppend |    722.3 ms |     8.35 ms |     7.40 ms |
|      PostgreSql_SqlBulkCopy_FromList |    310.4 ms |     3.55 ms |     3.14 ms |
| PostgreSql_SqlBulkCopy_FromDataTable |    280.6 ms |     5.55 ms |     9.57 ms |
---

### 5.结论

* 注：*10W条数据的典型场景*

|项目|SqlServer|MySql|PostgreSql|
|-|-|-|-|
|for循环|for循环|for循环|for循环|
|最快|119ms(0.12s)|394ms(0.39s)|208ms(0.2s)|
|最慢|43793ms(43.79s)|61492ms(61.49s)|55238ms(55.23s)|
|差距|368倍|156倍|265倍|
|-|-|-|-|
|Benchmark|Benchmark|Benchmark|Benchmark|Benchmark|
|最快|239ms(0.24s)|515ms(0.51s)|280ms(0.28s)|
|最慢|19484ms(19.48s)|59331ms(59.33s)|53797ms(53.79s)|
|差距|81倍|115倍|192倍|

* **SqlBulkCopy方法能显著提高批量插入性能**
* **SqlServer的insert () values(),(),()....语句似乎没有优化**
* **SqlServer的整体表现比MySql和PostgreSql好一些(都是空数据库,表结构简单,应该还没有达到硬件限制)**
* **MySql和PostgreSql的insert () values(),(),()....语句性能不错,尤其是PostgreSql**
* **PostgreSql各项指标均优于MySql**

## <span id="使用时的注意事项">五.注意事项</span>

* 构建的DataTable要跟数据库表完全一致，包含自增列，排除NotMapped
* 构建DataColumn时列名要跟表一致，类型要传实际类型，不能不传或者传object
* MySql连接字符串需要加上AllowLoadLocalInfile=true且服务端设置local_infile=1(建议修改全局配置文件)
* PostgreSql的copy指令对表名大小写有特殊要求，建议建表和实体特性都使用小写
* PostgreSql的NpgsqlBinaryImporter.Write(,)类型要求和数据库一致，需要使用枚举NpgsqlDbType

---
作者：NewcatsHuang  
时间：2021-12-25  
完整代码：[https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/tests/SqlBulkCopyTest](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/tests/SqlBulkCopyTest)  
转载请注明出处，谢谢O(∩_∩)O
