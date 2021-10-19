## 数据库交互实体类及相关特性使用说明
 * 1.数据库实体类以Entity结尾
 * 2.使用相关特性，对实体类属性进行设置
 * TableAttribute：数据库表名，多表连接时为对应的连接关系
 * KeyAttribute：数据库主键标识
 * DatabaseGeneratedAttribute：数据库生成特性，标识自增、计算列等（插入时会忽略此字段）
 * NotMappedAttribute：数据库中不存在此字段时，使用此特性忽略该字段
 * ColumnAttribute：实体类别名映射特性，标注数据库实际字段名

## 默认约定
 * 1.若不使用特性，则程序按默认约定进行解析
 * 2.表名称为类名，或者类名去掉Entity字符串
 * 3.主键为Id字段，或者Id结尾的字段
 * 4.推荐使用特性进行设置

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