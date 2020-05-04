## 依赖注入使用说明
 * 1.在项目的Startup.cs类中，添加依赖注入服务
 ```c#
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        //依赖注入
        services.AddScoped(typeof(DataAccess.IRepository<,>), typeof(DataAccess.SqlServer.Repository<,>));//注册泛型仓储
        services.AddDependencyInjection();//注册依赖
    }
 ```
 * 2.对于需要使用依赖注入的接口，手动继承IScopedDependency/ISingletonDependency/ITransientDependency
 ```c#
    public interface IUserInfoService : IScopedDependency
    {
    }
 ```
 * 3.具体需要哪个标记接口，看需求，一般Web项目用IScopedDependency
 * 4.程序启动时会扫描bin目录下的所有自己项目的dll文件，然后注册依赖关系
 * 5.对于不会自动生成在bin下的项目dll,请直接引用
 * 6.使用构造函数注入
 * 7.对于不能使用构造函数注入的，可以使用HttpContext.RequestServices.GetService<IService>();
  ```c#
  IWorkContext _workContext = context.HttpContext.RequestServices.GetService<IWorkContext>();
 ```