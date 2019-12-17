using System;
using System.Collections.Generic;
using System.Threading;

namespace Kugar.Core.ExtMethod
{
    public static class NumericValueAbout
    {
        public static string ToStringEx(this int n)
        {
            return ToStringEx(n, 10);
        }

        public static string ToStringEx(this int n, int jinzhi)
        {
            return Convert.ToString(n, jinzhi).ToUpper();
        }

        public static string ToStringEx(this int? n, string defaultValue, int jinzhi)
        {
            return n.HasValue ? Convert.ToString(n.Value, jinzhi).ToUpper() : defaultValue;
        }

        public static string ToStringEx(this decimal v, string format="")
        {
            return v.ToString(format);
        }

        public static string ToStringEx(this decimal? v, string format = "", string defaultValue = "")
        {
            return v.HasValue ?ToStringEx(v.Value, format) : defaultValue;
        }

        public static string ToStringEx(this double v, string format = "")
        {
            return v.ToString(format);
        }

        public static string ToStringEx(this double? v, string format = "", string defaultValue = "")
        {
            return v.HasValue ?ToStringEx(v.Value, format) : defaultValue;
        }

        public static string ToStringEx(this float v, string format = "")
        {
            return v.ToString(format);
        }

        public static string ToStringEx(this float? v, string format = "", string defaultValue = "")
        {
            return v.HasValue ?ToStringEx(v.Value, format) : defaultValue;
        }

        public static string ToStringEx(this long v, string format = "")
        {
            return v.ToString(format);
        }

        public static string ToStringEx(this long? v, string format = "", string defaultValue = "")
        {
            return v.HasValue ?ToStringEx(v.Value, format) : defaultValue;
        }

