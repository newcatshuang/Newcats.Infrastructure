/***************************************************************************
 *GUID: 6fe54fef-41ee-42a1-b2e2-5d0da0ba8fe9
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-20 21:26:58
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using System.Diagnostics;
using System.Text;
using Dapper;
using MySqlConnector;

namespace SqlBulkCopyTest
{
    internal class MySqlTest
    {
        const string ConnectionString = "server=192.168.13.129;port=3306;database=NewcatsMyDB;uid=root;pwd=mysql-server1-ubuntu;CharSet=utf8;AllowLoadLocalInfile=true";
        const string TableName = "NewcatsUserInfoTest";
        const string SqlText = $"INSERT INTO {TableName} (Id,Name,CreateTime) VALUES (@Id,@Name,@CreateTime)";

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                const string sql = @$"
drop table if exists {TableName};
CREATE TABLE {TableName}(
	Id bigint NOT NULL,
	Name varchar(64) NULL,
	CreateTime datetime NULL
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
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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

        /// <summary>
        /// dapper直接传list参数
        /// </summary>
        internal long InsertBulk(List<NewcatsUserInfoTest> list)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int result = 0;
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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

        /// <summary>
        /// SqlBulkCopy插入-FromDataTable
        /// </summary>
        internal long SqlBulkCopyFromDataTable(DataTable dt)
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
                var r = copy.WriteToServer(dt);
                result = r.RowsInserted;
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}