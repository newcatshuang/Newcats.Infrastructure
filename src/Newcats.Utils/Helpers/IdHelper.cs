using System.Text;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// Id生成器
    /// </summary>
    public static class IdHelper
    {
        /// <summary>
        /// 根据雪花算法，创建一个19位Id
        /// </summary>
        /// <returns>19位雪花Id</returns>
        public static long Create()
        {
            return SnowflakeIdGenerator.Current.Create();
        }

        ///// <summary>
        ///// 根据雪花算法，创建一个Id，返回string形式
        ///// </summary>
        ///// <param name="datePrefix">是否在当前雪花Id前面追加当前日期</param>
        ///// <returns>字符串形式Id</returns>
        //public static string Create(bool datePrefix = false)
        //{
        //    return datePrefix ? (DateTime.Now.Date.ToString("yyyyMMdd") + SnowflakeIdGenerator.Current.Create().ToString()) : SnowflakeIdGenerator.Current.Create().ToString();
        //}

        /// <summary>
        /// 生成8位唯一的随机字符串
        /// </summary>
        /// <param name="id">数字Id</param>
        /// <exception cref="ArgumentOutOfRangeException">传入的id值大于可转换的最大值</exception>
        /// <returns>8位随机字符串</returns>
        public static string Create(long id)
        {
            return StringIdConverter.Create(id);
        }

        /// <summary>
        /// 解析生成的8位唯一字符串到数字id
        /// </summary>
        /// <param name="key">通过 IdHelper.Create(long id) 方法生成的8位字符串</param>
        /// <returns>数字Id</returns>
        public static long Reverse(string key)
        {
            return StringIdConverter.Reverse(key);
        }
    }

    /// <summary>
    /// 雪花算法ID 生成器
    /// </summary>
    internal class SnowflakeIdGenerator
    {
        /// <summary>
        /// 雪花算法ID
        /// </summary>
        private readonly SnowflakeId _id = new(1, 1);

        /// <summary>
        /// 获取<see cref="SnowflakeIdGenerator"/>类型的实例
        /// </summary>
        public static SnowflakeIdGenerator Current { get; } = new SnowflakeIdGenerator();

        /// <summary>
        /// 创建ID
        /// </summary>
        /// <returns></returns>
        public long Create()
        {
            return _id.NextId();
        }
    }

    /// <summary>
    /// 雪花算法，代码出自：https://github.com/dunitian/snowflake-net/blob/master/Snowflake.Net.Core/IdWorker.cs
    /// </summary>
    internal class SnowflakeId
    {
        /// <summary>
        /// 基准时间
        /// </summary>
        private const long TWEPOCH = 696096000000L;

        /// <summary>
        /// 机器标识位数
        /// </summary>
        private const int WORKER_ID_BITS = 5;

        /// <summary>
        /// 数据标志位数
        /// </summary>
        private const int DATACENTER_ID_BITS = 5;

        /// <summary>
        /// 序列号标识位数
        /// </summary>
        private const int SEQUENCE_BITS = 12;

        /// <summary>
        /// 机器ID最大值
        /// </summary>
        private const long MAX_WORKER_ID = -1L ^ (-1L << WORKER_ID_BITS);

        /// <summary>
        /// 数据标志最大值
        /// </summary>
        private const long MAX_DATACENTER_ID = -1L ^ (-1L << DATACENTER_ID_BITS);

        /// <summary>
        /// 序列号ID最大值
        /// </summary>
        private const long SEQUENCE_MASK = -1L ^ (-1L << SEQUENCE_BITS);

        /// <summary>
        /// 机器ID偏左移12位
        /// </summary>
        private const int WORKER_ID_SHIFT = SEQUENCE_BITS;

        /// <summary>
        /// 数据ID偏左移17位
        /// </summary>
        private const int DATACENTER_ID_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS;

        /// <summary>
        /// 时间毫秒左移22位
        /// </summary>
        private const int TIMESTAMP_LEFT_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS + DATACENTER_ID_BITS;

        /// <summary>
        /// 最后时间戳
        /// </summary>
        private long _lastTimestamp = -1L;

        /// <summary>
        /// 机器ID
        /// </summary>
        public long WorkerId { get; protected set; }

        /// <summary>
        /// 数据标志ID
        /// </summary>
        public long DatacenterId { get; protected set; }

        /// <summary>
        /// 序列号ID
        /// </summary>
        public long Sequence { get; internal set; } = 0L;

        /// <summary>
        /// 初始化一个<see cref="SnowflakeId"/>类型的实例
        /// </summary>
        /// <param name="workerId">机器ID</param>
        /// <param name="datacenterId">数据标志ID</param>
        /// <param name="sequence">序列号ID</param>
        public SnowflakeId(long workerId, long datacenterId, long sequence = 0L)
        {
            // 如果超出范围就抛出异常
            if (workerId > MAX_WORKER_ID || workerId < 0)
            {
                throw new ArgumentException($"worker Id 必须大于0，且不能大于 MaxWorkerId：{MAX_WORKER_ID}");
            }

            if (datacenterId > MAX_DATACENTER_ID || datacenterId < 0)
            {
                throw new ArgumentException($"datacenter Id 必须大于0，且不能大于 MaxDatacenterId：{MAX_DATACENTER_ID}");
            }

            // 先校验再赋值
            WorkerId = workerId;
            DatacenterId = datacenterId;
            Sequence = sequence;
        }

        /// <summary>
        /// 对象锁
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 获取下一个ID
        /// </summary>
        /// <returns></returns>
        public long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception($"时间戳必须大于上一次生成ID的时间戳，拒绝为{_lastTimestamp - timestamp}毫秒生成id");
                }

                // 如果上次生成时间和当前时间相同，在同一毫秒内
                if (_lastTimestamp == timestamp)
                {
                    // sequence自增，和sequenceMask相与一下，去掉高位
                    Sequence = (Sequence + 1) & SEQUENCE_MASK;
                    //判断是否溢出,也就是每毫秒内超过1024，当为1024时，与sequenceMask相与，sequence就等于0
                    if (Sequence == 0)
                    {
                        //等待到下一毫秒
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    //如果和上次生成时间不同,重置sequence，就是下一毫秒开始，sequence计数重新从0开始累加,
                    //为了保证尾数随机性更大一些,最后一位可以设置一个随机数
                    //_sequence = 0;//new Random().Next(10);
                    Sequence = Random.Shared.Next(10);
                }

                _lastTimestamp = timestamp;
                return ((timestamp - TWEPOCH) << TIMESTAMP_LEFT_SHIFT) | (DatacenterId << DATACENTER_ID_SHIFT) | (WorkerId << WORKER_ID_SHIFT) | Sequence;
            }
        }

        /// <summary>
        /// 获取增量时间戳，防止产生的时间比之前的时间还要小（由于NTP回拨等问题），保持增量的趋势
        /// </summary>
        /// <param name="lastTimestamp">最后一个时间戳</param>
        /// <returns></returns>
        protected long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }

            return timestamp;
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        protected long TimeGen()
        {
            return CurrentTimeMills();
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        public static Func<long> CurrentTimeFunc = InternalCurrentTimeMillis;

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMills()
        {
            return CurrentTimeFunc();
        }

        /// <summary>
        /// 重置当前时间戳
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IDisposable StubCurrentTime(Func<long> func)
        {
            CurrentTimeFunc = func;
            return new DisposableAction(() => { CurrentTimeFunc = InternalCurrentTimeMillis; });
        }

        /// <summary>
        /// 重置当前时间戳
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>
        public static IDisposable StubCurrentTime(long millis)
        {
            CurrentTimeFunc = () => millis;
            return new DisposableAction(() => { CurrentTimeFunc = InternalCurrentTimeMillis; });
        }

        private static readonly DateTime Jan1St1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 默认当前时间戳
        /// </summary>
        /// <returns></returns>
        private static long InternalCurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }

        /// <summary>
        /// 一次性方法
        /// </summary>
        public class DisposableAction : IDisposable
        {
            /// <summary>
            /// 执行方法
            /// </summary>
            private readonly Action _action;

            /// <summary>
            /// 初始化一个<see cref="DisposableAction"/>类型的实例
            /// </summary>
            /// <param name="action">执行方法</param>
            public DisposableAction(Action action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                _action();
            }
        }
    }

    /// <summary>
    /// 数字Id和8位随机字符串转换器
    /// </summary>
    internal class StringIdConverter
    {
        //参考
        //https://www.cnblogs.com/xmlnode/p/4544302.html
        //https://github.com/Bryan-Cyf/SuperShortLink/blob/55d57d8678e48129b0aeb9e437d44cb9710023ac/src/SuperShortLink.Core/Service/Base62Converter.cs

        private const string _62Seq = "KL01GptV82QYdAX7ujWi9fsIcHDla4TvUJNqPnROhgF3kEmxbe6SZr5BoMywCz";//62位序列
        private const int _codeLength = 8;//随机字符串长度

        /// <summary>
        /// 补充0的长度
        /// </summary>
        private static int ZeroLength
        {
            get
            {
                return MaxValue.ToString().Length;
            }
        }

        /// <summary>
        /// Code长度位数下能达到的最大值
        /// </summary>
        private static long MaxValue
        {
            get
            {
                var max = (long)Math.Pow(62, _codeLength) - 1;
                return (long)Math.Pow(10, max.ToString().Length - 1) - 1;
            }
        }

        /// <summary>
        /// 生成8位随机字符串
        /// </summary>
        /// <param name="id">数字id</param>
        /// <returns>8位随机字符串</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的id值大于可转换的最大值</exception>
        internal static string Create(long id)
        {
            // 1、根据自增主键id前面补0，如：00000123
            // 2、倒转32100000
            // 3、把倒转后的十进制转六十二进制（乱序后）

            if (id > MaxValue)
            {
                throw new ArgumentOutOfRangeException($"The converted value cannot exceed the maximum value {MaxValue}");
            }

            var idChars = id.ToString()
                   .PadLeft(ZeroLength, '0')
                   .ToCharArray()
                   .Reverse();

            var confuseId = long.Parse(string.Join("", idChars));
            var base62Str = Convert10To62(confuseId);
            return base62Str.PadLeft(_codeLength, _62Seq.First());
        }

        /// <summary>
        /// 解析到数字id
        /// </summary>
        /// <param name="key">通过 StringIdConverter.Create() 方法生成的字符串</param>
        /// <returns>数字id</returns>
        internal static long Reverse(string key)
        {
            // 1、六十二进制转十进制，得到如：32100000
            // 2、倒转00000123，得到123

            if (key.Length != _codeLength)
            {
                return 0;
            }

            var confuseId = Convert62To10(key);
            var idChars = confuseId.ToString()
                .PadLeft(ZeroLength, '0')
                .ToCharArray()
                .Reverse();

            var id = long.Parse(string.Join("", idChars));
            id = id > MaxValue ? 0 : id;
            return id;
        }

        /// <summary>
        /// 十进制 -> 62进制
        /// </summary>
        private static string Convert10To62(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("value", "value must be greater or equal to zero");

            var sb = new StringBuilder();
            do
            {
                sb.Insert(0, _62Seq[(int)(value % 62)]);
                value /= 62;
            } while (value > 0);

            return sb.ToString();
        }

        /// <summary>
        /// 62进制 -> 10进制
        /// </summary>
        private static long Convert62To10(string value)
        {
            long result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                int power = value.Length - i - 1;
                int digit = _62Seq.IndexOf(value[i]);
                if (digit < 0)
                    throw new ArgumentException("Invalid character in base62 string", "value");
                result += digit * (long)Math.Pow(62, power);
            }

            return result;
        }

        /// <summary>
        /// 生成随机的0-9a-zA-Z的62位字符串
        /// </summary>
        /// <returns>62位字符串</returns>
        private static string Generate62Sequence()
        {
            string[] Chars = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z".Split(',');
            int SeekSeek = unchecked((int)DateTime.Now.Ticks);
            Random SeekRand = new(SeekSeek);
            for (int i = 0; i < 100000; i++)
            {
                int r = SeekRand.Next(1, Chars.Length);
                string f = Chars[0];
                Chars[0] = Chars[r - 1];
                Chars[r - 1] = f;
            }
            return string.Join("", Chars);
        }
    }
}