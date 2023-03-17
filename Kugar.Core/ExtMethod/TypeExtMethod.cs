using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class TypeExtMethod
    {

        #region
        private static readonly Dictionary<Type, HashSet<Type>> ConvertFromDict =
           new Dictionary<Type, HashSet<Type>>() { 
				{ typeof(short), new HashSet<Type>(){ typeof(sbyte), typeof(byte) } },
				{ typeof(ushort), new HashSet<Type>(){ typeof(char), typeof(byte) } },
				{ typeof(int), new HashSet<Type>(){ 
					typeof(char), typeof(sbyte), typeof(byte), 
					typeof(short), typeof(ushort) } },
				{ typeof(uint), new HashSet<Type>(){ 
					typeof(char), typeof(byte), typeof(ushort) } },
				{ typeof(long), new HashSet<Type>(){ 
					typeof(char), typeof(sbyte), typeof(byte), 
					typeof(short), typeof(ushort), typeof(int), typeof(uint) } },
				{ typeof(ulong), new HashSet<Type>(){ 
					typeof(char), typeof(byte), typeof(ushort), typeof(uint) } },
				{ typeof(float), new HashSet<Type>(){ 
					typeof(char), typeof(sbyte), typeof(byte), typeof(short), 
					typeof(ushort), typeof(int), typeof(uint), 
					typeof(long), typeof(ulong) } },
				{ typeof(double), new HashSet<Type>(){ 
					typeof(char), typeof(sbyte), typeof(byte), typeof(short), 
					typeof(ushort), typeof(int), typeof(uint), typeof(long), 
					typeof(ulong), typeof(float) } },
				{ typeof(decimal), new HashSet<Type>(){ 
					typeof(char), typeof(sbyte), typeof(byte), typeof(short), 
					typeof(ushort), typeof(int), typeof(uint), 
					typeof(long), typeof(ulong) } },
		};
    	
    	#endregion
    	
    	private static Dictionary<Type,HashSet<Type>> _cacheCanAssignableFrom=new Dictionary<Type, HashSet<Type>>();

        private static Dictionary<Type, HashSet<Type>> _cacheNoAssignableFrom = new Dictionary<Type, HashSet<Type>>();

        private static ConcurrentDictionary<Type,bool> _typeIsDispose=new ConcurrentDictionary<Type, bool>();

        public static bool IsDisposable(this Type type)
        {
            return _typeIsDispose.GetOrAdd(type, checkTypeIsDispose);
        }

        private static bool checkTypeIsDispose(Type type)
        {
            return type.IsImplementInterface(typeof(IDisposable));
        }

        /// <summary>
        ///     获取当前Type的默认值
        ///     注:如果为枚举型(enum),则返回枚举列表中的第一个值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            if (type.IsValueType)
            {

                if (type.IsEnum)
                {
                    var ar = Enum.GetValues(type);

                    if (ar == null || ar.Length <= 0)
                    {
                        throw new ArgumentException("当前可枚举类型中,未定义任何值");
                    }
                    return ar.GetValue(0);
                }

                if (type == typeof(int) ||
                    type == typeof(Int16) ||
                    type == typeof(Int64) ||
                    type == typeof(sbyte) ||
                    type == typeof(uint) ||
                    type == typeof(short) ||
                    type == typeof(ulong) ||
                    type == typeof(ushort)
                    )
                {
                    return 0;
                }
                else if (type == typeof(long))
                {
                    return 0L;
                }
                else if (type == typeof(float))
                {
                    return 0.0F;
                }
                else if (type == typeof(double))
                {
                    return 0.0D;
                }
                else if (type == typeof(decimal))
                {
                    return 0.0M;
                }
                else if (type == typeof(bool))
                {
                    return false;
                }
                else if (type == typeof(byte))
                {
                    return 0;
                }
                else if (type == typeof(char))
                {
                    return '\0';
                }
                else if (type == typeof(sbyte))
                {
                    return 0;
                }
                else if (!type.IsEnum && !type.IsPrimitive)
                {
                    return Activator.CreateInstance(type);
                }
                else
                {
                    throw new Exception("未知类型");
                }

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     生成指定类型的可空类型,如果当前为引用类型,则返回原Type,如果为ValueType(值类型)则返回生成的可空泛型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GencentNullableType(this Type type)
        {
            if (type.IsValueType)
            {
                var nullable = typeof(Nullable<>);

                return nullable.MakeGenericType(new[] { type });
            }
            else
            {
                return type;
            }
        }

        /// <summary>
        ///     根据value值的类型，返回一个格式化器
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ValueTypeFormatter GetValueFormatter(this object value)
        {
            return GetTypeFormatter(value.GetType());
        }

        /// <summary>
        ///     根据type指定的类型，返回一个格式化器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ValueTypeFormatter GetTypeFormatter(this Type type)
        {
            if (type == typeof(short))
            {
                return Int16Formatter;
            }

            if (type == typeof(int))
            {
                return Init32Formatter;
            }

            if (type  == typeof( long))
            {
                return Int64Formatter;
            }

            if (type  == typeof( ushort))
            {
                return UInt16Formatter;
            }

            if (type  == typeof( uint))
            {
                return UInit32Formatter;
            }

            if (type  == typeof( ulong))
            {
                return UInt64Formatter;
            }

            if (type  == typeof( double))
            {
                return DoubleFormatter;
            }

            if (type  == typeof( float))
            {
                return SingleFormatter;
            }

            if (type  == typeof( decimal))
            {
                return DecimalFormatter;
            }

            if (type  == typeof( byte))
            {
                return ByteFormatter;
            }

            if (type  == typeof( DateTime))
            {
                return DateTimeFormatter;
            }

            return null;
        }

        /// <summary>
        ///     返回value参数指定值格式化之后的字符串
        /// </summary>
        /// <param name="value">源值</param>
        /// <param name="formatString">格式化字符串</param>
        /// <returns>返回格式化后的字符串</returns>
        public static string GetValueFormatString(this object value,string formatString)
        {
            var d = GetTypeFormatter(value.GetType());

            if (d!=null)
            {
                return d(value, formatString, null);
            }
            else
            {
                if (value!=null)
                {
                    return value.ToString();
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        ///     判断指定类型是否实现了某个接口
        /// </summary>
        /// <param name="type">类型Type对象</param>
        /// <param name="interfaceType">判断用的接口Type</param>
        /// <returns></returns>
        [Obsolete]
        public static bool IsImplementlInterface(this Type type,Type interfaceType)
        {
            var interfacelist = type.GetInterfaces();

            if (interfacelist != null && interfacelist.Length>0)
            {
                foreach (var ifc in interfacelist)
                {
                    if (ifc == interfaceType)// || ifc.IsAssignableFrom(interfaceType))
                    {
                        return true;
                    }

                    if (ifc.IsGenericType)
                    {
                        var genType = ifc.GetGenericTypeDefinition();

                        if (genType==interfaceType )//|| genType.IsAssignableFrom(interfaceType))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

        /// <summary>
        ///     判断指定类型是否实现了某个接口
        /// </summary>
        /// <param name="type">类型Type对象</param>
        /// <param name="interfaceType">判断用的接口Type</param>
        /// <returns></returns>
        public static bool IsImplementInterface(this Type type, Type interfaceType)
        {
            var interfacelist = type.GetInterfaces();

            if (interfacelist != null && interfacelist.Length > 0)
            {
                foreach (var ifc in interfacelist)
                {
                    if (ifc == interfaceType)// || ifc.IsAssignableFrom(interfaceType))
                    {
                        return true;
                    }

                    if (ifc.IsGenericType)
                    {
                        var genType = ifc.GetGenericTypeDefinition();

                        if (genType == interfaceType)//|| genType.IsAssignableFrom(interfaceType))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

        /// <summary>
        ///      判断指定Type是否为数字类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumericType(this Type type)
        {
            if (type == typeof(int) ||
                            type == typeof(uint) ||
                            type == typeof(long) ||
                            type == typeof(ulong) ||
                            type == typeof(short) ||
                            type == typeof(ushort) ||
                            type == typeof(double) ||
                            type == typeof(float) ||
                            type == typeof(decimal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        ///     判断 srcType 类型是否继承自baseType类型
        /// </summary>
        /// <param name="srcType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
		public static bool IsAssignableFromEx(this Type srcType,Type baseType)
		{
			
			if (srcType==baseType) {
				return true;
			}
			
			HashSet<Type> matchTypes=null;
			
			if (_cacheCanAssignableFrom.TryGetValue(srcType, out matchTypes) && matchTypes.Contains(baseType))
			{
				return true;
			}

		    if (_cacheNoAssignableFrom.TryGetValue(srcType,out matchTypes) && matchTypes.Contains(baseType))
		    {
		        return false;
		    }
			
			var ret=false;
			
			if (baseType.IsInterface) {
				ret= IsImplementInterface(srcType,baseType);
				//return IsImplementlInterface(srcType,baseType);
			}
			
			if (!ret ) {
				ret=srcType.IsAssignableFrom(baseType) || srcType.IsSubclassOf(baseType);
				//return true;
			}
			
			
			HashSet<Type> typeSet;
			if (!ret && ConvertFromDict.TryGetValue(baseType, out typeSet))
			{
				ret=typeSet.Contains(srcType);
				//return typeSet.Contains(srcType.TypeHandle);
			}
			
	        if (!ret) {

                ret = srcType.GetMethod("op_Explicit", (BindingFlags.Public | BindingFlags.Static), null, new Type[] { baseType }, new ParameterModifier[0])!=null;
	        	//return true;
	        }
			
			if (!ret) {

                ret = baseType.GetMethod("op_Explicit", (BindingFlags.Public | BindingFlags.Static), null, new Type[] { srcType }, new ParameterModifier[0])!=null;
	        	//return true;
	        }

		    if (ret)
		    {
                if (!_cacheCanAssignableFrom.TryGetValue(srcType, out matchTypes))
                {
                    matchTypes = new HashSet<Type>();

                    _cacheCanAssignableFrom.Add(srcType, matchTypes);
                }

                matchTypes.Add(baseType);
		    }
		    else
		    {
                if (!_cacheNoAssignableFrom.TryGetValue(srcType, out matchTypes))
		        {
                    matchTypes = new HashSet<Type>();
                    _cacheNoAssignableFrom.Add(srcType,matchTypes);
		        }

		        matchTypes.Add(baseType);
		    }

			return ret;
		}

        /// <summary>
        /// 	判断 srcType 参数指定的类型,是否能转换为 baseType 参数指定的类型
        /// </summary>
        /// <param name="srcType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        /// <example>
        ///    CanConvertTo(typeof(int),typeof(object))  =true <br/>
        ///    CanConvertTo(typeof(int),typeof(IConvertible)) = true <br/>
        /// </example>
        public static bool CanConvertTo(this Type srcType,Type baseType)
		{
        	 
			if (srcType==baseType) {
				return true;
			}
			
			if (baseType.IsInterface) {
				return IsImplementInterface(srcType,baseType);
			}
			
			if (!srcType.IsAssignableFrom(baseType) && !srcType.IsSubclassOf(baseType)) {
				return false;
			}
			else{
				return true;
			}
		}

        /// <summary>
        /// 获取childType对parentType的继承层数
        /// </summary>
        /// <param name="childType">源类型</param>
        /// <param name="parentType">用于最终检查的类型</param>
        /// <returns></returns>
        public static int GetTypeInheritLevel(this Type childType, Type parentType)
        {
            if (!childType.IsClass || !parentType.IsClass)
            {
                throw new ArgumentException("该函数只能检查Class类型");
            }

            var level = 0;

            var currentLevelType = childType;

            while (currentLevelType != parentType)
            {
                if (currentLevelType == parentType)
                {
                    break;
                }

                level += 1;

                currentLevelType = currentLevelType.BaseType;
            }

            return level;
        }

        /// <summary>
        /// 从DescriptionAttribute中获取注释,如果未定义,这返回defaultValue参数的值
        /// </summary>
        /// <param name="member"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetMemberDescription(this MemberInfo member,string defaultValue="")
        {
            var attr = member.GetCustomAttribute<DescriptionAttribute>();

            if (attr!=null)
            {
                return attr.Description;
            }
            else
            {
                return defaultValue;
            }
        }

        #region "定义格式化委托"

        private static string Int16Formatter(object value, string formatString, IFormatProvider formatProvide)
        {
            if (value is short)
            {
                return ((short)value).ToString(formatString, formatProvide);
            }

            return null;
        }

        private static string Init32Formatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is int)
            {
                return ((int)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string Int64Formatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is long)
            {
                return ((long)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string UInt16Formatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is ushort)
            {
                return ((ushort)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string UInit32Formatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is uint)
            {
                return ((uint)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string UInt64Formatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is ulong)
            {
                return ((ulong)value).ToString(formatString, formatProvider);
            }

            return null;
        }


        private static string DoubleFormatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is double)
            {
                return ((double)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string SingleFormatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is float)
            {
                return ((float)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string DecimalFormatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is decimal)
            {
                return ((decimal)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string ByteFormatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is byte)
            {
                return ((byte)value).ToString(formatString, formatProvider);
            }

            return null;
        }

        private static string DateTimeFormatter(object value, string formatString, IFormatProvider formatProvider)
        {
            if (value is DateTime)
            {
                return ((DateTime)value).ToString(formatString,formatProvider);
            }

            return null;
        }

        #endregion

        #region PorpertyInfo

        public static bool IsStaticProperty(this PropertyInfo property)
        {
            if (property.CanRead)
            {
                return property.GetGetMethod().IsStatic;
            }
            else if (property.CanWrite)
            {
                return property.GetSetMethod().IsStatic;
            }
            else
            {
                throw new Exception("该属性不可读,也不可写");
            }
        }

        #endregion
    }

    public delegate string ValueTypeFormatter(object value, string formatString,IFormatProvider formatProvider);
}
