/***************************************************************************
 *GUID: 9c6ff16b-bcd2-4251-bfa9-4918f12ad3c3
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-20 21:28:12
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using System.Diagnostics;
using System.Text;
using Dapper;
using Newcats.DataAccess.PostgreSql;
using Npgsql;

namespace SqlBulkCopyTest
{
    internal class PostgreSqlTest
    {
        const string ConnectionString = "Host=192.168.13.131;Port=5432;Username=postgres;Password=mysql-server1-ubuntu;Database=NewcatsPgDB;Pooling=true;";
        const string TableName = "newcatsuserinfotest";
        //const string TableName = "NewcatsUserInfoTest";
        const string SqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id,@Name,@CreateTime)";

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                const string sql = @$"
drop table if exists {TableName};
CREATE TABLE {TableName}(
	Id bigint NOT NULL,
	Name varchar(64) NULL,
	CreateTime date NULL
);";
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                conn.Execute(sql, null, commandType: System.Data.CommandType.Text);
            }
        }

        /// <summary>
        /// dapper执行foreach循环插入数据
        /// </summary>
        internal long InsertForEach(List<NewcatsUserInfoTest> list)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int result = 0;
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
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
        internal long InsertForEachNative(List<NewcatsUserInfoTest> list)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int result = 0;
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                foreach (NewcatsUserInfoTest item in list)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        string sqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id{result},@Name{result},@CreateTime{result})";
                        cmd.Connection = conn;
                        cmd.CommandText = sqlText;
                        cmd.Parameters.Add(new NpgsqlParameter("@Id" + result.ToString(), item.Id));
                        cmd.Parameters.Add(new NpgsqlParameter("@Name" + result.ToString(), item.Name));
                        cmd.Parameters.Add(new NpgsqlParameter("@CreateTime" + result.ToString(), item.CreateTime));
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
                using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
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
        internal long SqlBulkCopy(List<NewcatsUserInfoTest> list)
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
                    copy.WriteToServer(list.ToDataTable());
                }
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}