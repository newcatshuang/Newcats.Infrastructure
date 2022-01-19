using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
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

        //private readonly DataAccess.PostgreSql.IRepository<PgContext> _repository;

        private readonly DataAccess.Sqlite.IRepository<SqliteContext> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(DataAccess.Sqlite.IRepository<SqliteContext> repository)
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

        [HttpGet]
        public async Task<string> Index()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //_repository.Execute("create table NewcatsUserInfoTest(Id bigint,Name varchar(64),CreateTime datetime);");

            var getall = _repository.GetAll<UserInfo>();
            //bool success = await _repository.ChangePasswordAsync("NewcatsPassword");

            return getall.Count().ToString();

            List<UserInfo> list = new List<UserInfo>();
            for (int i = 0; i < 50; i++)
            {
                long id = IdHelper.Create();
                string name = EncryptHelper.GetRandomString(Random.Shared.Next(20));
                DateTime now = DateTime.Now;
                list.Add(new UserInfo { Id = id, Name = name, CreateTime = now });
            }
            _repository.InsertBulk(list);


            //using (var tran = _repository.BeginTransaction())
            //{
            //    _repository.Insert<UserInfo>(new UserInfo { Id = 1, Name = "Newcats", CreateTime = DateTime.Now }, tran);
            //    _repository.InsertSqlBulkCopy(list);
            //    sw.Stop();
            //    tran.Commit();


            //}

            //return sw.ElapsedMilliseconds.ToString() + "ms";

            var rrrr = _repository.Count<UserInfo>();
            var all = _repository.GetAll<UserInfo>();
            //_repository.Execute("drop table NewcatsUserInfoTest;");
            return "ok";

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
            int r6 = _repository.Delete<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.CreateTime, new DateTime(2022, 12, 12), OperateTypeEnum.GreaterEqual, LogicTypeEnum.And) });

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

            //9.根据主键，获取一条记录(select * from userinfo where id=1;)
            UserInfo r9 = _repository.Get<UserInfo>(1);

            //10.根据给定条件，获取一条记录(select top 1 * from userinfo where Name='Newcats' order by CreateTime desc;)
            UserInfo r10 = _repository.Get<UserInfo>(new List<DbWhere<UserInfo>>
            {
                new DbWhere<UserInfo> (s=>s.Name,"Newcats", OperateTypeEnum.Equal, LogicTypeEnum.And)
            }, null, null, false, new DbOrderBy<UserInfo>(s => s.CreateTime, SortTypeEnum.DESC));

            //11.根据给定的条件及排序，分页获取数据(获取Name包含'newcats'字符串的第2页的20条数据)
            (IEnumerable<UserInfo> list, int totalCount) r11 = _repository.GetPage<UserInfo>(1, 20, new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

            //12.分页获取数据，逻辑同上
            var p = new PageInfo<UserInfo>(1, 20);
            p.Where = new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) };
            (IEnumerable<UserInfo> list, int totalCount) r12 = _repository.GetPage<UserInfo>(p);

            //13.根据给定的条件及排序，分页获取数据，逻辑同上
            PageInfo<UserInfo> r13 = _repository.GetPageInfo<UserInfo>(1, 20, new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

            //14.根据给定的条件及排序，分页获取数据，逻辑同上
            PageInfo<UserInfo> r14 = _repository.GetPageInfo<UserInfo>(p);

            //15.获取所有数据
            IEnumerable<UserInfo> r15 = _repository.GetAll<UserInfo>();

            //16.根据给定的条件及排序，获取所有数据(获取Name包含'newcats'字符串的所有数据)
            IEnumerable<UserInfo> r16 = _repository.GetAll<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

            //17.根据默认排序，获取指定数量的数据(select top 10 * from userinfo;)
            IEnumerable<UserInfo> r17 = _repository.GetTop<UserInfo>(10);

            //18.根据给定的条件及排序，获取指定数量的数据(select top 10 * from userinfo where Name like '%newcats%' order by Id;)
            IEnumerable<UserInfo> r18 = _repository.GetTop<UserInfo>(10, new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) }, null, null, false, new DbOrderBy<UserInfo>(s => s.Id, SortTypeEnum.ASC));

            //19.获取记录总数量(select count(1) from userinfo;)
            int r19 = _repository.Count<UserInfo>();

            //20.根据给定的条件，获取记录数量(select count(1) from userinfo where Name like '%newcats%')
            int r20 = _repository.Count<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

            //21.根据主键，判断数据是否存在(select top 1 1 from userinfo where Id=2021;=>r==1?)
            bool r21 = _repository.Exists<UserInfo>(2021);

            //22.根据给定的条件，判断数据是否存在(select top 1 1 from userinfo where Name like '%newcats%';=>r==1?)
            bool r22 = _repository.Exists<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

            return "ok";

            //23.执行存储过程
            DynamicParameters dp = new Dapper.DynamicParameters();
            dp.Add("@id", 1);
            //int r23 = _repository.ExecuteStoredProcedure("Usp_GetUserName", dp);

            //24.执行sql语句，返回受影响的行数
            int r24 = _repository.Execute("delete from userinfo where Id=@id;", true, dp);

            //25.执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
            string r25 = _repository.ExecuteScalar<string>("select Name from userinfo where Id=@id;", false, dp);

            //26.执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
            object r26 = _repository.ExecuteScalar("select Name from userinfo where Id=@id;", false, dp);

            //27.执行查询，返回结果集
            IEnumerable<UserInfo> r27 = _repository.Query<UserInfo>("select * from userinfo where Id=@id;", false, dp);

            //28.执行单行查询，返回结果
            UserInfo r28 = _repository.QueryFirstOrDefault<UserInfo>("select * from userinfo where Id=@id;", false, dp);

            //29.事务一
            using (var tran = _repository.BeginTransaction())
            {
                try
                {
                    _repository.Delete<UserInfo>(1, tran);
                    _repository.Delete<UserInfo>(2, tran);
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }

            //30.事务二
            using (var tran = _repository.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    _repository.Delete<UserInfo>(1, tran);
                    _repository.Delete<UserInfo>(2, tran);
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }

            //31.事务三
            using (TransactionScope scope = TransactionScopeBuilder.Create(IsolationLevel.ReadUncommitted, true))
            {
                try
                {
                    _repository.Delete<UserInfo>(1);
                    _repository.Delete<UserInfo>(2);
                    scope.Complete();
                }
                catch (Exception)
                {
                    throw;
                }
            }












            return "ok";
        }



        public async Task<string> Get()
        {
            return "ok";
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

    [Table("NewcatsUserInfoTest")]
    public class PgUserInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [Column("CreateTime")]
        public DateTime JoinTime { get; set; }
    }

    [Table("NewcatsUserInfoTest")]
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
