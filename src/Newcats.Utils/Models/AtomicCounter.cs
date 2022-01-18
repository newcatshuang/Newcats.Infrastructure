/***************************************************************************
 *GUID: 85f5286c-1800-4306-86f6-8a8b8d7db3ce
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-18 11:55:48
 *Author: NewcatsHuang
*****************************************************************************/

namespace Newcats.Utils.Models
{
    /// <summary>
    /// 原子计数器
    /// </summary>
    public sealed class AtomicCounter
    {
        private int _value;

        /// <summary>
        /// Gets the current value of the counter.
        /// </summary>
        public int Value
        {
            get => Volatile.Read(ref _value);
            set => Volatile.Write(ref _value, value);
        }

        /// <summary>
        /// Atomically increments the counter value by 1.
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically decrements the counter value by 1.
        /// </summary>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// Atomically resets the counter value to 0.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }
}