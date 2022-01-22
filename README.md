# Newcats.Infrastructure

[![Net](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![GitHub license](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

README: [中文](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/README.md) | [English](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/README.md)

---
The new infrastructure to launch a new era of the great .NET

---
## 组件库

|Package|NuGet|Document|Description|
|-------|--------|-------|-------|
| [Newcats.DataAccess.MySql](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.DataAccess.MySql) | [![Nuget](https://img.shields.io/nuget/v/Newcats.DataAccess.MySql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.MySql) [![Newcats.DataAccess.MySql](https://img.shields.io/nuget/dt/Newcats.DataAccess.MySql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.MySql) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.DataAccess.MySql/README.md)|基于dapper封装的mysql仓储组件，基于实体类直接CRUD操作，批量插入SqlBulkCopy等，支持一主多从的读写分离配置，支持平滑加权轮询等从库选择策略，支持强制使用主库查询
|[Newcats.DataAccess.PostgreSql](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.DataAccess.PostgreSql) | [![Nuget](https://img.shields.io/nuget/v/Newcats.DataAccess.PostgreSql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.SqlServer) [![Newcats.DataAccess.SqlServer](https://img.shields.io/nuget/dt/Newcats.DataAccess.PostgreSql.svg)](https://www.nuget.org/packages/Newcats.DataAccess.PostgreSql) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.DataAccess.PostgreSql/README.md)|基于dapper封装的PostgreSql仓储组件，基于实体类直接CRUD操作，批量插入SqlBulkCopy等，支持一主多从的读写分离配置，支持平滑加权轮询等从库选择策略，支持强制使用主库查询
|[Newcats.DataAccess.SqlServer](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.DataAccess.SqlServer) | [![Nuget](https://img.shields.io/nuget/v/Newcats.DataAccess.SqlServer.svg)](https://www.nuget.org/packages/Newcats.DataAccess.SqlServer) [![Newcats.DataAccess.SqlServer](https://img.shields.io/nuget/dt/Newcats.DataAccess.SqlServer.svg)](https://www.nuget.org/packages/Newcats.DataAccess.SqlServer) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.DataAccess.SqlServer/README.md)|基于dapper封装的sqlserver仓储组件，基于实体类直接CRUD操作，批量插入SqlBulkCopy等，支持一主多从的读写分离配置，支持平滑加权轮询等从库选择策略，支持强制使用主库查询
|[Newcats.DataAccess.Sqlite](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.DataAccess.Sqlite) | [![Nuget](https://img.shields.io/nuget/v/Newcats.DataAccess.Sqlite.svg)](https://www.nuget.org/packages/Newcats.DataAccess.Sqlite) [![Newcats.DataAccess.Sqlite](https://img.shields.io/nuget/dt/Newcats.DataAccess.Sqlite.svg)](https://www.nuget.org/packages/Newcats.DataAccess.Sqlite) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.DataAccess.Sqlite/README.md)|基于dapper封装的Sqlite仓储组件，基于实体类直接CRUD操作，批量插入SqlBulkCopy，修改密码等，支持一主多从的读写分离配置，支持平滑加权轮询等从库选择策略，支持强制使用主库查询
|[Newcats.DependencyInjection](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.DependencyInjection) | [![Nuget](https://img.shields.io/nuget/v/Newcats.DependencyInjection.svg)](https://www.nuget.org/packages/Newcats.DependencyInjection) [![Newcats.DependencyInjection](https://img.shields.io/nuget/dt/Newcats.DependencyInjection.svg)](https://www.nuget.org/packages/Newcats.DependencyInjection) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.DependencyInjection/README.md)|基于微软原生Microsoft DI的自动依赖注册组件
|[Newcats.Office](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.Office) | [![Nuget](https://img.shields.io/nuget/v/Newcats.Office.svg)](https://www.nuget.org/packages/Newcats.Office) [![Newcats.Office](https://img.shields.io/nuget/dt/Newcats.Office.svg)](https://www.nuget.org/packages/Newcats.Office) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.Office/README.md)|基于NPOI封装的excel导入导出组件
|[Newcats.Utils](https://github.com/newcatshuang/Newcats.Infrastructure/tree/master/src/Newcats.Utils) | [![Nuget](https://img.shields.io/nuget/v/Newcats.Utils.svg)](https://www.nuget.org/packages/Newcats.Utils) [![Newcats.Utils](https://img.shields.io/nuget/dt/Newcats.Utils.svg)](https://www.nuget.org/packages/Newcats.Utils) |[ReadMe](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/src/Newcats.Utils/README.md)|加密解密、rsa、des、AES、hash、雪花Id、中文拼音、Json序列化……常用扩展方法、帮助类、工具类等


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

## 作者: newcats-2020/05/04
