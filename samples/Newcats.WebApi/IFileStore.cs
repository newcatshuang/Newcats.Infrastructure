using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Newcats.DependencyInjection;

namespace Newcats.WebApi
{
    public interface IFileStore
    {
        void Save();
    }

    public class FileStore : IFileStore
    {
        public void Save()
        {
            Console.WriteLine("hello file");
        }
    }

    public class ServiceRegister : IDependencyRegistrar
    {
        public void Register(IServiceCollection services)
        {
            const string connStr = "Data Source=.;Initial Catalog=NewcatsDB20170627;User ID=sa;Password=123456;TrustServerCertificate=True";
            const string connStr2 = "Data Source=.;Initial Catalog=AcadsochrDB20190701;User ID=sa;Password=123456;TrustServerCertificate=True";
            services.AddScoped(c => new DataAccess.DbContextBase(connStr));
            services.AddScoped(c => new TwoDbContext(connStr2));
            services.AddScoped<IFileStore, FileStore>();
            services.AddScoped(typeof(DataAccess.IRepository<,,>), typeof(DataAccess.Repository<,,>));//注册泛型仓储
        }
    }

    public class TwoDbContext : DataAccess.DbContextBase
    {
        public TwoDbContext(string connectionString) : base(connectionString)
        {
        }
    }
}