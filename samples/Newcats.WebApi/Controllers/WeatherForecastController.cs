using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newcats.AspNetCore.Filters;
using Newcats.DataAccess;
using Newcats.DataAccess.Core;
using Newcats.Utils.Helpers;
//using Newcats.DataAccess.MySql;

namespace Newcats.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        //private readonly DataAccess.SqlServer.IRepository<DataAccess.SqlServer.DbContextBase> _repository;
        //private readonly DataAccess.SqlServer.IRepository<TwoDbContext> _user;

        //private readonly DataAccess.MySql.IRepository<MySqlDbContext> _repository;

        private readonly DataAccess.PostgreSql.IRepository<PgContext> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(DataAccess.PostgreSql.IRepository<PgContext> repository)
        {
            _repository = repository;
        }

        //public WeatherForecastController(DataAccess.MySql.IRepository<MySqlDbContext> repository)
        //{
        //    _repository = repository;
        //}

        //public WeatherForecastController(DataAccess.SqlServer.IRepository<DataAccess.SqlServer.DbContextBase> repository, DataAccess.SqlServer.IRepository<TwoDbContext> user)
        //{
        //    _repository = repository;
        //    _user = user;
        //}

        //SqlServer测试
        //[HttpGet]
        //public async Task<string> Get()
        //{
        //    var r1 = _repository.GetTop<UserInfo>(10);
        //    _repository.Insert(new UserInfo { JoinTime = DateTime.Now, Name = "Newcats", UserId = 111 });

        //    var r2 = await _repository.GetTopAsync<UserInfo>(10);
        //    await _repository.InsertAsync<UserInfo>(new UserInfo { JoinTime = DateTime.Now, UserId = 222, Name = "huang" });

        //    var r3 = _user.GetTop<User>(10);
        //    var r4 = await _user.GetTopAsync<User>(10);

        //    _user.Insert<User>(new Controllers.User { Id = 111, Phone = "11111" });
        //    await _user.InsertAsync<User>(new Controllers.User { Id = 222, Phone = "222" });



        //    return "ok";
        //}

        public async Task<string> Index()
        {
            //1.插入数据，返回主键
            object r1 = _repository.Insert<UserInfo>(new UserInfo { Name = "Newcats", CreateTime = DateTime.Now });

            //2.插入数据，返回是否成功
            bool r2 = _repository.Insert<UserInfo>(new UserInfo { Id = 1, Name = "Huang", CreateTime = DateTime.UtcNow }, null);

            //3.批量插入，返回成功的条数
            //int r3 = _repository.InsertBulk<UserInfo>(new List<UserInfo>() { new UserInfo { Name = "Newcats", CreateTime = DateTime.Now } }, transaction, 600);

            //4.使用SqlBulkCopy批量插入数据
            //int r4 = _repository.InsertSqlBulkCopy<UserInfo>(new List<UserInfo>() { new UserInfo { Name = "Newcats", CreateTime = DateTime.Now } }, transaction, 600);

            //5.根据主键删除一条数据(delete from userinfo where id=1;)
            int r5 = _repository.Delete<UserInfo>(1);

            //6.根据给定的条件，删除记录(删除CreateTime>=2021-12-12的记录)(delete from userinfo where createtime>='2021-12-12';)
            int r6 = _repository.Delete<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.CreateTime, new DateTime(2021, 12, 12), OperateTypeEnum.GreaterEqual, LogicTypeEnum.And) });

            //7.根据主键，更新一条记录(update userinfo set Name='NewcatsHuang' where id=2;)
            //int r7 = _repository.Update<UserInfo>(2, new List<DbUpdate<UserInfo>>() { new DbUpdate<UserInfo>(s => s.Name, "NewcatsHuang") }, transaction, 60);

            //8.根据给定的条件，更新记录(update userinfo set Name='Newcats',CreateTime='2021-12-31' where CreateTime>='2021-12-12' and CreateTime<'2021-12-30';)
            int r8 = _repository.Update<UserInfo>(
                new List<DbWhere<UserInfo>>
                {
                    new DbWhere<UserInfo>(s => s.CreateTime, new DateTime(2021, 12, 12), OperateTypeEnum.GreaterEqual, LogicTypeEnum.And),
                    new DbWhere<UserInfo>(s=>s.CreateTime,new DateTime(2021,12,30), OperateTypeEnum.Less, LogicTypeEnum.And)
                },
                new List<DbUpdate<UserInfo>>
                {
                    new DbUpdate<UserInfo>(s => s.Name,"Newcats"),
                    new DbUpdate<UserInfo>(s=>s.CreateTime,new DateTime(2021,12,31))
                });














            return "ok";
        }


        [HttpGet]
        public async Task<string> Get()
        {
            (IEnumerable<PgUserInfo> list, int totalCount) r111 = _repository.GetPage<PgUserInfo>(1, 10);
            (IEnumerable<PgUserInfo> list, int totalCount) r222 = _repository.GetPage<PgUserInfo>(new PageInfo<PgUserInfo>(2, 20));
            PageInfo<PgUserInfo> r333 = _repository.GetPageInfo<PgUserInfo>(3, 30, returnTotal: false);
            PageInfo<PgUserInfo> r444 = _repository.GetPageInfo<PgUserInfo>(4, 40, returnTotal: true);
            PageInfo<PgUserInfo> r555 = _repository.GetPageInfo<PgUserInfo>(new PageInfo<PgUserInfo>(5, 50), returnTotal: true);
            PageInfo<PgUserInfo> r666 = _repository.GetPageInfo<PgUserInfo>(new PageInfo<PgUserInfo>(5, 50), returnTotal: false);
            object r777 = _repository.Insert<PgUserInfo>(new PgUserInfo() { Id = IdHelper.Create(), Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)), JoinTime = DateTime.Now });
            bool r888 = _repository.Insert<PgUserInfo>(new PgUserInfo() { Id = IdHelper.Create(), Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)), JoinTime = DateTime.Now }, null);
            return r111.ToString();

            List<PgUserInfo> list = new List<PgUserInfo>();
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            for (int i = 0; i < 200; i++)
            {
                PgUserInfo u = new PgUserInfo()
                {
                    Id = IdHelper.Create(),
                    Name = EncryptHelper.GetRandomString(Random.Shared.Next(20)),
                    JoinTime = DateTime.Now
                };
                list.Add(u);
                //await _repository.InsertAsync<PgUserInfo>(u);
            }
            sw.Stop();
            var t1 = sw.ElapsedMilliseconds;
            sw.Restart();
            var r = await _repository.InsertSqlBulkCopyAsync<PgUserInfo>(list);
            sw.Stop();
            var t2 = sw.ElapsedMilliseconds;
            return $"t1:{t1}\r\nt2:{t2}\r\nt3:{(_repository.Count<PgUserInfo>()).ToString()}";

            //TODO: bug待修复
            //InvalidOperationException: The binary import operation was started with 3 column(s), but 0 value(s) were provided.
            //Npgsql.ThrowHelper.ThrowInvalidOperationException_BinaryImportParametersMismatch(int columnCount, int valueCount)

            //var r1 = _repository.GetTop<UserInfo>(10);
            //var r3 = _repository.Insert(new UserInfo { Id = Random.Shared.NextInt64(1, 3000), Name = "Newcats", JoinTime = DateTime.Now });

            //var r2 = await _repository.GetTopAsync<UserInfo>(20);
            //var r4 = await _repository.InsertAsync<UserInfo>(new UserInfo { Id = Random.Shared.NextInt64(1, 3000), Name = "huang", JoinTime = DateTime.Now });

            //_repository.Delete<UserInfo>(2658);

            //_repository.Update<UserInfo>(1367, new List<DbUpdate<UserInfo>>
            //{
            //    new DbUpdate<UserInfo>(r=>r.Name,"NewcatsHuang")
            //});

            //_repository.GetPage<UserInfo>(2, 5);

            //_repository.Count<UserInfo>();

            //_repository.Exists<UserInfo>();

            //return $"r1:{r1.Count()}\r\nr3:{r3}\r\nr2:{r2.Count()}\r\nr4:{r4}";
        }

        //[Audit]
        //[HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    //事务不能跨连接
        //    //using (var tran = _repository.BeginTransaction())
        //    //{
        //    //    _ = _repository.GetTop(1, null, tran);
        //    //    _ = _repository.GetTop(2, null, tran);
        //    //    tran.Commit();
        //    //}

        //    //分布式事务，可跨连接
        //    //using (var tran = TransactionScopeBuilder.CreateReadCommitted())
        //    //{
        //    //    _ = _repository.GetTop(1, null);
        //    //    _ = _repository.GetTop(2, null);
        //    //    _user.GetTop(3, null);
        //    //    tran.Complete();
        //    //}

        //    var r = _repository.GetAll();
        //    //var r2 = _user.GetAll();
        //    var r3 = _repository.GetAll();
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

        //[HttpGet]
        //public async Task<string> Get()
        //{
        //    List<UserInfo> users = new List<UserInfo>();
        //    for (int i = 0; i < 20000; i++)
        //    {
        //        users.Add(new UserInfo
        //        {
        //            Name = Newcats.Utils.Helpers.EncryptHelper.GetRandomString(Random.Shared.Next(10, 20)),
        //            UserId = i,
        //            JoinTime = new DateTime(2021, 10, 25)
        //        });
        //    }
        //    Stopwatch stopwatch = Stopwatch.StartNew();
        //    stopwatch.Start();

        //    var r = await _repository.InsertSqlBulkCopyAsync(users);


        //    stopwatch.Stop();
        //    return $"Result:{r.ToString()}\r\nTimes:{stopwatch.ElapsedMilliseconds}ms";
        //}

        [HttpGet("/WeatherForecast/Hello")]
        public string Hello()
        {
            return "hello";
        }
    }

    [Table("userinfo", Schema = "public")]
    public class PgUserInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [Column("CreateTime")]
        public DateTime JoinTime { get; set; }
    }

    [Table("UserInfo")]
    public class UserInfo
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        //[NotMapped]
        //public long? UserId { get; set; }

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; }
    }

    [Table("UserInfo")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Phone { get; set; }
    }

    [Table("UserInfo")]
    public class MySqlUserInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public DateTime CreateTime { get; set; }
    }
}
