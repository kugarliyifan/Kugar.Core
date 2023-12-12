using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.ExtMethod
{
    public static class StringAbout
    {
        /// <summary>
        /// 	使用函数检查,截取字符串
        /// </summary>
        /// <param name="srcStr">原字符串</param>
        /// <param name="beginChecker">检查是否开始的函数,返回true则表示读取到该字符开始读取</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string Substring(this string srcStr, Predicate<char> beginChecker, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > srcStr.Length - 1)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            var index = 0;

            for (int i = startIndex; i < srcStr.Length; i++)
            {
                if (beginChecker(srcStr[i]))
                {
                    index = i;
                    break;
                }
            }

            return srcStr.Substring(index+1);
        }

        public static string Substring(this string srcStr, Func<char, char, bool> beginChecker,
                                       Func<char, char, bool> endChecker, int startIndex = 0)
        {
            if (startIndex <= 0 || startIndex > srcStr.Length - 1)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            var beginIndex = 0;
            var endIndex = 0;
            bool isBegin = false;

            for (int i = startIndex; i < srcStr.Length; i++)
            {
                char endChar = (char)0;
                if (!isBegin)
                {

                    if (i != srcStr.Length - 1)
                    {
                        endChar = srcStr[i];
                    }

                    if (beginChecker(srcStr[i], endChar))
                    {
                        isBegin = true;
                        beginIndex = i;
                        continue;
                    }
                }

                if (isBegin)
                {
                    if (i != srcStr.Length - 1)
                    {
                        endChar = srcStr[i + 1];
                    }

                    if (endChecker(srcStr[i], endChar))
                    {
                        endIndex = i;
                        break;
                    }
                }
            }

            return srcStr.Substring(beginIndex, endIndex - beginIndex + 1);
        }


        public static string SubString(this string srcStr, string startStr, string endStr,int startIndex=0)
        {
            var index = srcStr.IndexOf(startStr, startIndex);

            if (index<0)
            {
                return "";
            }

            index += startStr.Length;

            var endindex = srcStr.IndexOf(endStr, index);

            return srcStr.Substring(index, endindex - index);
        }

        /// <summary> 
        /// 在指定的字符串列表CnStr中检索符合拼音索引字符串 ,如果是英文字母，则返回大写字母
        /// </summary> 
        /// <param name="CnStr">汉字字符串</param> 
        /// <returns>相对应的汉语拼音首字母串</returns> 
        public static string GetSpellCode(this string CnStr)
        {
            //string strTemp = "";
            int iLen = CnStr.Length;
            int i = 0;

            var charList = new char[CnStr.Length];
            var oldCharList = CnStr.ToCharArray();

            for (i = 0; i <= iLen - 1; i++)
            {
                if (oldCharList[i] >= 255)
                {
                    charList[i] = oldCharList[i];
                }
                else
                {
                    charList[i] = GetCharSpellCode(oldCharList[i]);
                }

                //var t = CnStr[i] >= 255;

                //strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
            }

            return new string(charList);
        }
        

        /// <summary> 
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母 
        /// </summary> 
        /// <param name="CnChar">单个汉字</param> 
        /// <returns>单个大写字母</returns> 
        private static char GetCharSpellCode(int iCnChar)
        {
            //long iCnChar;

            //var t = (int) CnChar;

            //byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回 
            //if (ZW.Length == 1)
            //{
            //    return CnChar.ToUpper();
            //}
            //else
            //{
            //    // get the array of byte from the single char 
            //    int i1 = (short)(ZW[0]);
            //    int i2 = (short)(ZW[1]);
            //    iCnChar = i1 * 256 + i2;
            //}
            //expresstion 
            //table of the constant list 
            // 'A'; //45217..45252 
            // 'B'; //45253..45760 
            // 'C'; //45761..46317 
            // 'D'; //46318..46825 
            // 'E'; //46826..47009 
            // 'F'; //47010..47296 
            // 'G'; //47297..47613 

            // 'H'; //47614..48118 
            // 'J'; //48119..49061 
            // 'K'; //49062..49323 
            // 'L'; //49324..49895 
            // 'M'; //49896..50370 
            // 'N'; //50371..50613 
            // 'O'; //50614..50621 
            // 'P'; //50622..50905 
            // 'Q'; //50906..51386 

            // 'R'; //51387..51445 
            // 'S'; //51446..52217 
            // 'T'; //52218..52697 
            //没有U,V 
            // 'W'; //52698..52979 
            // 'X'; //52980..53640 
            // 'Y'; //53689..54480 
            // 'Z'; //54481..55289 

            // iCnChar match the constant 
            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {
                return 'A';
            }
            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {
                return 'B';
            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {
                return 'C';
            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {
                return 'D';
            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {
                return 'E';
            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {
                return 'F';
            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {
                return 'G';
            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {
                return 'H';
            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {
                return 'J';
            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {
                return 'K';
            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {
                return 'L';
            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {
                return 'M';
            }

            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {
                return 'N';
            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {
                return 'O';
            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {
                return 'P';
            }
            else if ((iCnChar >= 50906) && (iCnChar <= .51386))
            {
                return 'Q';
            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {
                return 'R';
            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {
                return 'S';
            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {
                return 'T';
            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {
                return 'W';
            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {
                return 'X';
            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {
                return 'Y';
            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {
                return 'Z';
            }
            else return ('?');
        }

        /// <summary>
        ///     比较两个字符串,可忽略大小写比较,注意:Linq To EF中不可用
        /// </summary>
        /// <param name="srcStr">原字符串</param>
        /// <param name="desStr">目标字符串</param>
        /// <param name="isIgoneCase">是否忽略大小写,,true为忽略大小写比较</param>
        /// <returns></returns>
        public static bool CompareTo(this string srcStr, string desStr, bool isIgnoreCase)
        {
            if (isIgnoreCase)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(srcStr, desStr) == 0;
            }
            else
            {
                return srcStr == desStr;
            }
        }

        /// <summary>
        ///     判断源字符串中是否包含指定字符串,允许忽略大小写,注意:Linq To EF中不可用
        /// </summary>
        /// <param name="srcStr">原字符串</param>
        /// <param name="checkStr">比较字符串</param>
        /// <param name="isIgnoreCase">是否忽略大小写,,true为忽略大小写比较</param>
        /// <returns></returns>
        public static bool Contains(this string srcStr, string checkStr, bool isIgnoreCase)
        {
            if (isIgnoreCase)
            {
                return srcStr.IndexOf(checkStr, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                return srcStr == checkStr;
            }
        }

        public static string ToStringEx(this string str)
        {
            return ToStringEx(str, "");
        }

        public static string ToStringEx(this string str, string defaultValue)
        {
            return IfEmptyOrWhileSpace(str, defaultValue);
        }



        [Obsolete]
        public static bool IsNum(this string str)
        {
            decimal temp;

            return decimal.TryParse(str, out temp);

        }

        public static bool IsNumeric(this string str)
        {

            decimal temp;

            return decimal.TryParse(str, out temp);

        }

        public static int ToInt(this string str)
        {
            return ToInt(str, 0);
        }

        public static int ToInt(this string str, int defaultvalue)
        {
            return ToInt(str, defaultvalue, 10);
        }

        public static uint ToUInt(this string str, uint defaultvalue=0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            uint i;
            
            try
            {
                
                var temp = str.IndexOf('.');
                if (temp != -1)
                {
                    str = str.Substring(0, temp);
                }

                i = Convert.ToUInt32(str.Trim());
            }
            catch (Exception)
            {
                return defaultvalue;
            }

            return i;
        }



        public static int ToInt(this string str, int defaultvalue, int jinzhi)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            int i;



            try
            {


                var temp = str.IndexOf('.');
                if (temp != -1)
                {
                    str = str.Substring(0, temp);
                }

                i = Convert.ToInt32(str.Trim(), jinzhi);
            }
            catch (Exception)
            {
                return defaultvalue;
            }

            return i;

        }

        public static long ToLong(this string src, long defaultValue=0l)
        {
            long value;

            if (long.TryParse(src,out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static ulong ToULong(this string src, ulong defaultValue = 0)
        {
            ulong value;

            if (ulong.TryParse(src, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static int ToMinInt(this string str, int minvalue)
        {
            int i = ToInt(str);
            if (i < minvalue)
            {
                return minvalue;
            }

            return i;
        }



        public static byte ToByte(this string str)
        {
            return ToByte(str, 0);
        }

        public static byte ToByte(this string str, byte defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            byte temp;

            if (byte.TryParse(str, out temp))
            {
                return temp;
            }
            return defaultvalue;
        }

        /// <summary>
        ///     转换到float型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float ToFloat(this string str)
        {
            return ToFloat(str, 0);
        }

        public static float ToFloat(this string str, float defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            float t;

            if (float.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        public static double ToDouble(this string str)
        {
            return ToDouble(str, 0);
        }

        public static double ToDouble(this string str, double defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            Double t;

            if (Double.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }


        public static decimal ToDecimal(this string str)
        {
            return ToDecimal(str, 0);
        }

        public static decimal ToDecimal(this string str, decimal defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }
            
            decimal t;

            if (decimal.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        //Bool粘贴
        public static bool ToBool(string str)
        {
            return ToBool(str, false);
        }

        public static bool ToBool(this string str, bool defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            bool b;

            str = str.ToLower();

            if (str=="false")
            {
                return false;
            }
            else if(str=="true")
            {
                return true;
            }

            var v = str.ToIntNullable(null);

            if (v==null)
            {
                return defaultvalue;
            }
            else if (v==0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }



        #region "转换可空类型"

        public static int? ToIntNullable(this string str)
        {
            return ToIntNullable(str, null, 10);
        }

        public static int? ToIntNullable(this string str, int? defaultvalue)
        {
            return ToIntNullable(str, defaultvalue, 10);
        }

        public static int? ToIntNullable(this string str, int? defaultvalue, int jinzhi)
        {

            if (string.IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            int i;

            try
            {
                var temp = str.IndexOf('.');
                if (temp != -1)
                {
                    str = str.Substring(0, temp);
                }

                i = Convert.ToInt32(str.Trim(), jinzhi);
            }
            catch (Exception)
            {
                return defaultvalue;
            }

            return i;

        }

        public static uint? ToUIntNullable(this string str, uint? defaultvalue)
        {

            if (string.IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            uint? i;

            try
            {
                var temp = str.IndexOf('.');
                if (temp != -1)
                {
                    str = str.Substring(0, temp);
                }

                i = Convert.ToUInt32(str.Trim());
            }
            catch (Exception)
            {
                return defaultvalue;
            }

            return i;

        }

        public static byte? ToByteNullable(this string str)
        {
            return ToByteNullable(str, null);
        }

        public static byte? ToByteNullable(this string str, byte? defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            byte temp;

            if (byte.TryParse(str, out temp))
            {
                return temp;
            }
            return defaultvalue;
        }

        public static float? ToFloatNullable(this string str)
        {
            return ToFloatNullable(str, null);
        }

        public static float? ToFloatNullable(this string str, float? defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            float t;

            if (float.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        public static double? ToDoubleNullable(this string str)
        {
            return ToDoubleNullable(str, null);
        }

        public static double? ToDoubleNullable(this string str, double? defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            Double t;

            if (Double.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        public static decimal? ToDecimalNullable(this string str)
        {
            return ToDecimalNullable(str, null);
        }

        public static decimal? ToDecimalNullable(this string str, decimal? defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }

            decimal t;

            if (decimal.TryParse(str, out t))
            {
                return t;
            }
            else
            {
                return defaultvalue;
            }
        }

        public static bool? ToBoolNullable(this string str)
        {
            return ToBoolNullable(str, null);
        }

        public static bool? ToBoolNullable(this string str, bool? defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultvalue;
            }
            
            bool b;
            if (bool.TryParse(str, out b))
            {
                return b;
            }
            return defaultvalue;
        }

        public static DateTime? ToDateTimeNullable(this string str)
        {
            const string s = @"yyyy-MM-dd HH:mm:ss";
            return ToDateTimeNullable(str, s);
        }

        public static DateTime? ToDateTimeNullable(this string str,string dateformat)
        {
            return ToDateTimeNullable(str, dateformat,null);
        }

        public static DateTime? ToDateTimeNullable(this string obj,string dateformat, DateTime? defaultvalue)
        {
            if (IsNullOrEmpty(obj))
            {
                return defaultvalue;
            }

            DateTime dt;

            if (DateTime.TryParseExact(obj,dateformat,null,DateTimeStyles.None,out dt))
            {
                return dt;
            }
            else
            {
                return defaultvalue;
            }

        }

        public static DateTime? ToDateNullable(this string obj, string dateformat="yyyy-MM-dd", DateTime? defaultvalue=null)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                return defaultvalue;
            }
            
            if (DateTime.TryParseExact(obj, dateformat, CultureInfo.GetCultureInfo("zh-cn"), DateTimeStyles.AllowWhiteSpaces, out var dt))
            {
                return dt;
            }
            else
            {
                return defaultvalue;
            }
        }


        public static long? ToLongNullable(this string src, long? defaultValue = null)
        {
            long value;

            if (long.TryParse(src, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static ulong? ToULongNullable(this string src, ulong? defaultValue = null)
        {
            ulong value;

            if (ulong.TryParse(src, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        private static char[] removeTrimChars = new[] { ' ' };

        public static StringBuilder TrimEnd(this StringBuilder builder)
        {
            return TrimEnd(builder, removeTrimChars);
        }

        public static StringBuilder Trim(this StringBuilder builder)
        {
            return Trim(builder, removeTrimChars);
        }

        /// <summary>
        /// 移出末尾指定字符,默认为空格
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="removeChar"></param>
        /// <returns></returns>
        public static StringBuilder TrimEnd(this StringBuilder builder,params char[] removeChar )
        {
            if (builder.Length == 0)
                return builder;

            if (!removeChar.HasData())
            {
                return builder;
            }

            var count = 0; 

            for (var i = builder.Length - 1; i >= 0; i--)
            {
                if (!removeChar.Contains(builder[i]))
                    break;
                count++;
            }

            if (count > 0)
                builder.Remove(builder.Length - count, count);

            return builder;
        }

        /// <summary>
        /// 移出前后指定字符,默认为空格
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="removeChar"></param>
        /// <returns></returns>
        public static StringBuilder Trim(this StringBuilder builder, params char[] removeChar)
        {
            if (builder.Length == 0)
                return builder;

            if (!removeChar.HasData())
            {
                return builder;
            }

            var count = 0;
            for (var i = 0; i < builder.Length; i++)
            {
                if (!removeChar.Contains(builder[i]))
                    break;
                count++;
            }

            if (count > 0)
            {
                builder.Remove(0, count);
                count = 0;
            }

            for (var i = builder.Length - 1; i >= 0; i--)
            {
                if (!removeChar.Contains(builder[i]))
                    break;
                count++;
            }

            if (count > 0)
                builder.Remove(builder.Length - count, count);

            return builder;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
           
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !IsNullOrEmpty(str);
        }

        public static DateTime ToDateTime(this string str)
        {
            return ToDateTime(str, DateTime.MinValue);
        }
        
        public static DateTime? ToDateTime(this string str, string dateformat="yyyy-MM-dd HH:mm:ss")
        {
            return ToDateTime(str, dateformat, DateTime.MinValue);
        }

        public static DateTime ToDateTime(this string str, DateTime defaultvalue)
        {
            return ToDateTime(str, "yyyy-MM-dd HH:mm:ss", defaultvalue);
        }

        public static DateTime ToDateTime(this string str, string dateformat, DateTime defaultvalue)
        {
            if (IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            DateTime dt;

            if (DateTime.TryParseExact(str, dateformat, CultureInfo.GetCultureInfo("zh-cn"), DateTimeStyles.AllowWhiteSpaces, out dt))
            {
                return dt;
            }
            else
            {
                return defaultvalue;
            }

        }

        public static DateTime ToDate(this string str, DateTime defaultValue)
        {
            return ToDateTime(str, "yyyy-MM-dd", defaultValue);
        }

        public static DateTime ToDate(this string str)
        {
            return ToDate(str, DateTime.Now.Date);
        }

        public static JObject ToJObject(this string str)
        {
            return JObject.Parse(str);
        }

        public static JObject ToJObject(this string str, JObject defaultValue)
        {
            try
            {
                return JObject.Parse(str);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T ToEnum<T>(this string str)
        {
            //if (typeof(T) == typeof(Enum))
            //{
            //    return default(T);
            //}

            //T temp = default(T);

            //try
            //{
            //    temp = (T) Enum.Parse(typeof (T), str, true);
            //}
            //catch (Exception)
            //{
            //    return default(T);
            //}

            //return temp;

            return ToEnum(str, default(T));


        }

        public static T ToEnum<T>(this string str,T defaultvalue)
        {
            if (typeof(T) == typeof(System.Enum))
            {
                return default(T);
            }

            T temp = default(T);

            try
            {
                temp = (T)System.Enum.Parse(typeof(T), str, true);
            }
            catch (Exception)
            {
                //return default(T);
                return defaultvalue;
            }

            return temp;

        }

        public static bool CheckIn(this string s, params string[] str)
        {
            if (s == null)
            {
                return false;
            }

            if (str == null || str.Length < 0)
            {
                return true;
            }

            foreach (var s1 in str)
            {
                if (s1 == s)
                {
                    return true;
                }
            }

            return false;
        }

        public static string LeftBefore(this string srcStr, char c)
        {
            return LeftBefore(srcStr, c, srcStr);
        }

        public static string LeftBefore(this string srcStr,char c,string defaultValue)
        {
            var index = srcStr.IndexOf(c);

            if (index < 0)
            {
                return defaultValue;
            }
            else
            {
                return srcStr.Substring(0, index-1);
            }
        }

        public static string LeftAfter(this string srcStr, char c)
        {
            return LeftAfter(srcStr, c, srcStr);
        }

        public static string LeftAfter(this string srcStr, char c, string defaultValue)
        {
            var index = srcStr.IndexOf(c);

            if (index < 0)
            {
                return defaultValue;
            }
            else
            {
                return srcStr.Substring(index+1, srcStr.Length - index);
            }
        }

        public static string RightBeforce(this string srcStr, char c)
        {
            return RightBeforce(srcStr, c, srcStr);
        }

        public static string RightBeforce(this string srcStr, char c, string defaultValue)
        {
            var index = srcStr.LastIndexOf(c);

            if (index < 0)
            {
                return defaultValue;
            }
            else
            {
                return srcStr.Substring(index, srcStr.Length - index);
            }
        }

        public static string RightAfter(this string srcStr, char c)
        {
            return RightAfter(srcStr, c, srcStr);
        }

        public static string RightAfter(this string srcStr, char c, string defaultValue)
        {
            var index = srcStr.LastIndexOf(c);

            if (index < 0)
            {
                return defaultValue;
            }
            else
            {
                return srcStr.Substring(0,  index-1);
            }
        }

        /// <summary>
        ///     在字符串头部去除指定的匹配字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="startStr">匹配字符串</param>
        /// <returns>返回成功去除后的结果</returns>
        public static string TrimStart(this string str, string startStr)
        {
            if (!str.StartsWith(startStr))
            {
                return str;
            }

            string t = str.Substring(startStr.Length);

            return t;

        }

        /// <summary>
        ///     在字符串末尾去除指定的匹配字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="endStr"></param>
        /// <returns></returns>
        public static string TrimEnd(this string str, string endStr)
        {
            if (!str.EndsWith(endStr))
            {
                return str;
            }

            string t = str.Substring(0, str.Length - endStr.Length);

            return t;

        }

        /// <summary>
        ///     在字符串首尾去除指定的匹配字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="partStr"></param>
        /// <returns></returns>
        public static string Trim(this string str, string partStr)
        {
            if (!str.EndsWith(partStr) && !str.StartsWith(partStr))
            {
                return str;
            }

            string t = TrimEnd(TrimStart(str, partStr), partStr);

            return t;

        }

        public static string TrimSelf(this string str, string partStr)
        {
            str = Trim(str, partStr);

            return str;
        }

        public static string Format(this string str,params object[] args)
        {
            if (!str.IsNullOrEmpty())
            {
                return string.Format(str, args);
            }

            return str;
        }

        public static string Format(this string str,string args1)
        {
            if (!str.IsNullOrEmpty())
            {
                return string.Format(str, args1);
            }

            return str;
        }



        /// <summary>
        ///     从字符串左边开始截取指定长度的字符串,源字符串长度不足时,则返回源字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string Left(this string str,int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            if (str.Length<=length)
            {
                return str;
            }
            else
            {
                return str.Substring(0, length);
            }
        }

        /// <summary>
        ///     从字符串右边开始截取指定长度的字符串,源字符串长度不足时,则返回源字符串
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string Right(this string str, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            if (str.Length <= length)
            {
                return str;
            }
            else
            {
                return str.Substring(str.Length-length);
            }
        }

        /// <summary>
        ///     判断是否为空或者空格的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmptyOrWhileSpace(this string str)
        {
            if(str==null)
            {
                return true;
            }

            return string.IsNullOrEmpty(str.Trim());
        }

        /// <summary>
        ///     当字符串为空或空格字符串时，调用指定的函数，并返回函数处理后的值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func">处理用的函数</param>
        /// <returns></returns>
        public static string IfEmptyOrWhileSpace(this string str,Func<string ,string > func)
        {
            if (IsEmptyOrWhileSpace(str))
            {
                return func(str);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        ///     当字符串为空或空格字符串时,返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string IfEmptyOrWhileSpace(this string str,string defaultValue)
        {
            if (IsEmptyOrWhileSpace(str))
            {
                return defaultValue;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        ///     当字符串不为空或空格字符串时，调用指定的函数，并返回函数处理后的值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func">处理用的函数</param>
        /// <returns></returns>
        public static string IfNotEmptyOrWhileSpace(this string str, Func<string, string> func)
        {
            if (!IsEmptyOrWhileSpace(str))
            {
                return func(str);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        ///     当字符串不为空或空格字符串时,返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string IfNotEmptyOrWhileSpace(this string str, string defaultValue)
        {
            if (!IsEmptyOrWhileSpace(str))
            {
                return defaultValue;
            }
            else
            {
                return str;
            }
        }

        public static List<string> ToList(this string srcstr)
        {
            return ToList(srcstr, '\n');
        }

        public static List<string> ToList(this string srcstr, char linesplite)
        {
            return ToList(srcstr, linesplite, null);
        }

        public static List<string> ToList(this string srcstr, char linesplite, List<string> defaultvalue)
        {
            if (string.IsNullOrEmpty(srcstr))
            {
                return defaultvalue;
            }

            var s1 = srcstr.Split(linesplite);

            if (s1.Length <= 0)
            {
                return defaultvalue;
            }

            var temp = new List<string>(10);

            foreach (var s in s1)
            {
                temp.Add(s);
            }

            return temp;
        }

        /// <summary>
        /// 替换sql语句中的有问题符号
        /// </summary>
        public static string CheckSQL(this string str)
        {
            string str2;

            if (str == null)
            {
                str2 = "";
            }
            else
            {
                str = str.Replace("'", "''");
                str2 = str;
            }


            //if  Instr(LCase(Str),"select  ")  >  0  or  Instr(LCase(Str),"insert  ")  >  0  or  Instr(LCase(Str),"delete  ")  >  0  or  Instr(LCase(Str),"delete  from  ")  >  0  or  Instr(LCase(Str),"count(")  >  0  or  Instr(LCase(Str),"drop  table")  >  0  or  Instr(LCase(Str),"update  ")  >  0  or  Instr(LCase(Str),"truncate  ")  >  0  or  Instr(LCase(Str),"asc(")  >  0  or  Instr(LCase(Str),"mid(")  >  0  or  Instr(LCase(Str),"char(")  >  0  or  Instr(LCase(Str),"xp_cmdshell")  >  0  or  Instr(LCase(Str),"exec  master")  >  0  or  Instr(LCase(Str),"net  localgroup  administrators")  >  0    or  Instr(LCase(Str),"and  ")  >  0  or  Instr(LCase(Str),"user")  >  0  or  Instr(LCase(Str),"or  ")  >  0  then  
            //  response.redirect"index.asp" 
            //  ' Response.write(" <script  language=javascript>"  &  vbcrlf  &  "window.location.href  ='ShowError.asp?errtype="  &  errtype  &  "'"  &  vbcrlf  &  " </script>")  
            //    Response.End  
            //  end  if  


            //  Str=Replace(Str,"_","")          '过滤SQL注入_  
            //  Str=Replace(Str,"*","")          '过滤SQL注入*  
            //  Str=Replace(Str,"  ","")        '过滤SQL注入空格  
            //  Str=Replace(Str,chr(34),"")      '过滤SQL注入"  
            //  Str=Replace(Str,chr(39),"")      '过滤SQL注入'  
            //  Str=Replace(Str,chr(91),"")      '过滤SQL注入[  
            //  Str=Replace(Str,chr(93),"")      '过滤SQL注入]  
            //  Str=Replace(Str,chr(37),"")      '过滤SQL注入%  
            //  Str=Replace(Str,chr(58),"")      '过滤SQL注入:  
            //  Str=Replace(Str,chr(59),"")      '过滤SQL注入;  
            //  Str=Replace(Str,chr(43),"")      '过滤SQL注入+  
            //  Str=Replace(Str,"{","")          '过滤SQL注入{  
            //  Str=Replace(Str,"}","")          '过滤SQL注入}  
            //  sqlcheck=Str                      '返回经过上面字符替换后的Str 

            return str2;
        }

        public static string CheckSQL_Like(this string value)
        {
            string str = "";
            int startIndex = 0;
            int num2 = 0;
            while (true)
            {
                num2 = value.IndexOfAny(new char[] { '[', ']', '%', '_' }, startIndex);
                if (num2 == -1)
                {
                    break;
                }
                char ch = value[num2];
                str = str + value.Substring(startIndex, num2 - startIndex) + string.Format("[{0}]", ch.ToString());
                startIndex = num2 + 1;
            }
            return (str + value.Substring(startIndex)).Replace("'", "''");
        }

        public static string ToBase64(this string srcStr,Encoding encoding=null,bool isForUrl=false)
        {
            if (encoding==null)
            {
                encoding = Encoding.UTF8;
            }

            var str=Convert.ToBase64String(encoding.GetBytes(srcStr));

            if (isForUrl)
            {
                str = str.Replace('+', '-').Replace('/', '_');
            }

            return str;
        }

        public static string FromBase64(this string base64Str, Encoding encoding = null, bool isForUrl = false)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            if (isForUrl)
            {
                base64Str = base64Str.Replace('-', '+').Replace('_', '/');
            }

            return encoding.GetString(Convert.FromBase64String(base64Str));
        }

        private static Regex scriptRegex = new Regex(@"<script[^>]*?>.*?</script>", RegexOptions.IgnoreCase);

        /// <summary>
        /// 替换Html中的标签字符起止字符,并过滤掉脚本代码
        /// </summary>
        /// <param name="srcStr"></param>
        /// <returns></returns>
        public static string ReplaceHtml(this string srcStr)
        {
            if (string.IsNullOrWhiteSpace(srcStr))
            {
                return srcStr;
            }

            var sb = new StringBuilder(srcStr);

            sb.Replace("\r\n", "\n");
            sb.Replace("'", "&#39;");
            sb.Replace("\"", "&#34;");
            sb.Replace("<", "&#60;");
            sb.Replace(">", "&#62;");
            sb.Replace("\n", "<br />");

            var s = sb.ToStringEx();

            scriptRegex.Replace(s, "");

            return s;
        }

        /// <summary>
        /// 替换掉文本中，有关sql的危险字符
        /// </summary>
        /// <param name="srcStr"></param>
        /// <returns></returns>
        public static string ReplaceSQL(this string srcStr)
        {


            if (srcStr == String.Empty)
                return String.Empty;

            var sb = new StringBuilder(srcStr);

            sb.Replace("'", "‘");
            sb.Replace(";", "；");
            sb.Replace(",", ",");
            sb.Replace("?", "?");
            sb.Replace("<", "＜");
            sb.Replace(">", "＞");
            sb.Replace("(", "(");
            sb.Replace(")", ")");
            sb.Replace("@", "＠");
            sb.Replace("=", "＝");
            sb.Replace("+", "＋");
            sb.Replace("*", "＊");
            sb.Replace("&", "＆");
            sb.Replace("#", "＃");
            sb.Replace("%", "％");
            sb.Replace("$", "￥");

            return sb.ToStringEx();
        }

        public static T ConvertToPrimitive<T>(this string value, T defaultValue = default)
        {
            return (T) ConvertToPrimitive(value, typeof(T), defaultValue);
        }

        public static object ConvertToPrimitive(this string value, Type targetType,object defaultValue)
        {
            

            if (targetType==typeof(int))
            {
                return value.ToInt((int)defaultValue);
            }
            else if (targetType == typeof(uint))
            {
                return value.ToUInt((uint) defaultValue);
            }
            else if (targetType == typeof(long))
            {
                return value.ToLong((long) defaultValue);
            }
            else if (targetType == typeof(ulong))
            {
                return value.ToULong((ulong) defaultValue);
            }
            else if (targetType == typeof(short))
            {
                return value.ToInt16((short) defaultValue);
            }
            else if (targetType==typeof(bool))
            {
                return value.ToBool((bool) defaultValue);
            }
            else if (targetType == typeof(float))
            {
                return value.ToFloat((float) defaultValue);
            }
            else if (targetType == typeof(double))
            {
                return value.ToDouble((double) defaultValue);
            }
            else if (targetType == typeof(decimal))
            {
                return value.ToDecimal((decimal) defaultValue);
            }
            else if(targetType==typeof(int?))
            {
                return value.ToIntNullable((int?) defaultValue);
                
            }
            else if (targetType == typeof(short?))
            {
                return value.ToInt16Nullable((short?) defaultValue);
            }
            else if (targetType == typeof(uint?))
            {
                return value.ToUIntNullable((uint?) defaultValue);
            }
            else if (targetType == typeof(long?))
            {
                return value.ToLongNullable((long?) defaultValue);
            }
            else if (targetType == typeof(ulong?))
            {
                return value.ToULongNullable((ulong?) defaultValue);
            }
            else if (targetType == typeof(float?))
            {
                return value.ToFloatNullable((float?) defaultValue);
            }
            else if (targetType == typeof(double?))
            {
                return value.ToDoubleNullable((double?) defaultValue);
            }
            else if (targetType == typeof(decimal?))
            {
                return value.ToDecimalNullable((decimal?) defaultValue);
            }
            else if (targetType == typeof(bool?))
            {
                return value.ToBoolNullable((bool?) defaultValue);
            }
            else if (targetType == typeof(byte))
            {
                return value.ToByte((byte) defaultValue);
            }
            else if (targetType == typeof(byte?))
            {
                return value.ToByteNullable((byte?) defaultValue);
            }

            throw new ArgumentOutOfRangeException();
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLineEx(this StringBuilder sb, string str, params object[] param)
        {
            return sb.AppendLine(string.Format(str, param));
        }

        public static int IndexOf(this StringBuilder sb, string s)
        {
            return IndexOf(sb, s, 0);
        }

        public static int IndexOf(this StringBuilder sb, string s, int startIndex)
        {
            // Note: This does a StringComparison.Ordinal kind of comparison.

            if (sb == null)
                throw new ArgumentNullException("sb");
            if (s == null)
                s = string.Empty;

            if (startIndex < 0 || startIndex > sb.Length - 1)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            var sbLength = sb.Length;
            var slength = s.Length;

            for (int i = startIndex; i < sbLength; i++)
            {
                int j;
                for (j = 0; j < slength && i + j < sbLength && sb[i + j] == s[j]; j++) ;
                if (j == s.Length)
                    return i;
            }

            return -1;
        }
    }
}
