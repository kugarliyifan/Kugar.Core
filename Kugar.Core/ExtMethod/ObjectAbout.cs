using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Kugar.Core.ExtMethod
{
    public static class ObjectAbout
    {
        public static bool SafeEquals(this object x, object y)
        {
            if (x != null)
            {
                return x.Equals(y);
            }
            else if (y != null)
            {
                return  y.Equals(x);
            }
            else
            {
                return true;
            }
        }

        public static void Swap<T>(ref T first, ref T second)
        {
            //if (first.GetType()!=second.GetType())
            //{
            //    throw new Exception("不能交换不同类型的对象");
            //}

            T temp = first;

            first = second;

            second = temp;
        }

        [Obsolete]
        public static bool IsNum(this object str)
        {
            if (str == null)
            {
                return false;
            }

            decimal temp;

            return decimal.TryParse(str.ToString(), out temp);

        }


        public static int ToInt32(this object obj)
        {
            return ToInt(obj, 0);
        }

        public static int ToInt32(this object obj, int defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is int)
            {
                return (int)obj;
            }

            return StringAbout.ToInt(obj.ToString(), defaultvalue);

        }


        public static int ToInt(this object obj)
        {
            return ToInt(obj, 0);
        }

        public static int ToInt(this object obj, int defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is int)
            {
                return (int)obj;
            }

            return StringAbout.ToInt(obj.ToString(), defaultvalue);


            //int tempindex = -1;
            //tempindex = str.ToString().IndexOf('.');

            //var strvalue = str.ToString();

            //if (tempindex!=-1)
            //{
            //    strvalue = strvalue.Substring(tempindex - 1, tempindex);
            //}

            //if (int.TryParse(strvalue, out t))
            //{
            //    return t;
            //}
            //else
            //{
            //    return defaultvalue;
            //}
        }

        public static long ToLong(this object src, long defaultValue = 0)
        {
            long value;

            if (long.TryParse(src.ToStringEx(), out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }


        public static int ToMinInt(this object obj, int minvalue)
        {
            int i = ToInt(obj);
            if (i < minvalue)
            {
                return minvalue;
            }

            return i;
        }


        public static byte ToByte(this object obj)
        {
            return ToByte(obj, 0);
        }

        public static byte ToByte(this object obj, byte defaultvalue)
        {
            if (obj is byte)
            {
                return (byte)obj;
            }

            return StringAbout.ToByte(obj.ToString(), defaultvalue);
        }


        public static double ToDouble(this object obj)
        {
            return ToDouble(obj, 0);
        }

        public static double ToDouble(this object obj, double defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is double)
            {
                return (double)obj;
            }

            return StringAbout.ToDouble(obj.ToString(), defaultvalue);
        }



        public static decimal ToDecimal(this object obj)
        {
            return ToDecimal(obj, 0);
        }

        public static decimal ToDecimal(this object obj, decimal defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is decimal)
            {
                return (decimal)obj;
            }

            return StringAbout.ToDecimal(obj.ToString(), defaultvalue);
        }



        //Bool粘贴
        public static bool ToBool(this object obj)
        {
            return ToBool(obj, false);
        }

        public static bool ToBool(this object obj, bool defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is bool)
            {
                return (bool)obj;
            }

            return StringAbout.ToBool(obj.ToString(), defaultvalue);
        }



        public static string ToStringEx(this object obj)
        {
            return ToStringEx(obj, string.Empty);
        }

        public static string ToStringEx(this object obj, string defaultvalue)
        {
            if (obj == null || obj is DBNull)
            {
                return defaultvalue;
            }

            return obj.ToString();
        }


        /// <summary>
        ///     转换到float型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float ToFloat(this object obj)
        {
            return ToFloat(obj, 0);
        }

        public static float ToFloat(this object obj, float defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is float)
            {
                return (float)obj;
            }

            return StringAbout.ToFloat(obj.ToString(), defaultvalue);
        }


        public static DateTime ToDateTime(this object str)
        {
            return ToDateTime(str, DateTime.MinValue);
        }

        public static DateTime ToDateTime(this object str, DateTime defaultvalue)
        {
            DateTime d;

            if (DateTime.TryParse(str.ToString(), out d))
            {
                return d;
            }
            else
            {
                return defaultvalue;
            }

        }


        /// <summary>
        ///     转换到float型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(this object str)
        {
            return ToUInt16(str, 0);
        }

        public static UInt16 ToUInt16(this object str, UInt16 defaultvalue)
        {
            UInt16 t;

            if (UInt16.TryParse(str.ToStringEx(""), out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        public static short ToInt16(this object str, short defaultvalue=0)
        {
            short t;

            if (short.TryParse(str.ToStringEx(""), out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }



        /// <summary>
        /// 使用链式函数调用的方式模拟switch关键字和case,最后读取Result做为最后数据输出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="defalutValue">默认值</param>
        /// <returns></returns>
        public static SwithStruct<TInput, TOutput> Switch<TInput, TOutput>(this TInput src, TOutput defalutValue =default(TOutput))
        {
            return new SwithStruct<TInput, TOutput>(src, defalutValue);
        }

        /// <summary>
        /// 使用链式函数调用的方式模拟if关键字和else,最后读取Result做为最后数据输出
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="src"></param>
        /// <param name="predicateExp">第一个if的表达式</param>
        /// <param name="outputValue"></param>
        /// <returns></returns>
        public static IfStruct<TInput, TOutput> If<TInput, TOutput>(this TInput src, Predicate<TInput> predicateExp,
            TOutput outputValue=default(TOutput) )
        {
            return new IfStruct<TInput, TOutput>(src,predicateExp,outputValue);
        }

        /// <summary>
        /// 使用链式函数调用的方式模拟if关键字和else,最后读取Result做为最后数据输出
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="src"></param>
        /// <param name="predicateExp">第一个if的表达式</param>
        /// <param name="outputValue"></param>
        /// <returns></returns>
        public static IfStruct<TInput, TOutput> If<TInput, TOutput>(this TInput src, Predicate<TInput> predicateExp,
            Func<TInput,TOutput> outputValue )
        {
            return new IfStruct<TInput, TOutput>(src, predicateExp, outputValue(src));
        }

        /// <summary>
        /// 根据src的值,判断,输出不同的结果
        /// </summary>
        /// <typeparam name="TResult">输出的结果</typeparam>
        /// <param name="src">用于判断的值</param>
        /// <param name="ifTrueOut">如果为true,输出该参数的值</param>
        /// <param name="ifFalseOut">如果为false,输出该参数的值</param>
        /// <returns></returns>
        public static TResult If<TResult>(this bool src, TResult ifTrueOut, TResult ifFalseOut=default(TResult))
        {
            if (src)
            {
                return ifTrueOut;
            }
            else
            {
                return ifFalseOut;
            }
        }

        /// <summary>
        /// 根据src的值,判断,输出不同的结果
        /// </summary>
        /// <typeparam name="TResult">输出的结果</typeparam>
        /// <param name="src">用于判断的值</param>
        /// <param name="checker">用于检查的函数</param>
        /// <param name="ifTrueOut">如果为true,输出该参数的值</param>
        /// <param name="ifFalseOut">如果为false,输出该参数的值</param>
        /// <returns></returns>
        public static TResult If<TInput, TResult>(this TInput src, Predicate<TInput> checker, TResult ifTrueOut, TResult ifFalseOut = default(TResult))
        {
            if (checker(src))
            {
                return ifTrueOut;
            }
            else
            {
                return ifFalseOut;
            }
        }


        /// <summary>
        /// 根据src的值,判断,输出不同的结果
        /// </summary>
        /// <typeparam name="TResult">输出的结果</typeparam>
        /// <param name="src">用于判断的值</param>
        /// <param name="ifTrueOut">如果为true,输出该参数的值</param>
        /// <param name="ifFalseOut">如果为false,输出该参数的值</param>
        /// <param name="ifNullOut">如果为null,输出该参数的值</param>
        /// <returns></returns>
        public static TResult If<TResult>(this bool? src, TResult ifTrueOut, TResult ifFalseOut= default(TResult), TResult ifNullOut=default(TResult))
        {
            if (src==true)
            {
                return ifTrueOut;
            }
            else if (src == false)
            {
                return ifFalseOut;
            }
            else
            {
                return ifNullOut;
            }
        }



        #region "转换可空类型"

        public static int? ToIntNullable(this object obj)
        {
            return ToIntNullable(obj, null, 10);
        }

        public static int? ToIntNullable(this object obj, int? defaultvalue)
        {
            return ToIntNullable(obj, defaultvalue, 10);
        }

        public static int? ToIntNullable(this object obj, int? defaultvalue, int jinzhi)
        {
            if (obj==null)
            {
                return defaultvalue;
            }
                
            
            if (obj is int? || obj is int)
            {
                return (int?)obj;
            }

            return obj.ToString().ToIntNullable(defaultvalue, jinzhi);
        }

 

        public static byte? ToByteNullable(this object str)
        {
            return ToByteNullable(str, null);
        }

        public static byte? ToByteNullable(this object obj, byte? defaultvalue)
        {
            if (obj==null)
            {
                return defaultvalue;
            }
            
            if (obj is byte? || obj is byte)
            {
                return (byte?)obj;
            }

            return obj.ToString().ToByteNullable(defaultvalue);
        }



        public static float? ToFloatNullable(this object str)
        {
            return ToFloatNullable(str, null);
        }

        public static float? ToFloatNullable(this object obj, float? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is float? || obj is float)
            {
                return (float?)obj;
            }

            return obj.ToString().ToFloatNullable(defaultvalue);
        }



        public static double? ToDoubleNullable(this object obj)
        {
            return ToDoubleNullable(obj, null);
        }

        public static double? ToDoubleNullable(this object obj, double? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is double? || obj is double)
            {
                return (double?)obj;
            }

            return obj.ToString().ToDoubleNullable(defaultvalue);
        }


        public static decimal? ToDecimalNullable(this object str)
        {
            return ToDecimalNullable(str, null);
        }

        public static decimal? ToDecimalNullable(this object obj, decimal? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is decimal? || obj is decimal)
            {
                return (decimal?)obj;
            }

            return obj.ToString().ToDecimalNullable(defaultvalue);
        }


        public static bool? ToBoolNullable(this object obj)
        {
            return ToBoolNullable(obj, null);
        }

        public static bool? ToBoolNullable(this object obj, bool? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            if (obj is bool? || obj is bool)
            {
                return (bool)obj;
            }

            return obj.ToString().ToBoolNullable(defaultvalue);
        }

        public static UInt16? ToUInt16Nullable(this object obj)
        {
            return ToUInt16Nullable(obj, null);
        }
        
        public static UInt16? ToUInt16Nullable(this object obj, UInt16? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            UInt16 t;

            if (UInt16.TryParse(obj.ToStringEx(""), out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }


        public static Int16? ToInt16Nullable(this object obj, Int16? defaultvalue)
        {
            if (obj == null)
            {
                return defaultvalue;
            }

            Int16 t;

            if (Int16.TryParse(obj.ToStringEx(""), out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }


        #endregion

        #region 对象类型转换 Cast

        /// <summary>
        ///     尝试将value的类型转换为dataType指定类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <returns>转换后的值</returns>
        /// <remarks>
        ///    如DataType=ConfigItemDataType.Boolean; <br/>
        ///    则会将: <br/>
        ///     int 非0 =true<br/>
        ///     int 0 =false<br/>
        ///                       <br/>
        ///     string "true" 或 "1" 或 "-1" =true<br/>
        ///     string "false" 或 "0" =false<br/>
        /// </remarks>
        /// <exception cref="InvalidCastException">无法转换为指定类型</exception>
        public static object Cast(this object value, Type dataType)
        {
            try
            {
                return CastInternal(value, dataType);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     尝试将value的类型转换为dataType指定类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue">如果无法转换,返回该默认值</param>
        /// <returns>转换后的值</returns>
        /// <remarks>
        ///    如DataType=ConfigItemDataType.Boolean; <br/>
        ///    则会将: <br/>
        ///     int 非0 =true<br/>
        ///     int 0 =false<br/>
        ///                       <br/>
        ///     string "true" 或 "1" 或 "-1" =true<br/>
        ///     string "false" 或 "0" =false<br/>
        /// </remarks>
        /// <exception cref="InvalidCastException">无法转换为指定类型</exception>
        public static object Cast(this object value, Type dataType, object defaultValue)
        {
            try
            {
                return CastInternal(value, dataType);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static T Cast<T>(this object value)
        {
            try
            {
                return (T) CastInternal(value, typeof (T));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static T Cast<T>(this object value, T defaultValue)
        {
            try
            {
                return (T) CastInternal(value, typeof (T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private static object CastInternal(object value, Type dataType)
        {
            object newValue = null;

            try
            {
                newValue = Convert.ChangeType(value, dataType);
            }
            catch (InvalidCastException)
            {
                if (dataType == typeof (bool))
                {
                    if (value is int)
                    {
                        if ((int) value == 0)
                        {
                            newValue = false;
                        }
                        else
                        {
                            newValue = true;
                        }
                    }
                    else if (value is string)
                    {
                        var tempValue = value.ToStringEx().ToLower();
                        if (tempValue == @"false" || tempValue == @"0")
                        {
                            newValue = false;
                        }
                        else if (tempValue == @"true" || tempValue == @"-1" || tempValue == @"1")
                        {
                            newValue = true;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                else if (dataType == typeof (DateTime))
                {
                    newValue = value.ToStringEx().ToDateTime("yyyy-MM-dd HH:mm:ss");
                    if (newValue == null)
                    {
                        newValue = value.ToStringEx().ToDateTime("yyyy-MM-dd");
                        if (newValue == null)
                        {
                            throw;
                        }
                    }

                }
                else if (dataType == typeof (Decimal))
                {
                    var temp = value.ToStringEx().ToDecimalNullable();

                    if (temp.HasValue)
                    {
                        newValue = temp.Value;
                    }
                    else
                    {
                        throw;
                    }
                }
                else if (dataType == typeof (int))
                {
                    var temp = value.ToStringEx().ToIntNullable();

                    if (temp.HasValue)
                    {
                        newValue = temp.Value;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return newValue;
        }

        #endregion

        /// <summary>
        ///    从当前对象将所有属性赋值到指定的对象中
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        public static void CopyValue<T>(this T origin, T target)
        {
            var orgProperties = (origin.GetType()).GetProperties();
            var orgFields = (origin.GetType()).GetFields();

            var targetProperties = (target.GetType()).GetProperties();
            var targetFields = (target.GetType()).GetFields();


            foreach (var field in orgFields)
            {
                if (!field.IsPublic)
                {
                    continue;
                }

                var ret = targetFields.FindItem((t1 => t1.Name == field.Name));

                if (ret != null && ret.IsPublic)
                {
                    ret.SetValue(target, field.GetValue(field));
                }
            }

            foreach (var property in orgProperties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var ret = targetProperties.FindItem((t1 => t1.Name == property.Name));

                if (ret != null && ret.CanWrite)
                {
                    ret.SetValue(target, property.GetValue(origin, null), null);
                }
            }


        }

        #region CheckIn

        /// <summary>
        ///     检查指定项是否在指定的列表中,采用默认的Equals比较函数
        /// </summary>
        /// <typeparam name="T">指定项类型</typeparam>
        /// <param name="obj">用于比较的项</param>
        /// <param name="checkList">源数据列表</param>
        /// <returns></returns>
        public static bool CheckIn<T>(this T obj, IEnumerable<T> checkList)
        {
            return CheckIn<T>(obj, checkList, null);
        }

        public static bool CheckIn<T>(this T obj, params T[] checkList)
        {
            return CheckIn(obj, checkList, null);
        }

        /// <summary>
        ///     检查指定项是否在指定的列表中
        /// </summary>
        /// <typeparam name="T">指定项类型</typeparam>
        /// <param name="obj">用于比较的项</param>
        /// <param name="checkList">源数据列表</param>
        /// <param name="checkFunc">自定义比较函数</param>
        /// <returns></returns>
        public static bool CheckIn<T>(this T obj, IEnumerable<T> checkList, CheckObjectEqual<T> checkFunc)
        {
            if (checkList == null)
            {
                return false;
            }

            CheckObjectEqual<T> func = checkFunc ?? CommonCheckEqual;

            foreach (var s1 in checkList)
            {
                if (func(obj, s1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CommonCheckEqual<T>(T obj1, T obj2)
        {
            return obj1.Equals(obj2);
            //return Equals(obj1, obj2);
        }

        #endregion

        private class DefaultValueStruct
        {
            public DefaultValueStruct(PropertyInfo executor, object defaultValue)
            {
                Executor = executor;
                DefaultValue = defaultValue;
            }

            public PropertyInfo Executor;
            public object DefaultValue;
        }

        private static Dictionary<Type, DefaultValueStruct[]> cacheDefaultValue = new Dictionary<Type, DefaultValueStruct[]>();
        public static void ResetPropsValueUsingDefaultAttributes(this Object oThis, bool initInheritedProperties)
        {
            var type = oThis.GetType();

            DefaultValueStruct[] defaultValueList = null;

            lock (cacheDefaultValue)
            {
                defaultValueList = cacheDefaultValue.TryGetValue(type, null);

                if (defaultValueList == null)
                {
                    defaultValueList = getDefaultValueExector(type, initInheritedProperties);

                    cacheDefaultValue.Add(type, defaultValueList);
                }
            }

            if (defaultValueList == null || defaultValueList.Length <= 0)
            {
                return;
            }

            foreach (var valueStruct in defaultValueList)
            {
                try
                {
                    valueStruct.Executor.SetValue(oThis, valueStruct.DefaultValue, null);
                }
                catch (Exception)
                {

                }

            }
        }

        private static DefaultValueStruct[] getDefaultValueExector(Type targetType, bool initInheritedProperties)
        {
            PropertyInfo[] infos = targetType.GetProperties(BindingFlags.NonPublic |
                           BindingFlags.Public | BindingFlags.Instance);

            var propList = new List<DefaultValueStruct>(infos.Length);

            foreach (PropertyInfo inf in infos)
            {
                if (initInheritedProperties || inf.DeclaringType.Equals(targetType))
                {
                    object[] oDefAtts = inf.GetCustomAttributes(typeof(DefaultValueAttribute), initInheritedProperties);

                    if (oDefAtts.Length > 0)
                    {
                        DefaultValueAttribute defAtt = oDefAtts.FindItem((o) => o is DefaultValueAttribute) as DefaultValueAttribute;

                        if (defAtt == null || defAtt.Value == null)
                        {
                            continue;
                        }

                        propList.Add(new DefaultValueStruct(inf, defAtt.Value));

                    }
                }
            }

            return propList.ToArray();
        }


        public delegate bool CheckObjectEqual<T>(T t1, T t2);

        public class SwithStruct<TInput,TOutput>
        {
            private TInput _currentValue;
            private bool _isCheckCompleted = false;
            private TOutput _outputValue = default(TOutput);

            internal SwithStruct(TInput value, TOutput deafultValue)
            {
                _currentValue = value;
                _outputValue = deafultValue;
            }

            public SwithStruct<TInput, TOutput> Case(TInput checkValue, TOutput value)
            {
                if (!_isCheckCompleted)
                {
                    if (checkValue == null && _currentValue==null)
                    {
                        _outputValue = value;
                    }
                    else if (checkValue != null)
                    {
                        if (checkValue.Equals(_currentValue))
                        {
                            _outputValue = value;
                        }
                    }                    
                }


                return this;
            }

            public TOutput Result => _outputValue;

            public override string ToString()
            {
                return Result?.ToString();
            }

            public static implicit  operator string(SwithStruct<TInput, TOutput> src)
            {
                return src.ToString();
            }
        }

        public class IfStruct<TInput,TOutput>
        {
            private TInput _inputValue = default(TInput);
            private TOutput _outputValue = default(TOutput);
            private bool _isCheckCompleted = false;

            internal IfStruct(TInput inputValue, Predicate<TInput> firstInputExp, TOutput firstOutput)
            {
                _inputValue = inputValue;

                if (firstInputExp(inputValue))
                {
                    _isCheckCompleted = true;
                    _outputValue = firstOutput;
                }
            }


            public IfStruct<TInput, TOutput> Elseif(Predicate<TInput> predicateExp, TOutput outputValue)
            {
                if (!_isCheckCompleted && predicateExp(_inputValue))
                {
                    _isCheckCompleted = true;
                    _outputValue = outputValue;
                }

                return this;
            }

            public IfStruct<TInput, TOutput> Elseif(Predicate<TInput> predicateExp, Func<TInput,TOutput> outputValue)
            {
                if (!_isCheckCompleted && predicateExp(_inputValue))
                {
                    _isCheckCompleted = true;
                    _outputValue = outputValue(_inputValue);
                }

                return this;
            }

            public IfStruct<TInput, TOutput> Else(TOutput outputValue)
            {
                if (!_isCheckCompleted)
                {
                    _outputValue = outputValue;
                    
                }

                return this;
            }

            public IfStruct<TInput, TOutput> Else(Func<TInput,TOutput> outputValue)
            {
                if (!_isCheckCompleted)
                {
                    _outputValue = outputValue(_inputValue);

                }

                return this;
            }

            public TOutput Result => _outputValue;

            public static implicit operator string(IfStruct<TInput, TOutput> d)
            {
                return d.Result.ToStringEx();
            }

            public override string ToString()
            {
                return Result?.ToStringEx();
            }
        }
    }

    public delegate TResult ChangeTypeFunc<TSource, TResult>(TSource src);

    public delegate object ChangeTypeFunc(object src);
}
