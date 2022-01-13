# Newcats.DependencyInjection 使用说明

[![Net Core](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![Nuget](https://img.shields.io/nuget/v/Newcats.DependencyInjection.svg)](https://www.nuget.org/packages/Newcats.DependencyInjection) [![Newcats.DependencyInjection](https://img.shields.io/nuget/dt/Newcats.DependencyInjection.svg)](https://www.nuget.org/packages/Newcats.DependencyInjection)
[![GitHub License](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

## 1.标记接口

### ISingletonDependency： 整个应用程序生命周期以内只创建一个实例

### IScopedDependency：在同一个Scope内只初始化一个实例 ，可以理解为（ 每一个request级别只创建一个实例，同一个http request会在一个 scope内）

### ITransientDependency：每一次GetService都会创建一个新的实例

* 注1：具体需要哪个标记接口，看需求，一般Web项目用IScopedDependency
* 注2：程序启动时会扫描bin目录下的所有自己项目的dll文件，查找上述3个标记接口，然后注册依赖关系
* 注3：对于不会自动生成在bin下的项目dll,请直接引用
* 注4：推荐使用使用构造函数注入
* 注5：对于不能使用构造函数注入的，可以使用HttpContext.RequestServices.GetService<IService>();

## 2.注册服务

#### Startup.cs

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddDependencyInjection();//注册依赖
    }

   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
   {
        app.UseStaticHttpContext();//可选,如需使用HttpContext.RequestServices.GetService<IService>();则必须添加此行代码
   }
}
```

## 3.添加依赖注册器
* 注：不想使用标记接口，不想在Startup.cs文件里写太多的注册依赖，需要手动管理依赖的，可以使用依赖注册器
* 添加类，继承Newcats.DependencyInjection.IDependencyRegistrar即可

```c#
/// <summary>
/// 自定义依赖注册器
/// </summary>
public class ServiceRegister : IDependencyRegistrar
{
   public void Register(IServiceCollection services)
      {
         const string mySqlStr = "server=localhost;port=3306;database=NewcatsDB20211019;uid=root;pwd=1232@mysql;CharSet=utf8;AllowLoadLocalInfile=true";

         services.AddScoped<IFileStore, FileStore>();
            
         //services.AddScoped(typeof(DataAccess.SqlServer.IRepository<>), typeof(DataAccess.SqlServer.Repository<>));//注册泛型仓储
            
         //services.AddScoped(typeof(DataAccess.MySql.IRepository<>), typeof(DataAccess.MySql.Repository<>));//注册泛型仓储

         services.AddMySqlDataAccess<MySqlDbContext>(opt =>
         {
               opt.ConnectionString = mySqlStr;
         });
        }
    }
```

## 4.使用构造函数注入

 ```c#
    //接口
    public interface IUserInfoService : IScopedDependency
    {
    }

    //实现
    public class UserInfoService : IUserInfoService
    {

    }
 ```

#### UserInfoController.cs

 ```c#
    //控制器
    public class UserInfoController : ControllerBase
    {
       private readonly IUserInfoService _userService;

       public UserInfoController(IUserInfoService userService)
       {
          _userService = userService;//此处得到的即为IUserInfoService的实现类UserInfoService
       }
    }
 ```

## 贡献与反馈

> 如果你在阅读或使用任意一个代码片断时发现Bug，或有更佳实现方式，欢迎提Issue。 

> 对于你提交的代码，如果我们决定采纳，可能会进行相应重构，以统一代码风格。 

> 对于热心的同学，将会把你的名字放到**贡献者**名单中。  

---

## 免责声明

* 虽然代码已经进行了高度审查，并用于自己的项目中，但依然可能存在某些未知的BUG，如果你的生产系统蒙受损失，本人不会对此负责。
* 出于成本的考虑，将不会对已发布的API保持兼容，每当更新代码时，请注意该问题。

---

## 协议
[MIT](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE) © Newcats

---

## 作者: newcats-2021/12/01