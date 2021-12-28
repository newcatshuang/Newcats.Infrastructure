# Newcats.Office 使用说明

[![Net Core](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![Nuget](https://img.shields.io/static/v1?label=Nuget&message=1.0.7&color=blue)](https://www.nuget.org/packages/Newcats.Office)
[![GitHub License](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

## 示例代码：

### Newcats.Office.Excel

```c#
//1.读取Excel到DataTable(默认获取第一个Sheet)(第一行为表头,不记录数据)
const string fullFileName = @"C:\Users\Newcats\Desktop\Result_108.xlsx";
DataTable r1 = Newcats.Office.Excel.ReadExcelToTable(fullFileName);

//2.从流读取Excel到DataTable(默认获取第一个Sheet)(第一行为表头,不记录数据)
using (FileStream fs = new(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
{
    DataTable r2 = Newcats.Office.Excel.ReadExcelToTable(fs, Newcats.Office.Excel.ExcelFormatEnum.xlsx);
}
```

---

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