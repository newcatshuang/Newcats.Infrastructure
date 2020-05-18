using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newcats.DenpendencyInjection;

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
            services.AddScoped<IFileStore, FileStore>();
            services.AddScoped(typeof(DataAccess.IRepository<,>), typeof(DataAccess.SqlServer.Repository<,>));//注册泛型仓储
        }
    }
}