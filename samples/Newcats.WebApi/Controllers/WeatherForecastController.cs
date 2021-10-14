using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newcats.AspNetCore.Filters;
using Newcats.DataAccess.SqlServer;

namespace Newcats.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly DataAccess.IRepository<DbContextBase, UserInfo, long> _repository;
        private readonly DataAccess.IRepository<TwoDbContext, User, long> _user;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DataAccess.IRepository<DbContextBase, UserInfo, long> repository, DataAccess.IRepository<TwoDbContext, User, long> user)
        {
            _repository = repository;
            _logger = logger;
            _user = user;
        }

        [Audit]
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var r = _repository.GetAll();
            var r2 = _user.GetAll();
            var r3 = _repository.GetAll();
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("/WeatherForecast/Hello")]
        public string Hello()
        {
            return "hello";
        }
    }

    [Table("sys_AdminUser")]
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
