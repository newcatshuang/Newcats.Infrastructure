﻿using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 仓储帮助类，对实体等进行缓存操作
    /// </summary>
    public class RepositoryHelper
    {
        #region 静态字典缓存及私有方法
        /// <summary>
        /// 实体类对应的insert语句字典，键为实体类全名（命名空间+类名+插入？批量插入？）
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _sqlInsertDic = new();

        /// <summary>
        /// 实体类对应的数据库表名字典，键为实体类全名（命名空间+类名）
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _tableNameDic = new();

        /// <summary>
        /// 实体类包含的所有有效字段的字典，键为实体类全名（命名空间+类名+插入?查询?）
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _tableFieldsDic = new();

        /// <summary>
        /// 实体类对应的数据库表的主键名，键为实体类全名（命名空间+类名）
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _tablePrimaryKeyDic = new();

        /// <summary>
        /// 获取实体类的表名
        /// 获取优先级为：
        /// 1.获取TableAttribute特性的表名
        /// 2.若类名以Entity结尾，则获取类名（去除Entity名称）
        /// 3.前面都不满足，则直接获取类名
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>数据库表名</returns>
        public static string GetTableName(Type type)
        {
            string key = type.FullName;
            string tableName = string.Empty;
            if (_tableNameDic.TryGetValue(key, out tableName))
            {
                if (!string.IsNullOrWhiteSpace(tableName))
                    return tableName;
            }

            var attrs = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                foreach (var item in attrs)
                {
                    if (item is TableAttribute tabAttr)
                    {
                        tableName = string.IsNullOrWhiteSpace(tabAttr.Schema) ? tabAttr.Name : $"{tabAttr.Schema}.{tabAttr.Name}";
                        break;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(tableName))//1.判断TableAttribute特性
            {
                _tableNameDic.TryAdd(key, tableName);
                return tableName;
            }
            tableName = type.Name;
            if (tableName.EndsWith("Entity", StringComparison.OrdinalIgnoreCase))//2.以Entity结尾的实体类型
            {
                tableName = tableName.Substring(0, tableName.Length - 6);
                _tableNameDic.TryAdd(key, tableName);
                return tableName;
            }
            _tableNameDic.TryAdd(key, tableName);
            return tableName;//3.直接返回类名
        }

        /// <summary>
        /// 获取实体表的所有字段（insert语句时使用）(实体类的属性应与表的字段完全一致)（表连接查询时的查询字段才会判断ColumnAttribute特性）
        /// 1.排除NotMappedAttribute特性的字段
        /// 2.排除DatabaseGeneratedAttribute特性的字段（自增字段）
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>数据库表字段(逗号(,)分割)</returns>
        public static string GetTableFieldsInsert(Type type)
        {
            string key = $"{type.FullName}_InsertFields";
            string fields = string.Empty;
            if (_tableFieldsDic.TryGetValue(key, out fields))
            {
                if (!string.IsNullOrWhiteSpace(fields))
                    return fields;
            }
            var pros = type.GetProperties();
            if (pros == null || pros.Length == 0)
                throw new ArgumentException("No Fields found in this Entity", nameof(type));
            StringBuilder sb = new();
            foreach (var pro in pros)
            {
                //1.排除NotMappedAttribute特性的字段
                var attrsNot = pro.GetCustomAttributes(typeof(NotMappedAttribute), false);
                if (attrsNot != null && attrsNot.Length > 0)
                {
                    continue;
                }

                //2.插入时，排除自增类型字段
                var attrAuto = pro.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), false);
                if (attrAuto != null && attrAuto.Length > 0)
                {
                    continue;
                }

                //获取实际字段
                var attrReal = pro.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (attrReal != null && attrReal.Length > 0)
                {
                    sb.AppendFormat("{0}", ((ColumnAttribute)attrReal[0]).Name);
                }
                else
                {
                    sb.AppendFormat("{0},", pro.Name);
                }
            }
            fields = sb.ToString().TrimEnd(',');
            _tableFieldsDic.TryAdd(key, fields);
            return fields;
        }

        /// <summary>
        /// 获取实体表的所有字段（insert语句时使用）(使用ColumnAttribute时，插入的变量名应为属性名，而不是ColumnAttribute.Name)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetTableFieldsInsertParameter(Type type)
        {
            string key = $"{type.FullName}_InsertFieldsParameter";
            string fields = string.Empty;
            if (_tableFieldsDic.TryGetValue(key, out fields))
            {
                if (!string.IsNullOrWhiteSpace(fields))
                    return fields;
            }
            var pros = type.GetProperties();
            if (pros == null || pros.Length == 0)
                throw new ArgumentException("No Fields found in this Entity", nameof(type));
            StringBuilder sb = new();
            foreach (var pro in pros)
            {
                //1.排除NotMappedAttribute特性的字段
                var attrsNot = pro.GetCustomAttributes(typeof(NotMappedAttribute), false);
                if (attrsNot != null && attrsNot.Length > 0)
                {
                    continue;
                }

                //2.插入时，排除自增类型字段
                var attrAuto = pro.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), false);
                if (attrAuto != null && attrAuto.Length > 0)
                {
                    continue;
                }
                sb.AppendFormat("{0},", pro.Name);
            }
            fields = sb.ToString().TrimEnd(',');
            _tableFieldsDic.TryAdd(key, fields);
            return fields;
        }

        /// <summary>
        /// 获取实体表的所有字段（select语句时使用）(单表实体类的属性应与表的字段完全一致)（表连接查询时的查询字段会判断RealColumn特性）
        /// 1.排除NotMappedAttribute特性的字段
        /// 2.表连接查询时请为每个属性使用ColumnAttribute特性，否则会直接使用属性名
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>数据库表字段(逗号(,)分割)</returns>
        public static string GetTableFieldsQuery(Type type)
        {
            string key = $"{type.FullName}_QueryFields";
            string fields = string.Empty;
            if (_tableFieldsDic.TryGetValue(key, out fields))
            {
                if (!string.IsNullOrWhiteSpace(fields))
                    return fields;
            }
            var pros = type.GetProperties();
            if (pros == null || pros.Length == 0)
                throw new ArgumentException("No Fields found in this Entity", nameof(type));
            StringBuilder sb = new();
            foreach (var pro in pros)
            {
                //1.排除NotMappedAttribute特性的字段
                var attrsNot = pro.GetCustomAttributes(typeof(NotMappedAttribute), false);
                if (attrsNot != null && attrsNot.Length > 0)
                {
                    continue;
                }
                //3.判断表连接时候的实际字段
                var attrReal = pro.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (attrReal != null && attrReal.Length > 0)
                {
                    sb.AppendFormat("{0} as {1},", ((ColumnAttribute)attrReal[0]).Name, pro.Name);//表连接查询时候的字段
                }
                else
                {
                    sb.AppendFormat("{0},", pro.Name);
                }
            }
            fields = sb.ToString().TrimEnd(',');
            _tableFieldsDic.TryAdd(key, fields);
            return fields;
        }

        /// <summary>
        /// 获取实体表的主键
        /// 获取优先级为：
        /// 1.获取Key特性的字段
        /// 2.获取字段名为id的字段（忽略大小写）
        /// 3.获取字段为以id结尾的字段（忽略大小写）
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>数据库表的主键</returns>
        public static string GetTablePrimaryKey(Type type)
        {
            string key = type.FullName;
            string pkName = string.Empty;
            if (_tablePrimaryKeyDic.TryGetValue(key, out pkName))
            {
                if (!string.IsNullOrWhiteSpace(pkName))
                    return pkName;
            }

            var pros = type.GetProperties();
            if (pros == null || pros.Length == 0)
                throw new ArgumentException("No Fields found in this Entity", nameof(type));
            //1.获取Key特性的字段
            foreach (var p in pros)
            {
                var pkAttrs = p.GetCustomAttributes(typeof(KeyAttribute), false);
                if (pkAttrs != null && pkAttrs.Length > 0)
                {
                    var realAttr = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                    if (realAttr != null && realAttr.Length > 0)
                        pkName = ((ColumnAttribute)realAttr[0]).Name;
                    else
                        pkName = p.Name;
                    _tablePrimaryKeyDic.TryAdd(key, pkName);
                    return pkName;
                }
            }
            //2.获取字段名为id的字段（忽略大小写）
            foreach (var p in pros)
            {
                if (p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    var realAttr = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                    if (realAttr != null && realAttr.Length > 0)
                        pkName = ((ColumnAttribute)realAttr[0]).Name;
                    else
                        pkName = p.Name;
                    _tablePrimaryKeyDic.TryAdd(key, pkName);
                    return pkName;
                }
            }
            //3.获取字段为以id结尾的字段（忽略大小写）
            foreach (var p in pros)
            {
                if (p.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    var realAttr = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                    if (realAttr != null && realAttr.Length > 0)
                        pkName = ((ColumnAttribute)realAttr[0]).Name;
                    else
                        pkName = p.Name;
                    _tablePrimaryKeyDic.TryAdd(key, pkName);
                    return pkName;
                }
            }
            if (string.IsNullOrWhiteSpace(pkName))
                throw new ArgumentException("No PrimaryKey found in this Entity", nameof(type));
            return pkName;
        }

        /// <summary>
        /// 获取单个实体的insert语句
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>insert语句</returns>
        public static string GetInsertSqlText(Type type)
        {
            string key = $"{type.FullName}_Insert";
            string sqlText = string.Empty;
            if (_sqlInsertDic.TryGetValue(key, out sqlText))
            {
                if (!string.IsNullOrWhiteSpace(sqlText))
                    return sqlText;
            }

            string fields = GetTableFieldsInsert(type);
            string fieldsParameter = GetTableFieldsInsertParameter(type);
            string tableName = GetTableName(type);
            string[] fieldArray = fieldsParameter.Split(',');
            for (int i = 0; i < fieldArray.Length; i++)
            {
                fieldArray[i] = $"@{fieldArray[i]}";
            }
            sqlText = $" INSERT INTO {tableName} ({fields}) VALUES ({string.Join(",", fieldArray)});";
            _sqlInsertDic.TryAdd(key, sqlText);
            return sqlText;
        }
        #endregion

        #region IEnumerable转DataTable
        /// <summary>
        /// IEnumerable<T>数据转为DataTable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="collection">数据源</param>
        /// <returns>DataTable数据</returns>
        public static DataTable ToDataTable<T>(IEnumerable<T> collection) where T : class
        {
            var tb = new DataTable(typeof(T).Name);
            if (collection == null || !collection.Any())
                return tb;
            List<PropertyInfo> props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            for (int i = 0; i < props.Count; i++)
            {
                var attrsNot = props[i].GetCustomAttributes(typeof(NotMappedAttribute), false);
                if (attrsNot != null && attrsNot.Length > 0)
                {
                    props.Remove(props[i]);
                    i--;
                    continue;
                }
                var attrReal = props[i].GetCustomAttributes(typeof(ColumnAttribute), false);
                if (attrReal != null && attrReal.Length > 0)
                {
                    tb.Columns.Add(((ColumnAttribute)attrReal[0]).Name, GetCoreType(props[i].PropertyType));
                }
                else
                {
                    tb.Columns.Add(props[i].Name, GetCoreType(props[i].PropertyType));
                }
            }

            foreach (T item in collection)
            {
                var values = new object[props.Count];
                for (int i = 0; i < props.Count; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                tb.Rows.Add(values);
            }
            return tb;
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
        #endregion
    }
}