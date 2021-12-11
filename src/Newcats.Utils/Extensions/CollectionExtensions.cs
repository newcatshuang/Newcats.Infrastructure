using System.Data;
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
        /// IEnumerable<T>数据转为DataTable
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
                tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in collection)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                tb.Rows.Add(values);
            }
            return tb;
        }

        /// <summary>
        /// DataTable数据转为List<T>
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns>List<T>数据</returns>
        public static List<T> ToList<T>(this DataTable source) where T : class, new()
        {
            List<T> list = new List<T>();
            if (source == null || source.Rows.Count == 0)
                return list;

            //获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in source.Rows)
            {
                T t = new T();
                //获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  //检查DataTable是否包含此列    
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
        private static bool IsNullable(Type t)
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
            else
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
        /// <exception cref="ArgumentException">collection is empty</exception>
        public static void ThrowIfEmpty<T>(this IEnumerable<T> collection)
        {
            if (!collection.Any())
                throw new ArgumentException("The collection is empty.", nameof(collection));
        }

        /// <summary>
        /// Throws an System.ArgumentNullException if the collection is null or empty
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate</typeparam>
        /// <param name="collection">The collection</param>
        /// <exception cref="ArgumentNullException">collection is null or empty</exception>
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null || !collection.Any())
                throw new ArgumentNullException(nameof(collection), "The collection is null or empty.");
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
    }
}