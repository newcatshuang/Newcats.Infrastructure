using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// 连接逻辑
    /// </summary>
    public enum LogicTypeEnum
    {
        /// <summary>
        /// where语句之间用and连接
        /// </summary>
        And = 0,

        /// <summary>
        /// where语句之间用or连接
        /// </summary>
        Or = 1
    }

    /// <summary>
    /// 操作逻辑
    /// </summary>
    public enum OperateTypeEnum
    {
        /// <summary>
        /// 相等
        /// </summary>
        Equal = 0,

        /// <summary>
        /// 不相等
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// 大于
        /// </summary>
        Greater = 2,

        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterEqual = 3,

        /// <summary>
        /// 小于
        /// </summary>
        Less = 4,

        /// <summary>
        /// 小于等于
        /// </summary>
        LessEqual = 5,

        /// <summary>
        /// like
        /// </summary>
        Like = 6,

        /// <summary>
        /// LeftLike
        /// </summary>
        LeftLike = 7,

        /// <summary>
        /// RightLike
        /// </summary>
        RightLike = 8,

        /// <summary>
        /// NotLike
        /// </summary>
        NotLike = 9,

        /// <summary>
        /// In
        /// </summary>
        In = 10,

        /// <summary>
        /// NotIn
        /// </summary>
        NotIn = 11,

        /// <summary>
        /// 不带参数的sql语句
        /// </summary>
        //SqlFormat = 12,

        /// <summary>
        /// 带参数的sql语句
        /// </summary>
        //SqlFormatPar = 13,

        /// <summary>
        /// Between
        /// </summary>
        Between = 14,

        /// <summary>
        /// End
        /// </summary>
        End = 15,

        /// <summary>
        /// 直接拼接sql语句.
        /// sql写在第二个参数value处.
        /// </summary>
        SqlText = 16
    }

    /// <summary>
    /// 排序类型
    /// </summary>
    public enum SortTypeEnum
    {
        /// <summary>
        /// 正序
        /// </summary>
        ASC = 0,

        /// <summary>
        /// 倒序
        /// </summary>
        DESC = 1
    }

    /// <summary>
    /// 数据库sql where参数封装类
    /// 封装了sql逻辑，便于生成sql语句
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    public class DbWhere<TEntity> where TEntity : class
    {
        /// <summary>
        /// 属性名（即数据库表的字段名）
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// 对应字段的值
        /// </summary>
        public object Value { get; private set; } = null;

        /// <summary>
        /// 操作逻辑
        /// </summary>
        public OperateTypeEnum OperateType { get; private set; }

        /// <summary>
        /// 连接逻辑（and/or）
        /// </summary>
        public LogicTypeEnum LogicType { get; private set; }

        /// <summary>
        /// 数据库sql where参数封装类
        /// </summary>
        /// <param name="expression">表达式，例u=>u.Id</param>
        /// <param name="value">表达式的值</param>
        /// <param name="operateType">操作逻辑</param>
        /// <param name="logicType">连接逻辑</param>
        public DbWhere(Expression<Func<TEntity, object>> expression, object value, OperateTypeEnum operateType = OperateTypeEnum.Equal, LogicTypeEnum logicType = LogicTypeEnum.And)
        {
            PropertyInfo property = GetProperty(expression) as PropertyInfo;
            var real = property.GetCustomAttribute(typeof(ColumnAttribute), false);
            if (real != null && real is ColumnAttribute)
            {
                PropertyName = ((ColumnAttribute)real).Name;
            }
            else
            {
                PropertyName = property.Name;
            }
            Value = value;
            OperateType = operateType;
            LogicType = logicType;
        }

        private MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }
    }

    /// <summary>
    /// 数据库sql update参数的封装类
    /// 封装了sql逻辑，便于生成sql语句
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    public class DbUpdate<TEntity> where TEntity : class
    {
        /// <summary>
        /// 属性名（即数据库表的字段名）
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// 对应字段的值
        /// </summary>
        public object Value { get; private set; } = null;

        /// <summary>
        /// 数据库sql update参数的封装类
        /// </summary>
        /// <param name="expression">表达式，例u=>u.Id</param>
        /// <param name="value">表达式的值</param>
        public DbUpdate(Expression<Func<TEntity, object>> expression, object value)
        {
            PropertyInfo property = GetProperty(expression) as PropertyInfo;
            PropertyName = property.Name;
            Value = value;
        }

        private MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }
    }

    /// <summary>
    /// 数据库sql order by参数的封装类
    /// 封装了sql逻辑，便于生成sql语句
    /// </summary>
    /// <typeparam name="TEntity">数据库实体类</typeparam>
    public class DbOrderBy<TEntity> where TEntity : class
    {
        /// <summary>
        /// 属性名（即数据库表的字段名）
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// 排序类型
        /// </summary>
        public string OrderByType { get; private set; }

        /// <summary>
        /// 数据库sql order by参数的封装类
        /// </summary>
        /// <param name="expression">表达式，例u=>u.Id</param>
        /// <param name="orderByType">SortType.ASC/SortType.DESC</param>
        public DbOrderBy(Expression<Func<TEntity, object>> expression, SortTypeEnum orderByType = SortTypeEnum.ASC)
        {
            PropertyInfo property = GetProperty(expression) as PropertyInfo;
            var real = property.GetCustomAttribute(typeof(ColumnAttribute), false);
            if (real != null && real is ColumnAttribute)
            {
                PropertyName = ((ColumnAttribute)real).Name;
            }
            else
            {
                PropertyName = property.Name;
            }
            OrderByType = orderByType.ToString();
        }

        /// <summary>
        /// 数据库sql order by参数的封装类
        /// </summary>
        /// <param name="propertyName">属性名（即数据库表的字段名）</param>
        /// <param name="isAsc">是否升序</param>
        public DbOrderBy(string propertyName, bool isAsc = true)
        {
            PropertyName = propertyName;
            OrderByType = isAsc ? "ASC" : "DESC";
        }

        private MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }
    }
}