        /// <summary>
        ///     返回指定int的第index位是1还是0,1返回true,0返回false
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index">第index位</param>
        /// <returns></returns>
        public static bool GetBit(this int i, int index)
        {
            if (index <= 0 || index > 32)
            {
                throw new ArgumentOutOfRangeException("index", "index值只能在1到32之内");
                return false;
            }

            int p = (int)Math.Pow(2, index);

            if ((i & p) == p)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     返回指定ushort的第index位是1还是0,1返回true,0返回false
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index">第index位</param>
        /// <returns></returns>
        public static bool GetBit(this UInt16 i, ushort index)
        {
            if (index <= 0 || index > 16)
            {
                throw new ArgumentOutOfRangeException("index", "index值只能在1到16之内");
                //return false;
            }

            ushort p = (ushort)Math.Pow(2, index);

            if ((i & p) == p)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     判断两个float是否相等,当两个float的差值小于10^-6时,判断为相等
        /// </summary>
        /// <param name="n">第一个值</param>
        /// <param name="value">第二个值</param>
        /// <returns></returns>
        public static bool IsEquals(this float n, float value)
        {
            if ((n - value) < (Math.Pow(10, -6)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     判断testingValue的值是否在  n～ n+addValue 值中间,用于当 n+addValue值可能出现溢出回环现象时的判断 <br/>
        ///     当出现溢出回环时，，如果testingValue大于n或者小于回环后的值，则返回true；<br/>
        ///     当出现溢出时，isOverflowCheck为true时，则抛出溢出错误，为false时，则不检查溢出
        /// </summary>
        /// <param name="n">判断的下边界</param>
        /// <param name="addValue">增加的值</param>
        /// <param name="testingValue">要判断的值</param>
        /// <param name="isOverflowCheck">是否做溢出回环判断</param>
        /// <returns></returns>
        public static bool AddAndCheckRang(this int n, int addValue, int testingValue, bool isOverflowCheck=false)
        {
            int tempValue = 0;

            Interlocked.Exchange(ref tempValue, n);

            if (isOverflowCheck)
            {
                tempValue += addValue;

                if (testingValue > n && testingValue < tempValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                unchecked
                {
                    tempValue += addValue;
                }

                if (n < int.MaxValue - addValue)
                {
                    if (testingValue > n && testingValue<tempValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (testingValue > n || testingValue < tempValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        /// <summary>
        ///     判断testingValue的值是否在  n～ n+addValue 值中间,用于当 n+addValue值可能出现溢出回环现象时的判断 <br/>
        ///     当出现溢出回环时，，如果testingValue大于n或者小于回环后的值，则返回true；<br/>
        ///     当出现溢出时，isOverflowCheck为true时，则抛出溢出错误，为false时，则不检查溢出
        /// </summary>
        /// <param name="n">判断的下边界</param>
        /// <param name="addValue">增加的值</param>
        /// <param name="testingValue">要判断的值</param>
        /// <param name="isOverflowCheck">是否做溢出回环判断</param>
        /// <returns></returns>
        public static bool AddAndCheckRang(this short n, short addValue, short testingValue, bool isOverflowCheck = false)
        {
            short tempValue = n;

            if (isOverflowCheck)
            {
                tempValue += addValue;

                if (testingValue > n && testingValue < tempValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                unchecked
                {
                    tempValue += addValue;
                }

                if (n < short.MaxValue - addValue)
                {
                    if (testingValue > n && testingValue < tempValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (testingValue > n || testingValue < tempValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }



        /// <summary>
        ///     判断指定的int值是否在min与max之间
        /// </summary>
        /// <param name="n">指定的数值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static bool IsBetween(this int n, int min, int max)
        {
            if (n >= min && n <= max)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     判断指定的int值是否不在min与max之间
        /// </summary>
        /// <param name="n">指定的数值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static bool IsNotBetween(this int n, int min, int max)
        {
            return !IsBetween(n, min, max);
        }

        public static bool IsBetween(this long n, long min, long max)
        {
            if (n >= min && n <= max)
            {
                return true;
            }

            return false;
        }

        public static bool IsNoBetween(this long n, long min, long max)
        {
            return !IsBetween(n, min, max);
        }

        public static bool IsBetween(this decimal n,decimal min,decimal max)
        {
            if (n >= min && n <= max)
            {
                return true;
            }

            return false;
        }

        public static bool IsNotBetween(this decimal n,decimal min,decimal max)
        { 
            return !IsBetween(n,min,max);
        }

        /// <summary>
        ///     判断指定的int值是否在给定的值列表内
        /// </summary>
        /// <param name="n">指定的数值</param>
        /// <param name="lst">给定的匹配列表</param>
        /// <returns></returns>
        public static bool IsInList(this int n, IEnumerable<int> lst)
        {
            foreach (var i in lst)
            {
                if (i == n)
                {
                    return true;
                }
            }

            return false;
        }


        public static int DivRem(this int n, int divnum)
        {
            int i = 0;

            Math.DivRem(n, divnum, out i);

            return i;
        }

        public static long DivRem(this long n, long divnum)
        {
            long i = 0;

            Math.DivRem(n, divnum, out i);

            return i;
        }





        public static float Round(this float n, int count)
        {
            return (float)Math.Round(n, count);
        }

        public static double Round(this double n, int count)
        {
            return Math.Round(n, count);
        }

        /// <summary>
        ///     递增一个Int型值,默认为不检测溢出
        /// </summary>
        /// <param name="src">原值</param>
        /// <returns>返回递增后的值</returns>
        public static int Increment(this int src)
        {
            return Increment(src, true);
        }

        /// <summary>
        ///     递增一个Int型值
        /// </summary>
        /// <param name="src">原值</param>
        /// <param name="isUnChecked">是否不检测溢出</param>
        /// <returns>返回递增后的值</returns>
        public static int Increment(this int src, bool isUnChecked)
        {
            int ret = src;

            if (isUnChecked)
            {
                unchecked
                {
                    ++ret;
                }
            }
            else
            {
                ++ret;
            }

            return ret;
        }

        /// <summary>
        ///     递增一个short型值,默认为不检测溢出
        /// </summary>
        /// <param name="src">原值</param>
        /// <returns>返回递增后的值</returns>
        public static short Increment(this short src)
        {
            return Increment(src, true);
        }

        /// <summary>
        ///     递增一个short型值
        /// </summary>
        /// <param name="src">原值</param>
        /// <param name="isUnChecked">是否不检测溢出</param>
        /// <returns>返回递增后的值</returns>
        public static short Increment(this short src, bool isUnChecked)
        {
            short ret = src;

            if (isUnChecked)
            {
                unchecked
                {
                    ++ret;
                }
            }
            else
            {
                ++ret;
            }

            return ret;
        }


        /// <summary>
        ///     在不检查溢出的情况下,在原值上增加value值
        /// </summary>
        /// <param name="src">原值</param>
        /// <param name="value">将要增加的值</param>
        /// <returns>返回递增后的值</returns>
        public static short AddUnChecked(this short src, short value)
        {
            short ret = src;

            unchecked
            {
                ret += value;
            }

            return ret;
        }


        public static int ToMinInt(this int value,int minValue)
        {
            if (value>=minValue)
            {
                return value;
            }
            else
            {
                return minValue;
            }
        }

        public static int ToMaxInt(this int value,int maxValue)
        {
            if (value <= maxValue)
            {
                return value;
            }
            else
            {
                return maxValue;
            }
        }

        public static int ToBetween(this int value,int minValue,int maxValue)
        {
            if (value >=minValue && value<=maxValue)
            {
                return value;
            }
            else if(value>maxValue)
            {
                return maxValue;
            }
            else
            {
                return minValue;
            }
        }


        /// <summary>
        /// 将小数值按指定的小数位数截断
        /// </summary>
        /// <param name="d">要截断的小数</param>
        /// <param name="s">小数位数，s大于等于0，小于等于28</param>
        /// <returns></returns>
        public static decimal ToFixed(this decimal d, int s)
        {
            decimal sp = Convert.ToDecimal(Math.Pow(10, s));

            if (d < 0)
                return Math.Truncate(d) + Math.Ceiling((d - Math.Truncate(d)) * sp) / sp;
            else
                return Math.Truncate(d) + Math.Floor((d - Math.Truncate(d)) * sp) / sp;
        }

        public static double ToFixed(this double d, int s)
        {
            var str = d.ToString();

            var i = str.IndexOf('.');

            if (i<0)
            {
                return d;
            }

            var digit = str.Substring(i + 1, s);

            var intValue = Math.Truncate(d);

            return string.Format("{0}.{1}", intValue, digit).ToDouble();
        }
    }
}
