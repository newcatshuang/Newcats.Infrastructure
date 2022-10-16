/***************************************************************************
 *GUID: 956dcd62-a226-4dcf-8148-19cfa235e542
 *CLR Version: 4.0.30319.42000
 *DateCreated: 10/16/2022 2:13:00 PM
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Newcats.Utils.Extensions;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// 深度克隆
    /// https://gitee.com/jackletter/DotNetCommon/blob/master/src/DotNetCommon.Core/DeepCloneHelper.cs
    /// </summary>
    public class CloneHelper
    {
        /// <summary>
        /// 克隆时用的缓存字典,内部封装了 ConcurrentDictionary，由 containsRepeatReference 决定是否使用内部的字典
        /// </summary>
        internal class CacheDictionary
        {
            /// <summary>
            /// 缓存字典
            /// </summary>
            private readonly ConcurrentDictionary<object, object> dic;

            /// <summary>
            /// 是否启用引用关系复制
            /// </summary>
            private readonly bool containsRepeatReference = true;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="containsRepeatReference"></param>
            /// <param name="capacity"></param>
            public CacheDictionary(bool containsRepeatReference, int capacity = 16)
            {
                this.containsRepeatReference = containsRepeatReference;

                if (containsRepeatReference)
                    dic = new ConcurrentDictionary<object, object>(1, capacity);
            }

            /// <summary>
            /// 是否包含缓存(不可删除，存在反射引用)
            /// </summary>
            /// <param name="key">键</param>
            /// <returns></returns>
            public bool ContainsKey(object key)
            {
                if (!containsRepeatReference) return false;
                return dic.ContainsKey(key);
            }

            /// <summary>
            /// 添加缓存(不可删除，存在反射引用)
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void Add(object key, object value)
            {
                if (!containsRepeatReference) return;
                _ = dic.TryAdd(key, value);
            }

            /// <summary>
            /// 获取项(不可删除，存在反射引用)
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public object get_Item(object obj)
            {
                if (!containsRepeatReference) return null;
                return dic.ContainsKey(obj) ? dic[obj] : null;
            }

            /// <summary>
            /// 设置项(不可删除，存在反射引用)
            /// </summary>
            /// <param name="key"></param>
            /// <param name="val"></param>
            public void set_Item(object key, object val)
            {
                if (!containsRepeatReference) return;
                _ = dic.TryAdd(key, val);
            }
        }

        /// <summary>
        /// 包装器
        /// </summary>
        class Wrapper
        {
            /// <summary>
            /// 方法
            /// </summary>
            public Func<object, CacheDictionary, object> Method { set; get; }

            /// <summary>
            /// 复制
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="dic"></param>
            /// <returns></returns>
            public object Copy(object obj, CacheDictionary dic) => Method.Invoke(obj, dic);
        }

        /// <summary>
        /// 简单类型
        /// </summary>
        static Type[] simpleTypes = new[] { typeof(DateTime),typeof(Guid),typeof(TimeSpan), typeof(DateTimeOffset),typeof(DBNull),/*DBNull的TypeCode是Object*/
            typeof(Vector2),typeof(Vector3),typeof(Vector4),typeof(Matrix3x2),typeof(Matrix4x4),typeof(Plane),typeof(Quaternion),
#if NET6_0_OR_GREATER
            typeof(DateOnly),  typeof(TimeOnly),
