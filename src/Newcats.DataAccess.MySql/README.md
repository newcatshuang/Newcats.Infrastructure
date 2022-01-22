# Newcats.DataAccess.MySql 使用说明

[![Net Core](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![Nuget](https://img.shields.io/nuget/v/Newcats.DataAccess.MySql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.MySql) [![Newcats.DataAccess.MySql](https://img.shields.io/nuget/dt/Newcats.DataAccess.MySql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.MySql)
[![GitHub License](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

# 示例代码：

* 注：此处只展示同步方法示例，异步方法同理

```c#
//1.插入数据，返回主键
object r1 = _repository.Insert<UserInfo>(new UserInfo { Name = "Newcats", CreateTime = DateTime.Now });

//2.插入数据，返回是否成功
bool r2 = _repository.Insert<UserInfo>(new UserInfo { Id = 1, Name = "Huang", CreateTime = DateTime.UtcNow }, null);

//3.批量插入，返回成功的条数
int r3 = _repository.InsertBulk<UserInfo>(new List<UserInfo>() { new UserInfo { Name = "Newcats", CreateTime = DateTime.Now } }, transaction, 600);

//4.使用SqlBulkCopy批量插入数据
int r4 = _repository.InsertSqlBulkCopy<UserInfo>(new List<UserInfo>() { new UserInfo { Name = "Newcats", CreateTime = DateTime.Now } }, transaction, 600);

//5.根据主键删除一条数据(delete from userinfo where id=1;)
int r5 = _repository.Delete<UserInfo>(1);

//6.根据给定的条件，删除记录(删除CreateTime>=2021-12-12的记录)(delete from userinfo where createtime>='2021-12-12';)
int r6 = _repository.Delete<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.CreateTime, new DateTime(2021, 12, 12), OperateTypeEnum.GreaterEqual, LogicTypeEnum.And) });

//7.根据主键，更新一条记录(update userinfo set Name='NewcatsHuang' where id=2;)
int r7 = _repository.Update<UserInfo>(2, new List<DbUpdate<UserInfo>>() { new DbUpdate<UserInfo>(s => s.Name, "NewcatsHuang") }, transaction, 60);

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
}, null, null, new DbOrderBy<UserInfo>(s => s.CreateTime, SortTypeEnum.DESC));

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
IEnumerable<UserInfo> r18 = _repository.GetTop<UserInfo>(10, new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) }, null, null, new DbOrderBy<UserInfo>(s => sId, SortTypeEnum.ASC));

//19.获取记录总数量(select count(1) from userinfo;)
int r19 = _repository.Count<UserInfo>();

//20.根据给定的条件，获取记录数量(select count(1) from userinfo where Name like '%newcats%')
int r20 = _repository.Count<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

//21.根据主键，判断数据是否存在(select top 1 1 from userinfo where Id=2021;=>r==1?)
bool r21 = _repository.Exists<UserInfo>(2021);

//22.根据给定的条件，判断数据是否存在(select top 1 1 from userinfo where Name like '%newcats%';=>r==1?)
bool r22 = _repository.Exists<UserInfo>(new List<DbWhere<UserInfo>> { new DbWhere<UserInfo>(s => s.Name, "newcats", OperateTypeEnum.Like, LogicTypeEnum.And) });

//23.执行存储过程
DynamicParameters dp = new Dapper.DynamicParameters();
dp.Add("@id", 1);
int r23 = _repository.ExecuteStoredProcedure("Usp_GetUserName", dp);

//24.执行sql语句，返回受影响的行数
int r24 = _repository.Execute("delete from userinfo where Id=@id;", dp);

//25.执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
string r25 = _repository.ExecuteScalar<string>("select Name from userinfo where Id=@id;", dp);

//26.执行查询，并返回由查询返回的结果集中的第一行的第一列，其他行或列将被忽略
object r26 = _repository.ExecuteScalar("select Name from userinfo where Id=@id;", dp);

//27.执行查询，返回结果集
IEnumerable<UserInfo> r27 = _repository.Query<UserInfo>("select * from userinfo where Id=@id;", dp);

//28.执行单行查询，返回结果
UserInfo r28 = _repository.QueryFirstOrDefault<UserInfo>("select * from userinfo where Id=@id;", dp);

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
```

# 使用说明：

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

## 2.新建自定义DbContext,继承自 Newcats.DataAccess.MySql.DbContext

* 注1：不同的数据库类型继承不同的DbContext基类,此处以MySql为例
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
        services.AddMySqlDataAccess<MyDbContext>(opt =>
        {
            opt.ConnectionString = "ConnectionStringOfMyDbContext";//主库连接
            opt.EnableReadWriteSplit = true;//启用读写分离
            opt.ReplicaPolicy = ReplicaSelectPolicyEnum.WeightedRoundRobin;//从库选择策略为平滑加权轮询
            opt.ReplicaConfigs = new ReplicaConfig[]//从库配置
            {
                new ReplicaConfig(){ ReplicaConnectionString="从库连接字符串1", Weight=3},
                new ReplicaConfig(){ ReplicaConnectionString="从库连接字符串2", Weight=2},
                new ReplicaConfig(){ ReplicaConnectionString="从库连接字符串3", Weight=1}
            };
        });

        //第二个DbContext
        //同一个应用可以注册不同的数据库，例如：services.AddSqlServerDataAccess...
        services.AddMySqlDataAccess<OtherDbContext>(opt =>
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
    private readonly Newcats.DataAccess.MySql.IRepository<MyDbContext> _myRepository;
    private readonly Newcats.DataAccess.MySql.IRepository<OtherDbContext> _otherRepository;

    public UserService(Newcats.DataAccess.MySql.IRepository<MyDbContext> myRepository, Newcats.DataAccess.MySql.IRepository<OtherDbContext> otherRepository)
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