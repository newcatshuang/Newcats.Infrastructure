﻿using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Newcats.Utils.Extensions
{
    /// <summary>
    /// 集合类型扩展方法
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// IEnumerable数据转为DataTable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="collection">数据源</param>
        /// <returns>DataTable数据</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection) where T : class, new()
        {
            var tb = new DataTable(typeof(T).Name);
            if (collection == null || !collection.Any())
                return tb;

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                _ = tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in collection)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                _ = tb.Rows.Add(values);
            }
            return tb;
        }

        /// <summary>
        /// DataTable数据转为List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns>List数据</returns>
        public static List<T> ToList<T>(this DataTable source) where T : class, new()
        {
            List<T> list = new();
            if (source == null || source.Rows.Count == 0)
                return list;

            //获得此模型的类型   
            _ = typeof(T);
            foreach (DataRow dr in source.Rows)
            {
                T t = new();
                //获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    string tempName = pi.Name;
                    if (source.Columns.Contains(tempName))
                    {
                        //判断此属性是否有Setter      
                        if (!pi.CanWrite)
                            continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                list.Add(t);
            }
            return list;
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        public static bool IsNullable(this Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
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
        /// 获取当前类型的<seealso cref="TypeCode"/>
        /// </summary>
        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Object;
            if (type == typeof(bool)) { return TypeCode.Boolean; }
            if (type == typeof(char)) { return TypeCode.Char; }
            if (type == typeof(sbyte)) { return TypeCode.SByte; }
            if (type == typeof(byte)) { return TypeCode.Byte; }
            if (type == typeof(short)) { return TypeCode.Int16; }
            if (type == typeof(ushort)) { return TypeCode.UInt16; }
            if (type == typeof(int)) { return TypeCode.Int32; }
            if (type == typeof(uint)) { return TypeCode.UInt32; }
            if (type == typeof(long)) { return TypeCode.Int64; }
            if (type == typeof(ulong)) { return TypeCode.UInt64; }
            if (type == typeof(float)) { return TypeCode.Single; }
            if (type == typeof(double)) { return TypeCode.Double; }
            if (type == typeof(decimal)) { return TypeCode.Decimal; }
            if (type == typeof(DateTime)) { return TypeCode.DateTime; }
            if (type == typeof(string)) { return TypeCode.String; }
            // ReSharper disable once TailRecursiveCall
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (type.GetTypeInfo().IsEnum) { return Enum.GetUnderlyingType(type).GetTypeCode(); }
            return TypeCode.Object;
        }

        /// <summary>
        /// 根据condition条件，判断是否加入当前list集合
        /// </summary>
        /// <typeparam name="T">集合的元素类型</typeparam>
        /// <param name="list">当前集合</param>
        /// <param name="item">要加入的元素</param>
        /// <param name="condition">是否加入集合</param>
        /// <returns>加入了元素的集合</returns>
        public static IEnumerable<T> AddIf<T>(this IEnumerable<T> list, T item, bool condition)
        {
            if (condition)
                return list.Append(item);
            return list;
        }

        /// <summary>
        /// 根据condition条件，判断是否加入当前list集合
        /// </summary>
        /// <typeparam name="T">集合的元素类型</typeparam>
        /// <param name="list">当前集合</param>
        /// <param name="collection">另一个集合</param>
        /// <param name="condition">是否加入集合</param>
        /// <returns>加入了元素的集合</returns>
        public static List<T> AddRangeIf<T>(this List<T> list, IEnumerable<T> collection, bool condition)
        {
            if (condition)
            {
                list.AddRange(collection);
                return list;
            }
            return list;
        }

        /// <summary>
        /// 元素不为null，则加入当前list集合
        /// </summary>
        /// <typeparam name="T">集合的元素类型</typeparam>
        /// <param name="list">当前集合</param>
        /// <param name="item">要加入的元素</param>
        /// <returns>加入了元素的集合</returns>
        public static IEnumerable<T> AddIfNotNull<T>(this IEnumerable<T> list, T item)
        {
            return list.AddIf(item, item != null);
        }

        /// <summary>
        /// 若另一个集合有元素，则加入当前list集合
        /// </summary>
        /// <typeparam name="T">集合的元素类型</typeparam>
        /// <param name="list">当前集合</param>
        /// <param name="collection">另一个集合</param>
        /// <returns>加入了元素的集合</returns>
        public static IEnumerable<T> AddRangeIfNotNullAndEmpty<T>(this List<T> list, IEnumerable<T> collection)
        {
            return list.AddRangeIf(collection, collection.HasValue());
        }

        /// <summary>
        /// 随机返回集合中的一个元素
        /// </summary>
        /// <typeparam name="T">集合的元素类型</typeparam>
        /// <param name="list">当前集合</param>
        /// <returns>随机元素</returns>
        public static T Random<T>(this IEnumerable<T> list)
        {
            int index = System.Random.Shared.Next(list.Count());
            return list.ElementAt(index);
        }

        #region WhereIf
        /// <summary>
        /// Filters a <see cref="IEnumerable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="source">Enumerable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the enumerable</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered enumerable based on <paramref name="condition"/></returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Filters a <see cref="IEnumerable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="source">Enumerable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the enumerable</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered enumerable based on <paramref name="condition"/></returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, bool condition)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return condition ? query.Where(predicate) : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, Expression<Func<T, bool>> predicate, bool condition) where TQueryable : IQueryable<T>
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return condition ? (TQueryable)query.Where(predicate) : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, Expression<Func<T, int, bool>> predicate, bool condition)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return condition ? query.Where(predicate) : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <param name="condition">A boolean value</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, Expression<Func<T, int, bool>> predicate, bool condition) where TQueryable : IQueryable<T>
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return condition ? (TQueryable)query.Where(predicate) : query;
        }
        #endregion

        /// <summary>
        /// Throws an System.ArgumentException if the collection is empty        
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate</typeparam>
        /// <param name="collection">The collection</param>
        /// <param name="message">A message that describes the error.</param>
        /// <exception cref="ArgumentException">collection is empty</exception>
        public static void ThrowIfEmpty<T>(this IEnumerable<T> collection, string message = "")
        {
            if (!collection.Any())
                throw new ArgumentException(message, nameof(collection));
        }

        /// <summary>
        /// Throws an System.ArgumentNullException if the collection is null or empty
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate</typeparam>
        /// <param name="collection">The collection</param>
        /// <param name="message">A message that describes the error.</param>
        /// <exception cref="ArgumentNullException">collection is null or empty</exception>
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> collection, string message = "")
        {
            if (collection == null || !collection.Any())
                throw new ArgumentNullException(nameof(collection), message);
        }

        /// <summary>
        /// Indicates whether the specified collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate</typeparam>
        /// <param name="collection">The collection</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Whether the current object is not null and the collection has element and the string is not empty(also not whitespace).
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <returns>true if has value</returns>
        public static bool HasValue<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }
    }
}