/***************************************************************************
 *GUID: eb5c565f-ebec-44f4-a02d-b2b2f6599c64
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-01 21:33:31
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

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
    }
}