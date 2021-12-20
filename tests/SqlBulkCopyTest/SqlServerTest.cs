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
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newcats.DataAccess.Core;
using System.Linq;

namespace SqlBulkCopyTest
{
    internal class SqlServerTest
    {
        const string ConnectionString = "Data Source=.;Initial Catalog=NewcatsDB20170627;User ID=sa;Password=123456;TrustServerCertificate=True";
        const string TableName = "UserInfoTest";
        const string SqlText = "INSERT INTO UserInfoTest (Id,Name,CreateTime) VALUES (@Id,@Name,@CreateTime)";

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal long InsertOne(UserInfoTest model)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                result += conn.Execute(SqlText, model, commandType: System.Data.CommandType.Text);
            }
            return result;
        }

        /// <summary>
        /// foreach循环插入数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal long InsertForEach(List<UserInfoTest> list)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                foreach (UserInfoTest test in list)
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    result += conn.Execute(SqlText, test, commandType: System.Data.CommandType.Text);
                }
            }
            return result;
        }

        /// <summary>
        /// dapper直接传list参数
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal long InsertBulk(List<UserInfoTest> list)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                result = conn.Execute(SqlText, list, commandType: System.Data.CommandType.Text);
            }
            return result;
        }

        /// <summary>
        /// 拼接sql语句
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal long InsertAppend(List<UserInfoTest> list)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                const int perCount = 1000;
                int times = Convert.ToInt32(Math.Ceiling(list.Count * 1.0 / perCount));
                for (int i = 0; i < times; i++)
                {
                    StringBuilder sb = new StringBuilder("INSERT INTO UserInfoTest (Id,Name,CreateTime) VALUES");
                    var perList = list.Skip(i * perCount).Take(perCount);
                    foreach (UserInfoTest test in perList)
                    {
                        sb.Append("(@Id,@Name,@CreateTime),");
                    }
                    var s = sb.ToString().TrimEnd(',');
                    result += conn.Execute(sb.ToString().TrimEnd(','), perList, commandType: System.Data.CommandType.Text);
                    result += conn.Execute(s, perList, commandType: System.Data.CommandType.Text);
                }
            }
            return result;
        }

        /// <summary>
        /// SqlBulkCopy插入
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal long SqlBulkCopy(List<UserInfoTest> list)
        {
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
            return (long)result;
        }
    }
}