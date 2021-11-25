# Newcats.DataAccess.SqlServer 使用说明

[![Net Core](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![Nuget](https://img.shields.io/static/v1?label=Nuget&message=1.1.5&color=blue)](https://www.nuget.org/packages/Newcats.DataAccess.SqlServer)
[![GitHub License](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

## 1.实体类

* 1.数据库实体类以Entity结尾
* 2.使用相关特性，对实体类属性进行设置
* TableAttribute：数据库表名，多表连接时为对应的连接关系
* KeyAttribute：数据库主键标识
* DatabaseGeneratedAttribute：数据库生成特性，标识自增、计算列等（插入时会忽略此字段）
* NotMappedAttribute：数据库中不存在此字段时，使用此特性忽略该字段
* ColumnAttribute：实体类别名映射特性，标注数据库实际字段名

### 默认约定

* 1.若不使用特性，则程序按默认约定进行解析
* 2.表名称为类名，或者类名去掉Entity字符串
* 3.主键为Id字段，或者Id结尾的字段
* 4.推荐使用特性进行设置

#### UserEntity.cs

```c#
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("User")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }

    public string AddressId { get; set; }

    [NotMapped]
    public string Phone { get; set; }
}

[Table("Address")]
public class AddressEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
}

[Table(" User a left join Address b on a.AddressId=b.Id ")]
public class UserDto
{
    [Column("a.Id")]
    public int Id { get; set; }

    [Column("a.Name")]
    public string Name { get; set; }

    [Column("b.Name")]
    public string Address { get; set; }
}
```

## 2.新建自定义DbContext,继承自 Newcats.DataAccess.SqlServer.DbContext

* 注1：不同的数据库类型继承不同的DbContext基类,此处以SqlServer为例
* 注2：若使用了多个数据库连接字符串，需要为每个连接字符串分别定义DbContext

#### MyDbContext.cs

```c#
public class MyDbContext : DbContext
{
    public MyDbContext(IOptions<DataAccess.Core.DbContextOptions> optionsAccessor) : base(optionsAccessor)
    {
    }
}
```

#### OtherDbContext.cs

```c#
public class OtherDbContext : DbContext
{
    public OtherDbContext(IOptions<DataAccess.Core.DbContextOptions> optionsAccessor) : base(optionsAccessor)
    {
    }
}
```

## 3.Startup.cs类里注册相应的服务

#### Startup.cs

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        //第一个DbContext
        services.AddSqlServerDataAccess<MyDbContext>(opt =>
        {
            opt.ConnectionString = "ConnectionStringOfMyDbContext";
        });

        //第二个DbContext
        //同一个应用可以注册不同的数据库，例如：services.AddMySqlDataAccess...
        services.AddSqlServerDataAccess<OtherDbContext>(opt =>
        {
            opt.ConnectionString = "ConnectionStringOfOtherDbContext";
        });
    }
}
```

## 4.服务层使用依赖注入获取

#### UserService.cs

```c#
public class UserService : IUserService
{
    private readonly Newcats.DataAccess.SqlServer.IRepository<MyDbContext> _myRepository;
    private readonly Newcats.DataAccess.SqlServer.IRepository<OtherDbContext> _otherRepository;

    public UserService(Newcats.DataAccess.SqlServer.IRepository<MyDbContext> myRepository, Newcats.DataAccess.SqlServer.IRepository<OtherDbContext> otherRepository)
    {
         _myRepository = myRepository;
        _otherRepository = otherRepository;
    }

    public async Task<UserEntity> GetAsync(int id)
    {
        //根据主键Id获取一条记录
        return await _myRepository.GetAsync<UserEntity>(id);
    }

    public async Task<IEnumerable<AddressInfo>> GetListAsync(int top)
    {
        //获取Id>=30的前top条记录
        return await _otherRepository.GetTopAsync<AddressInfo>(top, new List<DbWhere>()
        {
            new DbWhere(t=>t.Id, 30, OperateTypeEnum.GreaterEqual)
        });
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

## 作者: newcats-2021/11/25