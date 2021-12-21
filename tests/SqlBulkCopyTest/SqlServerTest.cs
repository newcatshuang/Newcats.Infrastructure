/***************************************************************************
 *GUID: 74b29649-8f42-4089-b981-6a48a78af1a3
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-20 21:26:49
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using System.Diagnostics;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newcats.DataAccess.Core;

namespace SqlBulkCopyTest;

internal class SqlServerTest
{
    const string ConnectionString = "Data Source=.;Initial Catalog=NewcatsDB20170627;User ID=sa;Password=123456;TrustServerCertificate=True";
    const string TableName = "NewcatsUserInfoTest";
    const string SqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id,@Name,@CreateTime)";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    internal void Init()
    {
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            const string sql = @$"
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{TableName}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
begin
	drop table [dbo].[{TableName}];
end

CREATE TABLE [dbo].[{TableName}](
	[Id] [bigint] NOT NULL,
	[Name] [varchar](64) NULL,
	[CreateTime] [datetime] NULL
) ON [PRIMARY];";
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            conn.Execute(sql, null, commandType: System.Data.CommandType.Text);
        }
    }

    /// <summary>
    /// dapper执行foreach循环插入数据
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
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

    /// <summary>
    /// ADO.NET的foreach循环插入数据
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    internal long InsertForEachNative(List<NewcatsUserInfoTest> list)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int result = 0;
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            foreach (NewcatsUserInfoTest item in list)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string sqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id{result},@Name{result},@CreateTime{result})";
                    cmd.Connection = conn;
                    cmd.CommandText = sqlText;
                    cmd.Parameters.Add(new SqlParameter("@Id" + result.ToString(), item.Id));
                    cmd.Parameters.Add(new SqlParameter("@Name" + result.ToString(), item.Name));
                    cmd.Parameters.Add(new SqlParameter("@CreateTime" + result.ToString(), item.CreateTime));
                    result += cmd.ExecuteNonQuery();
                }
            }
        }
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// dapper直接传list参数
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    internal long InsertBulk(List<NewcatsUserInfoTest> list)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int result = 0;
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            result = conn.Execute(SqlText, list, commandType: System.Data.CommandType.Text);
        }
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// dapper拼接sql语句
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
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

    /// <summary>
    /// SqlBulkCopy插入
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    internal long SqlBulkCopy(List<NewcatsUserInfoTest> list)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int result = 0;
        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            using (SqlBulkCopy copy = new SqlBulkCopy(conn))
            {
                copy.DestinationTableName = TableName;
                copy.BatchSize = list.Count;
                copy.WriteToServer(RepositoryHelper.ToDataTable(list));
                result = copy.RowsCopied;
            }
        }
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}