#endif
             };

        /// <summary>
        /// 判断是否简单类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool JudgeSimple(Type type)
        {
            var typecode = type.GetTypeCode();
            if (typecode == TypeCode.Object)
            {
                if (type.IsNullable())
                {
                    type = type.GenericTypeArguments.FirstOrDefault();
                    typecode = type.GetTypeCode();
                }
            }
            var flag = false;
            switch (typecode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return true;
                case TypeCode.Object:
                default:
                    break;
            }
            if (simpleTypes.Contains(type))
            {
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// 缓存
        /// </summary>
        static ConcurrentDictionary<Type, Wrapper> _cache = new();

        /// <summary>
        /// 深度克隆对象实例，支持 <c>简单类pojo</c>、<c>数组</c>、<c>List</c>、<c>dicitionary</c>、<c>ValueTuple&lt;></c>、<c>匿名类型</c> 等
        /// 
        /// 示例代码:
        /// <code>
        /// var list = new List&lt;Person>()
        /// {
        ///     new Person{ Id = 1, Name = "小明" },
        ///     new Person{ Id = 2, Name = "小刚" }
        /// }
        /// var newList = list.Clone(false);//因为数据格式简单,没有冗余、循环的引用,传入 false 将克隆缓存关掉以提升性能
        /// </code>
        /// 当实例内部有冗余、循环的引用时:
        /// <code>
        /// var root = new Node { Id = 1, Children = new List&lt;Node>() };
        /// var current = new Node { Id = 2,Parent=root };
        /// root.Children.Add(current);
        /// var newRoot = root.Clone(true);//因为数据之间有冗余、循环的引用, 传入 true 打开克隆缓存，引用关系将被一起克隆
        /// </code>
        /// </summary>
        /// <remarks>
        /// 注意：本方法优先使用 <see cref="ICloneable"/> 中的 <c>Clone</c> 方法
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="containsRepeatReference">是否考虑重复的引用,当要克隆的对象内部没有冗余的、循环的引用时,将此设为 <c>false</c> 可能提升一半性能</param>
        /// <param name="capacity">当 <c>containsRepeatReference</c> 设为true时,缓存字典的默认容量</param>
        /// <returns></returns>
        public static T Clone<T>(T obj, bool containsRepeatReference = true, int capacity = 32)
        {
            var type = obj?.GetType();
            if (type == null)
                return default;

            if (_cache.ContainsKey(type))
            {
                return (T)(_cache[type].Method.Invoke(obj, new CacheDictionary(containsRepeatReference, capacity)));
            }

            var tmpDic = new ConcurrentDictionary<Type, Func<object>>();
            var func = _cache.GetOrAdd(type, type =>
            {
                var tmpCache = new Dictionary<Type, Wrapper>();
                var func = GetCloneMethod(type, tmpCache);
                if (tmpCache.Count > 0)
                {
                    foreach (var item in tmpCache)
                    {
                        _ = _cache.TryAdd(item.Key, item.Value);
                    }
                }
                return func;
            });

            return (T)func.Method(obj, new CacheDictionary(containsRepeatReference, capacity));

            #region GetCloneMethod
            Wrapper GetCloneMethod(Type type, Dictionary<Type, Wrapper> tmpCache)
            {
                if (_cache.ContainsKey(type)) return _cache[type];
                if (tmpCache.ContainsKey(type)) return tmpCache[type];
                var wrapper = new Wrapper();
                tmpCache.Add(type, wrapper);

                //基础类型
                var typecode = type.GetTypeCode();
                #region judgeSimple wrapper.Method = obj => obj;
                if (JudgeSimple(type))
                {
                    wrapper.Method = (obj, dic) => obj;
                    return wrapper;
                }
                #endregion

                var reflect = type.GetClassGenericFullName();
                if (type.IsArray)
                {
                    GetCloneMethod_Array(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                //else if (reflect.Name == "Newtonsoft.Json.Linq.JObject")
                //{
                //    wrapper.Method = (obj, dic) => dic.ContainsKey(obj) ? dic.get_Item(obj) : (obj as JObject).DeepClone();
                //    return wrapper;
                //}
                //else if (reflect.Name == "Newtonsoft.Json.Linq.JArray")
                //{
                //    wrapper.Method = (obj, dic) => dic.ContainsKey(obj) ? dic.get_Item(obj) : (obj as JArray).DeepClone();
                //    return wrapper;
                //}
                //else if (reflect.Name == "Newtonsoft.Json.Linq.JToken")
                //{
                //    wrapper.Method = (obj, dic) => dic.ContainsKey(obj) ? dic.get_Item(obj) : (obj as JToken).DeepClone();
                //    return wrapper;
                //}
                else if (reflect.Name == "System.Collections.Generic.List<T>")
                {
                    GetCloneMethod_List(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (type.GetInterface("System.ICloneable") != null)
                {
                    //无论是值类型还是引用类型都可以实现ICloneable方法
                    wrapper.Method = (obj, dic) => (obj as ICloneable).Clone();
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.ValueTuple<"))
                {
                    GetCloneMethod_ValueTuple(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.Collections.Generic.Dictionary<TKey, TValue>"))
                {
                    GetCloneMethod_Dictionary(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.Collections.Generic.HashSet<T>"))
                {
                    GetCloneMethod_HashSet(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.Collections.Generic.LinkedList<T>"))
                {
                    GetCloneMethod_LinkedList(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith(".<>f__AnonymousType"))
                {
                    GetCloneMethod_Anonymous(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.Tuple<"))
                {
                    GetCloneMethod_Tuple(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else if (reflect.Name.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection<T>"))
                {
                    GetCloneMethod_ReadOnlyCollection(type, reflect, wrapper, tmpCache);
                    return wrapper;
                }
                else
                {
                    var types = type.GetGenericArguments();

                    var enumerable = type.GetInterfaces().Where(i => i.Name == "IEnumerable`1").FirstOrDefault();
                    if (enumerable != null)
                    {
                        reflect = enumerable.GetClassGenericFullName();
                        GetCloneMethod_IEnumerable(type, reflect, wrapper, tmpCache);
                    }
                    else if (type.IsValueType)
                    {
                        GetCloneMethod_Struct(type, reflect, wrapper, tmpCache);
                    }
                    else
                    {
                        GetCloneMethod_Pojo(type, reflect, wrapper, tmpCache);
                    }
                    return wrapper;
                }
            }

            #region GetCommonExp
            (ParameterExpression para_obj,
                    ParameterExpression para_dic,
                    LabelTarget retLabel,
                    LabelExpression retExp,
                    ConditionalExpression ifnullExp,
                    ConditionalExpression ifCacheExp
                    ) GetCommonExp()
            {
                var para_obj = Expression.Parameter(typeof(object), "obj");//(object i)
                var para_dic = Expression.Parameter(typeof(CacheDictionary), "dic");//(dictionary<object,object> dic)
                var retLabel = Expression.Label(typeof(object), "ret");
                var retExp = Expression.Label(retLabel, para_obj);
                var ifnullExp = Expression.IfThen(Expression.Equal(para_obj, Expression.Constant(null)), Expression.Return(retLabel, para_obj));//if (i==null) return i;
                var ifCacheExp = Expression.IfThen(Expression.IsTrue(Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("ContainsKey"), para_obj)), Expression.Return(retLabel, Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("get_Item"), para_obj)));//if (dic.ContainsKey[i]) return dic[i];
                return (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp);
            }
            #endregion

            #region GetCloneMethod_List
            void GetCloneMethod_List(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type))
                {
                    var tmpRes = Expression.Variable(type, "res");//var res=int[];
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.Call(null, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(reflect.GenericTypes.FirstOrDefault().type), Expression.TypeAs(para_obj, type)));//(obj as List<int>).ToList()
                    var tmpAddDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, tmpRes);//dic.Add(i,res);
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, tmpAddDic, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //List<Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as List<Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));
                //int count;
                var countVar = Expression.Variable(typeof(int), "count");
                var countProp = Expression.Property(localObj, "Count");
                //count=obj2.Count;
                var assignCount = Expression.Assign(countVar, countProp);

                //List<Person> res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(type.GetConstructor(new Type[] { typeof(int) }), new Expression[] { countVar });
                //res=new List<Person>(count);
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                var innerClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);

                //for(var i=0;i<count;i++)
                //int i;
                var localVar_i = Expression.Variable(typeof(int), "i");
                //i=0;
                var loopInit = Expression.Assign(localVar_i, Expression.Constant(0));//i=0
                var breakLabel = Expression.Label("break");

                //res.Add(innerClone(obj2[i]))
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.LessThan(localVar_i, countVar),//if(i<count)
                        Expression.Block(
                            Expression.Call(//res.Add(innerClone(obj2[i])
                localRes
                                , type.GetMethod("Add", new Type[] { reflect.GenericTypes[0].type })
                                , Expression.TypeAs(//innerClone(obj2[i]) as Person
                                    Expression.Invoke(//innerClone(obj2[i])
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])
                                        , Expression.Convert(Expression.Call(localObj, type.GetMethod("get_Item"), new[] { localVar_i }), typeof(object))//(object)obj2[i]
                                        , para_dic
                ),
                                    reflect.GenericTypes[0].type)
                            ),
                             Expression.PostIncrementAssign(localVar_i)//i++;
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, countVar, localRes, localVar_i },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignCount,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_ValueTuple
            void GetCloneMethod_ValueTuple(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                if (reflect.GenericTypes.All(i => JudgeSimple(i.type)))
                {
                    wrapper.Method = (obj, dic) => obj;
                    return;
                }
                var valueProp = Expression.Variable(type, "p");
                var para_obj = Expression.Parameter(typeof(object), "i");
                var para_dic = Expression.Parameter(typeof(CacheDictionary), "dic");
                var tmpExp = Expression.Convert(para_obj, type);

                var args = new List<Expression>();
                for (int i = 0; i < reflect.GenericTypes.Count; i++)
                {
                    var valueAccess = Expression.MakeMemberAccess(tmpExp, type.GetMember("Item" + (i + 1))[0]);
                    var geneType = reflect.GenericTypes[i].type;
                    if (JudgeSimple(geneType))
                    {
                        args.Add(valueAccess);
                    }
                    else
                    {
                        var cloneMethod = GetCloneMethod(geneType, tmpCache);
                        args.Add(Expression.Convert(Expression.Invoke(Expression.MakeMemberAccess(Expression.Constant(cloneMethod), typeof(Wrapper).GetMember("Method")[0]), valueAccess, para_dic), geneType));
                    }
                }

                var newExp = Expression.New(type.GetConstructor(reflect.GenericTypes.Select(i => i.type).ToArray()),
                    args.ToArray());
                var convertExp = Expression.Convert(newExp, typeof(object));
                var tmp = Expression.Lambda<Func<object, CacheDictionary, object>>(convertExp, para_obj, para_dic);
                wrapper.Method = tmp.Compile();
            }
            #endregion

            #region GetCloneMethod_Pojo
            void GetCloneMethod_Pojo(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();

                var props = type.GetProperties().Where(i => i.CanWrite).ToList();
                var publicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
                var propSets = new List<BinaryExpression>();

                var tmp = Expression.Variable(type, "tmp");//Person tmp
                var assign = Expression.Assign(tmp, Expression.TypeAs(para_obj, type));//tmp= obj as Person;
                var retVar = Expression.Variable(type, "res");//Person res;
                var assignRes = Expression.Assign(retVar, Expression.New(type.GetConstructor(new Type[0])));//res=new Person()
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, retVar);//dic.Add(i,res)
                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i];
                    var flag = false;
                    var propType = prop.PropertyType;
                    if (JudgeSimple(propType))
                    {
                        propSets.Add(Expression.Assign(Expression.Property(retVar, prop), Expression.MakeMemberAccess(tmp, prop)));
                        flag = true;
                    }
                    if (!flag)
                    {
                        //其他的
                        propSets.Add(Expression.Assign(Expression.Property(retVar, prop), Expression.Convert(Expression.Invoke(Expression.MakeMemberAccess(Expression.Constant(GetCloneMethod(prop.PropertyType, tmpCache)), typeof(Wrapper).GetMember("Method")[0]), Expression.Convert(Expression.MakeMemberAccess(tmp, prop), typeof(object)), para_dic), propType)));
                        flag = true;
                    }
                }
                for (int i = 0; i < publicFields.Count; i++)
                {
                    var field = publicFields[i];
                    var flag = false;
                    var propType = field.FieldType;
                    if (JudgeSimple(propType))
                    {
                        propSets.Add(Expression.Assign(Expression.Field(retVar, field), Expression.MakeMemberAccess(tmp, field)));
                        flag = true;
                    }
                    if (!flag)
                    {
                        //其他的
                        propSets.Add(Expression.Assign(Expression.Field(retVar, field), Expression.Convert(Expression.Invoke(Expression.MakeMemberAccess(Expression.Constant(GetCloneMethod(field.FieldType, tmpCache)), typeof(Wrapper).GetMember("Method")[0]), Expression.Convert(Expression.MakeMemberAccess(tmp, field), typeof(object)), para_dic), propType)));
                        flag = true;
                    }
                }

                var exps = new List<Expression>() { ifnullExp, ifCacheExp, assign, assignRes, addDic };
                exps.AddRange(propSets);
                exps.Add(Expression.Goto(retLabel, retVar));
                exps.Add(retExp);

                var block = Expression.Block(new[] { tmp, retVar }, exps.ToArray());
                var memberInit = Expression.Lambda<Func<object, CacheDictionary, object>>(block, new ParameterExpression[] { para_obj, para_dic });
                wrapper.Method = memberInit.Compile();
            }
            #endregion

            #region GetCloneMethod_Array
            void GetCloneMethod_Array(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();

                var eleType = type.GetElementType();
                if (JudgeSimple(eleType))
                {
                    var tmpRes = Expression.Variable(type, "res");//var res=int[];
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.Call(null, typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(eleType), Expression.TypeAs(para_obj, type)));//res=(obj as int[]).ToArray()
                    var tmpAddDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, tmpRes);//dic.Add(i,res);
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, tmpAddDic, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //Person[] obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as Person[];
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));
                //int count;
                var countVar = Expression.Variable(typeof(int), "count");
                var countProp = Expression.Property(localObj, "Length");
                //count=obj2.Count;
                var assignCount = Expression.Assign(countVar, countProp);

                //Person[] res;
                var localRes = Expression.Variable(type, "res");

                //new Person[count]
                var newArrExp = Expression.NewArrayBounds(eleType, countVar);
                //var newListExp = Expression.New(type.GetConstructor(new Type[] { typeof(int) }), new Expression[] { countVar });
                //res=new Person[count];
                var assignRes = Expression.Assign(localRes, newArrExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);


                var innerClone = GetCloneMethod(eleType, tmpCache);

                //for(var i=0;i<count;i++)
                //int i;
                var localVar_i = Expression.Variable(typeof(int), "i");
                //i=0;
                var loopInit = Expression.Assign(localVar_i, Expression.Constant(0));//i=0
                var breakLabel = Expression.Label("break");

                //res.Add(innerClone(obj2[i]))
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.LessThan(localVar_i, countVar),//if(i<count)
                        Expression.Block(
                            Expression.Assign(Expression.ArrayAccess(localRes, localVar_i),
                                   Expression.Convert(Expression.Invoke(//innerClone(obj2[i])
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.ArrayIndex(localObj, localVar_i), typeof(object))//(object)obj2[i]
                                        , para_dic
                                    ), eleType)
                            ),
                             Expression.PostIncrementAssign(localVar_i)//i++;
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);

                var block = Expression.Block(
                    new ParameterExpression[] { localObj, countVar, localRes, localVar_i },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignCount,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_Dictionary
            void GetCloneMethod_Dictionary(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type) && JudgeSimple(reflect.GenericTypes.LastOrDefault().type))
                {
                    var tmpRes = Expression.Variable(type, "res");//var res=Dictionary<int,string>;
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.New(
                        type.GetConstructor(new[] {
                            typeof(IDictionary<,>).MakeGenericType(new[] { reflect.GenericTypes.FirstOrDefault().type, reflect.GenericTypes.LastOrDefault().type }),
                            typeof(IEqualityComparer<>).MakeGenericType(new[] { reflect.GenericTypes.FirstOrDefault().type }) }
                        ),
                        Expression.TypeAs(para_obj, type),
                        Expression.Property(Expression.TypeAs(para_obj, type), "Comparer")));//res= new Dictionary<int,string>(obj as Dictionary<int,string>,(obj as Dictionary<int,string>).Comparer)
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //Dictionary<int,Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as Dictionary<int,Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));
                //int count;
                var countVar = Expression.Variable(typeof(int), "count");
                var countProp = Expression.Property(localObj, "Count");
                //count=obj2.Count;
                var assignCount = Expression.Assign(countVar, countProp);

                //Dictionary<int,Person> res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(type.GetConstructor(new[] {
                    typeof(int),
                    typeof(IEqualityComparer<>).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type) }), new Expression[] { countVar, Expression.Property(localObj, "Comparer") });
                //res=new Dictionary<int,Person>(obj2.Count,obj2.Comparer)
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                //List<int> keys;
                var local_keys = Expression.Variable(typeof(List<>).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type), "keys");
                //keys=obj2.Keys.ToList();
                var keysAssign = Expression.Assign(local_keys, Expression.Call(typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(reflect.GenericTypes.FirstOrDefault().type), Expression.Property(localObj, "Keys")));


                Wrapper keyClone = null, valueClone = null;
                if (!JudgeSimple(reflect.GenericTypes.FirstOrDefault().type)) keyClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);
                if (!JudgeSimple(reflect.GenericTypes.LastOrDefault().type)) valueClone = GetCloneMethod(reflect.GenericTypes.LastOrDefault().type, tmpCache);

                //for(var i=0;i<count;i++)
                //int i;
                var localVar_i = Expression.Variable(typeof(int), "i");
                //i=0;
                var loopInit = Expression.Assign(localVar_i, Expression.Constant(0));//i=0
                var breakLabel = Expression.Label("break");

                //int key;
                var local_key = Expression.Variable(reflect.GenericTypes.FirstOrDefault().type, "key");
                var assignKey = Expression.Assign(local_key, Expression.Call(local_keys, typeof(List<>).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type).GetMethod("get_Item"), new[] { localVar_i }));
                var cloneKey = keyClone == null ? (local_key as Expression) :
                    Expression.TypeAs(Expression.Invoke(//keyClone(key,dic)
                                        Expression.MakeMemberAccess(Expression.Constant(keyClone), typeof(Wrapper).GetMember("Method")[0])
                                        , local_key
                                        , para_dic), reflect.GenericTypes.FirstOrDefault().type);

                var cloneValue = valueClone == null ? (Expression.Call(localObj, type.GetMethod("get_Item"), local_key) as Expression) :
                   Expression.TypeAs(Expression.Invoke(//valueClone(obj2[key],dic)
                                        Expression.MakeMemberAccess(Expression.Constant(valueClone), typeof(Wrapper).GetMember("Method")[0])
                                        , Expression.Call(localObj, type.GetMethod("get_Item"), local_key)
                                        , para_dic), reflect.GenericTypes.LastOrDefault().type);

                //res.Add(innerClone(obj2[i]))
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.LessThan(localVar_i, countVar),//if(i<count)
                        Expression.Block(
                            assignKey,
                            Expression.Call(//res.Add(key,value)
                localRes
                                , type.GetMethod("Add", new Type[] { reflect.GenericTypes[0].type, reflect.GenericTypes[1].type })
                                , cloneKey
                                , cloneValue
                            ),
                             Expression.PostIncrementAssign(localVar_i)//i++;
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, countVar, localRes, localVar_i, local_keys, local_key },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignCount,
                    keysAssign,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_HashSet
            void GetCloneMethod_HashSet(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type) && JudgeSimple(reflect.GenericTypes.LastOrDefault().type))
                {
                    var tmpRes = Expression.Variable(type, "res");//HaseSet<int> res;
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.New(
                        type.GetConstructor(new[] {
                            typeof(HashSet<>).MakeGenericType(new[] { reflect.GenericTypes.FirstOrDefault().type}),
                            typeof(IEqualityComparer<>).MakeGenericType(new[] { reflect.GenericTypes.FirstOrDefault().type }) }
                        ),
                        Expression.TypeAs(para_obj, type),
                        Expression.Property(Expression.TypeAs(para_obj, type), "Comparer")));//res= new HaseSet<int>(obj as HaseSet<int>,(obj as HaseSet<int>).Comparer)
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //HaseSet<Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as HaseSet<Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));

                //int count;
                var countVar = Expression.Variable(typeof(int), "count");
                var countProp = Expression.Property(localObj, "Count");
                //count=obj2.Count;
                var assignCount = Expression.Assign(countVar, countProp);

                var ratorType = typeof(HashSet<>.Enumerator).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type);
                //HashSet<Person>.Enumerator rator;
                var ratorVar = Expression.Variable(ratorType, "rator");
                //rator=obj2.GetEnumerator();
                var assignRator = Expression.Assign(ratorVar, Expression.Call(localObj, type.GetMethod("GetEnumerator")));

                //HashSet<Person> res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(type.GetConstructor(new[] {
                    typeof(int),
                    typeof(IEqualityComparer<>).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type) }), new Expression[] { countVar, Expression.Property(localObj, "Comparer") });
                //res=new HashSet<Person>(obj2.Count,obj2.Comparer)
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                var breakLabel = Expression.Label("break");

                var innerClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);

                //while (rator.MoveNext())
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.IsTrue(Expression.Call(ratorVar, ratorType.GetMethod("MoveNext"))),
                        Expression.Block(
                            Expression.Call(//res.Add(innerClone(rator.Current))
                localRes
                                , type.GetMethod("Add", new Type[] { reflect.GenericTypes[0].type })
                                , Expression.Convert(Expression.Invoke(//innerClone(rator.Current)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(ratorVar, "Current"), typeof(object))//(object)rator.Current
                                        , para_dic
                                    ), reflect.GenericTypes.FirstOrDefault().type)
                            )
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, countVar, localRes, ratorVar },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignCount,
                    assignRator,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_LinkedList
            void GetCloneMethod_LinkedList(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type) && JudgeSimple(reflect.GenericTypes.LastOrDefault().type))
                {
                    var tmpRes = Expression.Variable(type, "res");//LinkedList<int> res;
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.New(
                        type.GetConstructor(new[] {
                            typeof(IEnumerable<>).MakeGenericType(new[] { reflect.GenericTypes.FirstOrDefault().type}) }
                        ),
                        Expression.TypeAs(para_obj, type)));//res= new LinkedList<int>(obj as LinkedList<int>)
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //LinkedList<Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as LinkedList<Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));

                var ratorType = typeof(LinkedList<>.Enumerator).MakeGenericType(reflect.GenericTypes.FirstOrDefault().type);
                //LinkedList<Person>.Enumerator rator;
                var ratorVar = Expression.Variable(ratorType, "rator");
                //rator=obj2.GetEnumerator();
                var assignRator = Expression.Assign(ratorVar, Expression.Call(localObj, type.GetMethod("GetEnumerator")));

                //LinkedList<Person> res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(type.GetConstructor(new Type[0]));
                //res=new LinkedList<Person>()
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                var breakLabel = Expression.Label("break");

                var innerClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);

                //while (rator.MoveNext())
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.IsTrue(Expression.Call(ratorVar, ratorType.GetMethod("MoveNext"))),
                        Expression.Block(
                            Expression.Call(//res.AddLast(innerClone(rator.Current))
                localRes
                                , type.GetMethod("AddLast", new Type[] { reflect.GenericTypes[0].type })
                                , Expression.Convert(Expression.Invoke(//innerClone(rator.Current)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(ratorVar, "Current"), typeof(object))//(object)rator.Current
                                        , para_dic
                                    ), reflect.GenericTypes.FirstOrDefault().type)
                            )
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, localRes, ratorVar },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignRator,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_Anonymous
            void GetCloneMethod_Anonymous(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type) && JudgeSimple(reflect.GenericTypes.LastOrDefault().type))
                {
                    wrapper.Method = (para_obj, para_dic) => para_obj;
                    return;
                }

                //{Id,Name} obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as {Id,Name};
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));

                var ctor = type.GetConstructor(reflect.GenericTypes.Select(i => i.type).ToArray());
                var paras = new List<Expression>();
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < reflect.GenericTypes.Count; i++)
                {
                    var propType = reflect.GenericTypes[i].type;
                    if (JudgeSimple(propType)) paras.Add(Expression.Property(localObj, props[i].Name));
                    else
                    {
                        var innerClone = GetCloneMethod(propType, tmpCache);
                        paras.Add(Expression.Convert(Expression.Invoke(//innerClone(obj2.Person)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(localObj, props[i].Name), typeof(object)) //(object)obj2.Person
                                        , para_dic
                                    ), propType));
                    }
                }

                //{Id,Name,Person} res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(ctor, paras.ToArray());
                //res=new Anonymous(obj2.Id,obj2.Name,innerClone(obj2.Person)
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);


                var block = Expression.Block(
                    new ParameterExpression[] { localObj, localRes },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignRes,
                    addDic,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_Tuple
            void GetCloneMethod_Tuple(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();
                //simple
                if (JudgeSimple(reflect.GenericTypes.FirstOrDefault().type) && JudgeSimple(reflect.GenericTypes.LastOrDefault().type))
                {
                    wrapper.Method = (para_obj, para_dic) => para_obj;
                    return;
                }

                //Tuple<int,Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as Tuple<int,Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));

                var ctor = type.GetConstructor(reflect.GenericTypes.Select(i => i.type).ToArray());
                var paras = new List<Expression>();
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < reflect.GenericTypes.Count; i++)
                {
                    var propType = reflect.GenericTypes[i].type;
                    if (JudgeSimple(propType)) paras.Add(Expression.Property(localObj, props[i].Name));
                    else
                    {
                        var innerClone = GetCloneMethod(propType, tmpCache);
                        paras.Add(Expression.Convert(Expression.Invoke(//innerClone(obj2.Person)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(localObj, props[i].Name), typeof(object))//(object)obj2.Person
                                        , para_dic
                                    ), propType));
                    }
                }

                //{Id,Name,Person} res;
                var localRes = Expression.Variable(type, "res");
                var newListExp = Expression.New(ctor, paras.ToArray());
                //res=new Anonymous(obj2.Id,obj2.Name,innerClone(obj2.Person)
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);


                var block = Expression.Block(
                    new ParameterExpression[] { localObj, localRes },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignRes,
                    addDic,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_IEnumerable
            void GetCloneMethod_IEnumerable(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();

                var eleType = reflect.GenericTypes.FirstOrDefault().type;
                var finalType = typeof(List<>).MakeGenericType(eleType);
                var ienumerableType = typeof(IEnumerable<>).MakeGenericType(eleType);
                if (JudgeSimple(eleType))
                {
                    var tmpRes = Expression.Variable(finalType, "res");//List<int> res;
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.Call(null, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(eleType), Expression.TypeAs(para_obj, type)));//res=(obj as IEnumerable<int>).List()
                    var tmpAddDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, tmpRes);//dic.Add(i,res);
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes }, ifnullExp, ifCacheExp, tmpAssignRes, tmpAddDic, Expression.Goto(retLabel, tmpRes), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //IEnumerable<Person> obj2;
                var localObj = Expression.Parameter(ienumerableType, "obj2");
                //obj2=obj as IEnumerable<Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, ienumerableType));

                var ratorType = typeof(IEnumerator);
                //IEnumerator<Person> rator;
                var ratorVar = Expression.Variable(ratorType, "rator");
                //rator=obj2.GetEnumerator();
                var assignRator = Expression.Assign(ratorVar, Expression.Call(localObj, typeof(IEnumerable<>).MakeGenericType(eleType).GetMethod("GetEnumerator")));

                //List<Person> res;
                var localRes = Expression.Variable(finalType, "res");
                var newListExp = Expression.New(finalType.GetConstructor(new Type[0]));
                //res=new List<Person>()
                var assignRes = Expression.Assign(localRes, newListExp);
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                var breakLabel = Expression.Label("break");

                var innerClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);

                //while (rator.MoveNext())
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.IsTrue(Expression.Call(ratorVar, ratorType.GetMethod("MoveNext"))),
                        Expression.Block(
                            Expression.Call(//res.AddLast(innerClone(rator.Current))
                localRes
                                , finalType.GetMethod("Add", new Type[] { reflect.GenericTypes[0].type })
                                , Expression.Convert(Expression.Invoke(//innerClone(rator.Current)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(ratorVar, "Current"), typeof(object))//(object)rator.Current
                                        , para_dic
                                    ), reflect.GenericTypes.FirstOrDefault().type)
                            )
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, localRes, ratorVar },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignRator,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_ReadOnlyCollection
            void GetCloneMethod_ReadOnlyCollection(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();

                var eleType = reflect.GenericTypes.FirstOrDefault().type;
                if (JudgeSimple(eleType))
                {
                    var tmpRes = Expression.Variable(typeof(List<>).MakeGenericType(eleType), "tmpRes");//List<int> res;
                    var tmpAssignRes = Expression.Assign(tmpRes, Expression.Call(null, typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(eleType), Expression.TypeAs(para_obj, type)));//res=(obj as ReadOnlyCollection<int>).List()
                    var tmpReadonly = Expression.New(type.GetConstructor(new[] { typeof(IList<>).MakeGenericType(eleType) }), tmpRes);
                    var res = Expression.Variable(type, "res");//ReadOnlyCollection<int> res;
                    var finalAssign = Expression.Assign(res, tmpReadonly);
                    var tmpAddDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, res);//dic.Add(i,res);
                    var tmpBlock = Expression.Block(new ParameterExpression[] { tmpRes, res }, ifnullExp, ifCacheExp, tmpAssignRes, finalAssign, tmpAddDic, Expression.Goto(retLabel, res), retExp);
                    var lambda = Expression.Lambda<Func<object, CacheDictionary, object>>(tmpBlock, para_obj, para_dic);
                    wrapper.Method = lambda.Compile();
                    return;
                }

                //var ienumerableType = typeof(IEnumerable<>).MakeGenericType(eleType);
                var listType = typeof(List<>).MakeGenericType(eleType);
                //ReadOnlyCollection<Person> obj2;
                var localObj = Expression.Parameter(type, "obj2");
                //obj2=obj as IEnumerable<Person>;
                var assignLocalObj = Expression.Assign(localObj, Expression.TypeAs(para_obj, type));

                var ratorType = typeof(IEnumerator);
                //IEnumerator<Person> rator;
                var ratorVar = Expression.Variable(ratorType, "rator");
                //rator=obj2.GetEnumerator();
                var assignRator = Expression.Assign(ratorVar, Expression.Call(localObj, typeof(IEnumerable<>).MakeGenericType(eleType).GetMethod("GetEnumerator")));

                //List<Person> tmpRes;
                var localTmpRes = Expression.Variable(listType, "tmpRes");
                var newListExp = Expression.New(listType.GetConstructor(new[] { typeof(int) }), Expression.Property(localObj, "Count"));
                //res=new List<Person>(obj.Count)
                var assignTmpRes = Expression.Assign(localTmpRes, newListExp);
                var localRes = Expression.Variable(type, "res");
                var assignRes = Expression.Assign(localRes, Expression.New(type.GetConstructor(new[] { typeof(IList<>).MakeGenericType(eleType) }), localTmpRes));
                var addDic = Expression.Call(para_dic, typeof(CacheDictionary).GetMethod("set_Item"), para_obj, localRes);

                var breakLabel = Expression.Label("break");

                var innerClone = GetCloneMethod(reflect.GenericTypes.FirstOrDefault().type, tmpCache);

                //while (rator.MoveNext())
                var loopBody = Expression.Block(
                    Expression.IfThenElse(Expression.IsTrue(Expression.Call(ratorVar, ratorType.GetMethod("MoveNext"))),
                        Expression.Block(
                            Expression.Call(//tmpRes.Add(innerClone(rator.Current))
                localTmpRes
                                , listType.GetMethod("Add", new Type[] { reflect.GenericTypes[0].type })
                                , Expression.Convert(Expression.Invoke(//innerClone(rator.Current)
                                        Expression.MakeMemberAccess(Expression.Constant(innerClone), typeof(Wrapper).GetMember("Method")[0])//wrapper.Method.Invoke()
                                        , Expression.Convert(Expression.Property(ratorVar, "Current"), typeof(object)) //(object)rator.Current
                                        , para_dic
                                    ), reflect.GenericTypes.FirstOrDefault().type)
                            )
                        )
                        , Expression.Goto(breakLabel))
                );
                var loopExp = Expression.Loop(loopBody, breakLabel);
                var block = Expression.Block(
                    new ParameterExpression[] { localObj, localRes, ratorVar, localTmpRes },
                    ifnullExp,
                    ifCacheExp,
                    assignLocalObj,
                    assignRator,
                    assignTmpRes,
                    assignRes,
                    addDic,
                    loopExp,
                    Expression.Goto(retLabel, localRes),
                    retExp
                    );
                var finalExp = Expression.Lambda<Func<object, CacheDictionary, object>>(block, para_obj, para_dic);
                wrapper.Method = finalExp.Compile();
            }
            #endregion

            #region GetCloneMethod_Struct
            void GetCloneMethod_Struct(Type type, (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) reflect, Wrapper wrapper, Dictionary<Type, Wrapper> tmpCache)
            {
                var (para_obj, para_dic, retLabel, retExp, ifnullExp, ifCacheExp) = GetCommonExp();

                var props = type.GetProperties().Where(i => i.CanWrite).ToList();
                if (props.All(i => JudgeSimple(i.PropertyType)))
                {
                    wrapper.Method = (obj, cache) => obj;
                    return;
                }
                var res = Expression.Variable(type, "res");//MyStruct res;
                var assignRes = Expression.Assign(res, Expression.Convert(para_obj, type));
                var propSets = new List<BinaryExpression>();
                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i];
                    var flag = false;
                    var propType = prop.PropertyType;
                    if (JudgeSimple(propType))
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        //其他的
                        propSets.Add(Expression.Assign(Expression.Property(res, prop), Expression.Convert(Expression.Invoke(Expression.MakeMemberAccess(Expression.Constant(GetCloneMethod(prop.PropertyType, tmpCache)), typeof(Wrapper).GetMember("Method")[0]), Expression.Convert(Expression.MakeMemberAccess(res, prop), typeof(object)), para_dic), propType)));
                        flag = true;
                    }
                }

                var exps = new List<Expression>() { assignRes };
                exps.AddRange(propSets);
                exps.Add(Expression.Convert(res, typeof(object)));

                var block = Expression.Block(new[] { res }, exps.ToArray());
                var memberInit = Expression.Lambda<Func<object, CacheDictionary, object>>(block, new ParameterExpression[] { para_obj, para_dic });
                wrapper.Method = memberInit.Compile();
            }
            #endregion
            #endregion

        }

        /// <summary>
        /// 注册指定 Type 的克隆逻辑
        /// </summary>
        /// <param name="type"></param>
        /// <param name="func"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal static void RegisterCloneHander(Type type, Func<object, CacheDictionary, object> func)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (func == null) throw new ArgumentNullException("func");
            _ = _cache.TryAdd(type, new Wrapper
            {
                Method = func
            });
        }
    }

    /// <summary>
    /// 扩展类
    /// </summary>
    internal static class CloneExtensions
    {
        private static Dictionary<Type, string> shortNames2 = new()
        {
            { typeof(byte),"byte"},{ typeof(byte[]),"byte[]"},
            { typeof(sbyte),"sbyte"},{ typeof(sbyte[]),"sbyte[]"},
            { typeof(short),"short"},{ typeof(short[]),"short[]"},
            { typeof(ushort),"ushort"},{ typeof(ushort[]),"ushort[]"},
            { typeof(int),"int"},{ typeof(int[]),"int[]"},
            { typeof(uint),"uint"},{ typeof(uint[]),"uint[]"},
            { typeof(long),"long"},{ typeof(long[]),"long[]"},
            { typeof(ulong),"ulong"},{ typeof(ulong[]),"ulong[]"},
            { typeof(float),"float"},{ typeof(float[]),"float[]"},
            { typeof(double),"double"},{ typeof(double[]),"double[]"},
            { typeof(decimal),"decimal"},{ typeof(decimal[]),"decimal[]"},
            { typeof(char),"char"},{ typeof(char[]),"char[]"},
            { typeof(string),"string"},{ typeof(string[]),"string[]"},
            { typeof(bool),"bool"},{ typeof(bool[]),"bool[]"},
            { typeof(void),"void"},
        };

        private static ConcurrentDictionary<(Type type, bool containsOutOrIn), (string name, List<(string genetypeName, bool isGenericParameter, Type geneType)> genericParameters)> classNamesCache2 = new();

        /// <summary>
        /// 获取当前类型的泛型定义名称以及具化的泛型参数数组,如: 
        /// <list type="number">
        /// <item>typeof(List&lt;>) => (System.Collections.Generic.List&lt;T>,[("T", true, typeof(T))])</item>
        /// <item>typeof(List&lt;Person>) => (System.Collections.Generic.List&lt;T>,[("T", false, typeof(Person))])</item>
        /// </list>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="containsOutOrIn">
        /// <list type="bullet">
        /// <item>为true时,输出: System.Collections.Generic.IEnumerable&lt;out T></item>
        /// <item>为false时,输出: System.Collections.Generic.IEnumerable&lt;T></item>
        /// </list>
        /// </param>
        /// <returns></returns>
        internal static (string Name, List<(string name, bool isGeneric, Type type)> GenericTypes) GetClassGenericFullName(this Type type, bool containsOutOrIn = false)
        {
            if (type == null) return (null, null);
            return classNamesCache2.GetOrAdd((type, containsOutOrIn), key =>
            {
                var type = key.type;
                var containsOutOrIn = key.containsOutOrIn;
                if (shortNames2.ContainsKey(type)) return (shortNames2[type], new List<(string, bool, Type)>());
                if (type.IsGenericParameter) return (type.Name, new List<(string, bool, Type)>());
                if (type.IsArray)
                {
                    var tmp = GetClassGenericFullName(type.GetElementType());
                    tmp.Name += "[]";
                    return tmp;
                }
                var types = type.GetGenericArguments().ToList();
                var parents = new List<Type>();
                //获取所有父类型
                GetParents(type);
                void GetParents(Type type)
                {
                    parents.Add(type);
                    if (type.DeclaringType != null)
                    {
                        GetParents(type.DeclaringType);
                    }
                }

                parents.Reverse();
                //
                var geneParaArr = type.GenericTypeArguments.ToList();
                var names = new List<string>();
                var counter = 0;
                var map = new List<(string, bool, Type)>();
                for (var i = 0; i < parents.Count; i++)
                {
                    var parent = parents[i];
                    var name = "";
                    if (shortNames2.ContainsKey(parent))
                    {
                        name = shortNames2[parent];
                        names.Add(name);
                        continue;
                    }
                    else
                    {
                        name = (i == 0 ? parent.Namespace + "." : "") + parent.Name.Split("`").FirstOrDefault();
                    }

                    if (parent.IsGenericType)
                    {
                        var defs = parent.GetGenericTypeDefinition().GetGenericArguments();
                        var geneTypes = string.Join(", ", defs.Skip(counter).Select(i =>
                        {
                            if (!containsOutOrIn) return i.Name;
                            var k = i.GenericParameterAttributes;
                            if (k == GenericParameterAttributes.Contravariant) return $"in {i.Name}";
                            if (k == GenericParameterAttributes.Covariant) return $"out {i.Name}";
                            return i.Name;
                        }));
                        defs.Skip(counter).ToList().ForEach(k =>
                        {
                            if (geneParaArr.Count > 0)
                            {
                                map.Add((k.Name, false, geneParaArr[0]));
                                geneParaArr.RemoveAt(0);
                            }
                            else
                            {
                                map.Add((k.Name, true, k));
                            }
                        });
                        if (geneTypes.Length > 0) name += "<" + geneTypes + ">";
                        counter = defs.Length;
                    }
                    names.Add(name);
                }
                return (string.Join(".", names), map);
            });
        }
    }
}