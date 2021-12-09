using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newcats.DataAccess.Core;
using Newcats.DataAccess.PostgreSql;
//using Newcats.DataAccess.MySql;
//using Newcats.DataAccess.SqlServer;
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
            //const string connStr = "Data Source=.;Initial Catalog=NewcatsDB20170627;User ID=sa;Password=123456;TrustServerCertificate=True";
            //const string connStr2 = "Data Source=.;Initial Catalog=AcadsochrDB20190701;User ID=sa;Password=123456;TrustServerCertificate=True";
            //const string mySqlStr = "server=localhost;port=3306;database=newcatshq20211019;uid=root;pwd=hq1232@mysql;CharSet=utf8;AllowLoadLocalInfile=true";
            //services.AddScoped(c => new DataAccess.SqlServer.DbContextBase(connStr));
            //services.AddScoped(c => new TwoDbContext(connStr2));
            //services.AddScoped(_ => new DataAccess.MySql.DbContextBase(mySqlStr));
            //services.AddScoped<IFileStore, FileStore>();
            //services.AddScoped(typeof(DataAccess.SqlServer.IRepository<>), typeof(DataAccess.SqlServer.Repository<>));//注册泛型仓储
            //services.AddScoped(typeof(DataAccess.MySql.IRepository<,,>), typeof(DataAccess.MySql.Repository<,,>));//注册泛型仓储
            const string pgSqlConnstr = "Host=192.168.13.131;Port=5432;Username=postgres;Password=mysql-server1-ubuntu;Database=NewcatsPgDB;Pooling=true;Maximum Pool Size=20";

            //services.AddMySqlDataAccess<MySqlDbContext>(opt =>
            //{
            //    opt.ConnectionString = mySqlStr;
            //});

            services.AddPostgreSqlDataAccess<PgContext>(option =>
            {
                option.ConnectionString = pgSqlConnstr;
            });

        }
    }

    //public class TwoDbContext : DbContextBase
    //{
    //    public TwoDbContext(string connectionString) : base(connectionString)
    //    {
    //    }
    //}

    //public class MySqlDbContext : DbContext
    //{
    //    public MySqlDbContext(IOptions<DataAccess.Core.DbContextOptions> optionsAccessor) : base(optionsAccessor)
    //    {
    //    }
    //}

    public class PgContext : Newcats.DataAccess.PostgreSql.DbContext
    {
        public PgContext(IOptions<DbContextOptions> optionsAccessor) : base(optionsAccessor)
        {
        }
    }
}