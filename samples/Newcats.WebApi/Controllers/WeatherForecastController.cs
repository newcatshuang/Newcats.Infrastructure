﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newcats.AspNetCore.Filters;
using Newcats.DataAccess;
using Newcats.DataAccess.Core;
using Newcats.DataAccess.MySql;

namespace Newcats.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly DataAccess.SqlServer.IRepository<DataAccess.SqlServer.DbContextBase> _repository;
        private readonly DataAccess.SqlServer.IRepository<TwoDbContext> _user;

        //private readonly DataAccess.MySql.IRepository<DataAccess.MySql.DbContextBase, UserInfo, int> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        //public WeatherForecastController(ILogger<WeatherForecastController> logger, IRepository<DbContextBase, UserInfo, int> repository)
        //{
        //    _repository = repository;
        //    _logger = logger;
        //    //_user = user;
        //}

        public WeatherForecastController(DataAccess.SqlServer.IRepository<DataAccess.SqlServer.DbContextBase> repository, DataAccess.SqlServer.IRepository<TwoDbContext> user)
        {
            _repository = repository;
            _user = user;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var r1 = _repository.GetTop<UserInfo>(10);
            _repository.Insert(new UserInfo { JoinTime = DateTime.Now, Name = "Newcats", UserId = 111 });

            var r2 = await _repository.GetTopAsync<UserInfo>(10);
            await _repository.InsertAsync<UserInfo>(new UserInfo { JoinTime = DateTime.Now, UserId = 222, Name = "huang" });

            var r3 = _user.GetTop<User>(10);
            var r4 = await _user.GetTopAsync<User>(10);

            _user.Insert<User>(new Controllers.User { Id = 111, Phone = "11111" });
            await _user.InsertAsync<User>(new Controllers.User { Id = 222, Phone = "222" });



            return "ok";
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

    [Table("UserInfo")]
    public class UserInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public long? UserId { get; set; }

        [Column("CreateTime")]
        public DateTime JoinTime { get; set; }
    }

    [Table("UserInfo")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Phone { get; set; }
    }
}
