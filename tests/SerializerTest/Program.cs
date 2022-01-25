using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using MessagePack;
using SerializerTest;

class Program
{
    static void Main(string[] args)
    {
        Employee emp = new Employee
        {
            Id = 1,
            Email = "newcats@live.com",
            Phone = "11111111111",
            Password = "passwordpasswordpasswordpasswordpasswordpasswordpasswordpassword",
            Name = "Newcats",
            ChineseName = "秦始皇",
            Gender = GenderEnum.Man,
            JoinTime = DateTime.Now,
            JoinIP = "127.0.0.1",
            LoginNum = 1,
            TodayErrorCount = 1,
            TodayErrorTime = DateTime.Now,
            LastLoginTime = DateTime.Now,
            LastLoginIP = "127.0.0.1",
            Disabled = false,
            DisabledTime = DateTime.Now,
            Salt = "asdfasdfagerehfnsth r",
            CreateId = 1,
            CreateName = "bbbbbbbbbbbbb",
            UpdateId = 1,
            UpdateName = "ccccccccccccccccccc",
            UpdateTime = DateTime.Now
        };

        List<Employee> list = new List<Employee>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(emp);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        JsonSerializerOptions opt = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
        };
        var stj = System.Text.Json.JsonSerializer.Serialize(list, opt);
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);

        sw.Restart();
        var nj = Newtonsoft.Json.JsonConvert.SerializeObject(list);
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);

        sw.Restart();
        var mp = MessagePack.MessagePackSerializer.Serialize(list);



        //var mpj = MessagePackSerializer.ConvertToJson(mp);

        //Employee e = MessagePackSerializer.Deserialize<Employee>(mp);
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);

        //return;
        Summary summary = BenchmarkRunner.Run<SerializerBenchmark>();
        Console.ReadLine();
    }
}

/// <summary>
/// 结论：
/// 1.序列化1个对象，性能由高到低：MessagePack>NewtownJson>SystemJson
/// 2.序列化10个对象，性能由高到低：MessagePack>NewtownJson>SystemJson
/// 3.序列化100个对象，性能由高到低：MessagePack>SystemJson>NewtownJson
/// 4.序列化1000个对象，性能由高到低：MessagePack>SystemJson>NewtownJson
/// 5.序列化10000个对象，性能由高到低：MessagePack>SystemJson>NewtownJson（此时开始，mp和sj的差距不大）
/// 6.序列化100000个对象，性能由高到低：MessagePack>SystemJson>NewtownJson
/// 7.序列化1000000个对象，性能由高到低：MessagePack>SystemJson>NewtownJson（1.08s-1.85s-4.329s）
/// 
/// 
/// 8.先序列化，再反序列化1000000个对象，性能由高到低：MessagePack>SystemJson>NewtownJson（3.254s-6.28s-13.325s）
/// </summary>
public class SerializerBenchmark
{
    int totalCount = 1000000;

    private List<Employee> CreateList()
    {
        Employee emp = new Employee
        {
            Id = 1,
            Email = "newcats@live.com",
            Phone = "11111111111",
            Password = "passwordpasswordpasswordpasswordpasswordpasswordpasswordpassword",
            Name = "Newcats",
            ChineseName = "秦始皇",
            Gender = GenderEnum.Man,
            JoinTime = DateTime.Now,
            JoinIP = "127.0.0.1",
            LoginNum = 1,
            TodayErrorCount = 1,
            TodayErrorTime = DateTime.Now,
            LastLoginTime = DateTime.Now,
            LastLoginIP = "127.0.0.1",
            Disabled = false,
            DisabledTime = DateTime.Now,
            Salt = "asdfasdfagerehfnsth r",
            CreateId = 1,
            CreateName = "bbbbbbbbbbbbb",
            UpdateId = 1,
            UpdateName = "ccccccccccccccccccc",
            UpdateTime = DateTime.Now
        };

        List<Employee> list = new List<Employee>();
        for (int i = 0; i < totalCount; i++)
        {
            list.Add(emp);
        }

        return list;
    }

    [Benchmark]
    public void SystemTextJson()
    {
        List<Employee> list = CreateList();
        JsonSerializerOptions opt = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)//不转义编码字符集(可以输出中文)
        };
        //var byteRes = JsonSerializer.SerializeToUtf8Bytes(list, opt);
        var stj = System.Text.Json.JsonSerializer.Serialize(list);
        var r = System.Text.Json.JsonSerializer.Deserialize<List<Employee>>(stj);
    }

    [Benchmark]
    public void NewtonsoftJson()
    {
        List<Employee> list = CreateList();
        var nj = Newtonsoft.Json.JsonConvert.SerializeObject(list);
        var r = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employee>>(nj);
    }



    [Benchmark]
    public void MessagePackSerializer()
    {
        List<Employee> list = CreateList();
        var mp = MessagePack.MessagePackSerializer.Serialize(list);

        var r = MessagePack.MessagePackSerializer.Deserialize<List<Employee>>(mp);
    }
}