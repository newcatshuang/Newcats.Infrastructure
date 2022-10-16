/***************************************************************************
 *GUID: eb5c565f-ebec-44f4-a02d-b2b2f6599c64
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-01 21:33:31
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

using Newcats.Utils.Helpers;

namespace Newcats.Utils.Extensions
{
    /// <summary>
    /// Object类扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Throws an System.ArgumentNullException if the object is null
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="message">A message that describes the error.</param>
        /// <exception cref="ArgumentNullException">object is null</exception>
        public static void ThrowIfNull(this object obj, string message = "")
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), message);
        }

        /// <summary>
        /// Whether the current object is not null and the collection has element and the string is not empty(also not whitespace).
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>true if has value</returns>
        public static bool HasValue(this object obj)
        {
            return obj != null;
        }

        /// <summary>
        /// 深度克隆对象实例，支持 <c>简单类pojo</c>、<c>数组</c>、<c>List</c>、<c>dicitionary</c>、<c>ValueTuple&lt;></c>、<c>匿名类型</c> 等
        /// 
        /// 示例代码:
        /// <code>
        /// var list = new List&lt;Person>()
        /// {
        ///     new Person{ Id = 1, Name = "小明" },
        ///     new Person{ Id = 2, Name = "小刚" }
        /// }
        /// var newList = list.Clone(false);//因为数据格式简单,没有冗余、循环的引用,传入 false 将克隆缓存关掉以提升性能
        /// </code>
        /// 当实例内部有冗余、循环的引用时:
        /// <code>
        /// var root = new Node { Id = 1, Children = new List&lt;Node>() };
        /// var current = new Node { Id = 2,Parent=root };
        /// root.Children.Add(current);
        /// var newRoot = root.Clone(true);//因为数据之间有冗余、循环的引用, 传入 true 打开克隆缓存，引用关系将被一起克隆
        /// </code>
        /// </summary>
        /// <remarks>
        /// 注意：本方法优先使用 <see cref="ICloneable"/> 中的 <c>Clone</c> 方法
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="containsRepeatReference">是否考虑重复的引用,当要克隆的对象内部没有冗余的、循环的引用时,将此设为 <c>false</c> 可能提升一半性能</param>
        /// <param name="capacity">当 <c>containsRepeatReference</c> 设为true时,缓存字典的默认容量</param>
        /// <returns></returns>
        public static T Clone<T>(this T obj, bool containsRepeatReference = true, int capacity = 32)
        {
            return CloneHelper.Clone(obj, containsRepeatReference, capacity);
        }
    }
}