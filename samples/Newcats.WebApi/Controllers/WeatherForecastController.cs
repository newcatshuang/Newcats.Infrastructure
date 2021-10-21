using System;
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
        //private readonly DataAccess.IRepository<DbContextBase, UserInfo, long> _repository;
        //private readonly DataAccess.IRepository<TwoDbContext, User, long> _user;

        private readonly DataAccess.MySql.IRepository<DataAccess.MySql.DbContextBase, UserInfo, int> _repository;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, Newcats.DataAccess.MySql.IRepository<DataAccess.MySql.DbContextBase, UserInfo, int> repository)
        {
            _repository = repository;
            _logger = logger;
            //_user = user;
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

        [HttpGet]
        public async Task<string> Get()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var r = _repository.GetPage(100, 100);


            stopwatch.Stop();
            return $"Result:{r.list.First().ToString()}\r\nTimes:{stopwatch.ElapsedMilliseconds}ms";
        }

        [HttpGet("/WeatherForecast/Hello")]
        public string Hello()
        {
            return "hello";
        }
    }

    [Table("userinfo")]
    public class UserInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    [Table("UserInfo")]
    public class User
    {
        public long Id { get; set; }

        public string Phone { get; set; }
    }
}
