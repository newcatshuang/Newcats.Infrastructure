/***************************************************************************
 *GUID: f99fc662-0e0a-45a2-bef5-4f39a7e61016
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-20 21:33:09
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkCopyTest
{
    [Table("NewcatsUserInfoTest")]
    internal class NewcatsUserInfoTest
    {
        [Key]
        public long Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; }
    }

    internal static class DataTableEx
    {
        const string TableName = "NewcatsUserInfoTest";

        internal static DataTable ToDataTable<T>(this List<T> list)
        {
            DataTable dt = new DataTable(TableName);
            if (list == null || !list.Any())
                return dt;
            List<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            for (int i = 0; i < props.Count; i++)
            {
                dt.Columns.Add(props[i].Name, GetCoreType(props[i].PropertyType));
            }

            foreach (T item in list)
            {
                var values = new object[props.Count];
                for (int i = 0; i < props.Count; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
            }
            return dt;
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        private static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        private static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}