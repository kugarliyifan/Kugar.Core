using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Kugar.Core.Exceptions;

namespace Kugar.Core.ExtMethod
{
    public static class EnumAbout
    {
        /// <summary>
        ///     判断源枚举中，是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsContans<T>(this T src, T value) where T:struct
        {
            CheckEnumIsFlags<T>();

            var srcvalue= Convert.ToInt32(src);
            var desvalue=Convert.ToInt32(value);

            return (srcvalue & desvalue)==desvalue;
                
        }

        public static T FlagRemove<T>(this T src, T value) where T : struct
        {
            CheckEnumIsFlags<T>();
            

            var srcvalue = Convert.ToInt32(src);
            var desvalue = Convert.ToInt32(value);

            var ret = srcvalue ^ desvalue;

            var ec = new EnumConverter(typeof (T));

            return (T)ec.ConvertFrom(ret);
            
        }

        public static T FlagAdd<T>(this T src, T value) where T : struct
        {
            CheckEnumIsFlags<T>();
            

            var srcvalue = Convert.ToInt32(src);
            var desvalue = Convert.ToInt32(value);

            var ret = srcvalue | desvalue;

            var ec = new EnumConverter(typeof(T));

            return (T)ec.ConvertFrom(ret);
            
        }

        public static T ToEnum<T>(this int src)
        {
            var type = typeof (T);

            if (!type.IsEnum)
            {
                throw new ArgumentTypeNotMatchException("T","Enum");
            }

            if (!Enum.IsDefined(type, src))
            {
                throw new ArgumentOutOfRangeException("src","指定值不在枚举中");
            }

            return  (T)Enum.ToObject(type, src);
            
            //var convert = new EnumConverter(typeof (T));

            //return (T)convert.ConvertTo(src, typeof (T));
        }

#if !NETCOREAPP2_2 && !NETCOREAPP3_0 && !NETCOREAPP3_1
        /// <summary>
        /// 获取描述信息
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    foreach (var item in attrs)
                    {
                        if (item is DescriptionAttribute)
                            return ((DescriptionAttribute)item).Description;
                    }
                }

            }
            return en.ToString();
        }

#endif

        /// <summary>
        /// 获取DisplayNameAttribute中的名称
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    foreach (var item in attrs)
                    {
                        if (item is DisplayNameAttribute)
                            return ((DisplayNameAttribute)item).DisplayName;
                    }
                }

            }
            return en.ToString();
        }





        //public static IEnumerable<KeyValuePair<string,string>> ToKeyValuePairs(this Type) 

        private static void CheckEnumIsFlags<T>() where T:struct 
        {
            Type type = typeof(T);

            if (!type.IsEnum)
            {
                throw new TypeLoadException();
            }

            var customAttr = type.GetCustomAttributes(typeof(FlagsAttribute), true);

            if (customAttr == null || customAttr.Length <= 0)
            {
                throw new TypeLoadException("指定的枚举类型必须使用Flags标记");
            }
        }

        


    }
}
