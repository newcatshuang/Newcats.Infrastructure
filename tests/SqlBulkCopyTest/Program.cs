using System.Diagnostics;
using Newcats.Utils.Helpers;
using SqlBulkCopyTest;


const int TotalCount = 5000;
List<UserInfoTest> list = new List<UserInfoTest>();
for (int i = 0; i < TotalCount; i++)
{
    UserInfoTest u = new UserInfoTest()
    {
        Id = IdHelper.Create(),
        Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
        CreateTime = DateTime.Now
    };
    list.Add(u);
}

SqlServerTest sqlServer = new SqlServerTest();
Stopwatch sw = new Stopwatch();
sqlServer.InsertOne(new UserInfoTest
{
    Id = IdHelper.Create(),
    Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
    CreateTime = DateTime.Now
});

sw.Start();
sqlServer.InsertForEach(list);
sw.Stop();
var t1 = sw.ElapsedMilliseconds;

sw.Restart();
sqlServer.InsertBulk(list);
sw.Stop();
var t2 = sw.ElapsedMilliseconds;

sw.Restart();
sqlServer.InsertAppend(list);
sw.Stop();
var t3 = sw.ElapsedMilliseconds;

sw.Restart();
sqlServer.SqlBulkCopy(list);
sw.Stop();
var t4 = sw.ElapsedMilliseconds;

Console.WriteLine($"集合大小:{list.Count}\r\n\n1.InsertForEach方法耗时:{t1}ms\r\n\n2.InsertBulk方法耗时:{t2}ms\r\n\n3.InsertAppend方法耗时:{t3}\r\n\n4.SqlBulkCopy方法耗时:{t4}ms");