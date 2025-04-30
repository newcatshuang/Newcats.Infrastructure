using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSRedis;

namespace RedisTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string input = "Hello123World456";
            string result = Regex.Replace(input, @"\d+", "");
            Console.WriteLine(result); // 输出: HelloWorld
            return;

            var redis = new CSRedisClient("192.168.1.230");

            var setKey = "CSREDIS:SETKEY";
            Console.WriteLine(redis.Exists(setKey));
            //if (!await redis.ExistsAsync(setKey))
            //{
            //    await redis.SAddAsync(setKey, "1", "2", "3");
            //    await redis.ExpireAsync(setKey, TimeSpan.FromMinutes(2));
            //}
            await redis.SAddAsync(setKey, "5");
            Console.WriteLine(redis.Exists(setKey));

            bool setExists1 = await redis.SIsMemberAsync(setKey, "1");
            bool setExists2 = await redis.SIsMemberAsync(setKey, "4");
            bool setExists3 = await redis.SIsMemberAsync(setKey, "5");
            var members = redis.SMembers(setKey);

            var bitmapKey = "CSREDIS:BITKEY";
            if (!redis.Exists(bitmapKey))
            {
                await redis.SetBitAsync(bitmapKey, 100000, true);
                await redis.SetBitAsync(bitmapKey, 100001, true);
                await redis.SetBitAsync(bitmapKey, 169048, true);
                await redis.ExpireAsync(bitmapKey, TimeSpan.FromMinutes(2));
            }

            bool bitExists1 = await redis.GetBitAsync(bitmapKey, 10);
            bool bitExists2 = await redis.GetBitAsync(bitmapKey, 11);
            bool bitExists3 = await redis.GetBitAsync(bitmapKey, 40);
        }
    }
}