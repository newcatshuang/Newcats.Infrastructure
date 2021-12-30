# Newcats.Utils 使用说明

[![Net Core](https://img.shields.io/badge/.NET-6-brightgreen.svg?style=flat-square)](https://dotnet.microsoft.com/download)
[![Nuget](https://img.shields.io/static/v1?label=Nuget&message=1.0.8&color=blue)](https://www.nuget.org/packages/Newcats.Utils)
[![GitHub License](https://img.shields.io/badge/license-MIT-purple.svg?style=flat-square)](https://github.com/newcatshuang/Newcats.Infrastructure/blob/master/LICENSE)

## 示例代码：

```c#
//1.IEnumerable数据转为DataTable
List<UserInfo> users = new List<UserInfo>();
users.Add(new UserInfo() { Id = 1, Name = "Newcats" });
users.Add(new UserInfo() { Id = 2, Name = "Huang" });
DataTable r1 = users.ToDataTable();

//2.DataTable数据转为List
List<UserInfo> r2 = r1.ToList<UserInfo>();

//3.根据condition条件，判断是否加入当前list集合
users.AddIf(new UserInfo() { Id = 3, Name = "NewcatsHuang" }, "newcats".Length == 0);

//4.元素不为null，则加入当前list集合
UserInfo u1 = null;
users.AddIfNotNull(u1);

//5.若集合为空，则抛异常
users.ThrowIfEmpty();

//6.若集合为null或者空，则抛异常
users.ThrowIfNullOrEmpty();

//7.判断集合是否为null或者空
bool r3 = users.IsNullOrEmpty();//=>false

//8.获取枚举项的Description特性描述
string r4 = Season.Spring.GetDescription();//=>春季

//9.获取枚举项的枚举描述类
Newcats.Utils.Models.EnumDescription r5 = Season.Spring.GetEnumDescription();//=>r5.Value=0,r5.Name="Spring",r5.Description="春季"

//10.获取枚举的所有枚举描述类
List<Newcats.Utils.Models.EnumDescription> r6 = Season.Spring.GetAllEnumDescriptions();//=>r6[0].Value=1,r6[0].Name="Spring"...r6[1].Value=1,r6[2].Description="夏季"

//11.判断当前对象是否为null
r4.ThrowIfNull();//=>false

//12.Md5加密，返回32位结果
string r7 = "Newcats".MD5By32();//=>A659F0864BCF9A600D048E6158482459

//13.获取此字符串的Sha1值
string r8 = "Newcats".Sha1();//=>F94EB89F28111FBCA123CE6318ED795AA59A244E

//14.获取此字符串的Sha256值
string r9 = "Newcats".Sha256();//=>CD787FAEBCA9E9E1FFA1F688F9CF5F221E0F703022807CBFF1FA8AA685A7C678

//15.获取此字符串的DES加密结果(默认密钥)
string r10 = "Newcats".DesEncrypt();//=>YTM3cMMHHT0=

//16.DES解密
string r11 = r10.DesDecrypt();//=>Newcats

//16.获取此字符串的DES加密结果
string r12 = "Newcats".DesEncrypt("newcatsnewcatsnewcats123");//=>DkPSMnDuLRM=

//18.DES解密
string r13 = r12.DesDecrypt("newcatsnewcatsnewcats123");//=>Newcats

//19.转换对象为json字符串(使用System.Text.Json)
string r14 = users[0].ToJson();//=>{"Id":1,"Name":"Newcats","UserId":null,"JoinTime":"0001-01-01 00:00:00.000"}

//20.把json字符串反序列化为对象(使用System.Text.Json)
UserInfo r15 = r14.Deserialize<UserInfo>();//=>r15.Id=1,r15.Name="Newcats",r15.UserId=null,r15.JoinTime=0001-01-01 00:00:00.000

//21.转换为Unix时间戳(秒)
long r16 = DateTime.Now.ToUnixTimestamp();//1640788430

//22.从Unix时间戳获取时间(秒)(默认返回北京时间)
DateTime r17 = 1640788430L.GetTimeFromUnixTimestamp(beijingTimeZone: true);

//23.判断字符串是否为null,空或空白格
bool r17 = "Newcats".IsNullOrWhiteSpace();//=>false

//24.截取字符串
string r18 = "NewcatsHuang".ToSubstring(7);//=>"Newcats"

//25.获取汉字的拼音首字母
string r19 = "中国".PinYin();//=>zg

//26.一些正则表达式扩展方法
bool r20 = "newcats@live.com".IsEmail();//=>true

//27.判断字符串是否非空
"newcats".ThrowIfNullOrWhiteSpace();

//28.获取指定长度的随机字符串
string r21 = Newcats.Utils.Helpers.EncryptHelper.GetRandomString(7);//=>iPWs3Nz

//29.获取指定长度的随机数字
string r22 = Newcats.Utils.Helpers.EncryptHelper.GetRandomNumber(7);//=>8699505

//30.获取指定长度的随机数字，字母，字符
string r23 = Newcats.Utils.Helpers.EncryptHelper.GetRandomKey(7);//=>7z|DGki

//31.AES加密
string r24 = Newcats.Utils.Helpers.EncryptHelper.AESEncrypt("Newcats");//=>dIuVIledkcP0Ron1gwBdCA==

//31.AES解密
string r25 = Newcats.Utils.Helpers.EncryptHelper.AESDecrypt("dIuVIledkcP0Ron1gwBdCA==");//=>Newcats

//32.RSA公钥加密
//33.RSA私钥解密
//34.RSA验证
//35.文件操作 Newcats.Utils.Helpers.FileHelper
//36.Http操作帮助类 Newcats.Utils.Helpers.HttpHelper

//37.雪花算法Id生产器
long r26 = Newcats.Utils.Helpers.IdHelper.Create();//=>3962686302872276996

//38.拼接了日期的雪花算法Id
string r27 = Newcats.Utils.Helpers.IdHelper.Create(true);//=>202112303962686561048465409

//39.Lambda表达式帮助类 Newcats.Utils.Helpers.LambdaHelper
//40.反射操作帮助类 Newcats.Utils.Helpers.ReflectionHelper
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