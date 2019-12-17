using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class DateTimeExt
    {
        /// <summary>
        ///     修改日期中的年份
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="year">要修改的日期值,取值范围:0001-9999,该取值需按照正确日期取值,否则将抛出错误</param>
        /// <returns>返回设置了日期值的新的datetime值</returns>
        public static DateTime SetYear(this DateTime dt, int year)
        {
            
            return new DateTime(year, dt.Month, dt.Day, dt.Minute, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        /// <summary>
        ///     修改日期中的月份
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="month">要修改的日期值,取值范围:1-12,该取值需按照正确日期取值,否则将抛出错误</param>
        /// <returns>返回设置了日期值的新的datetime值</returns>
        public static DateTime SetMonth(this DateTime dt, int month)
        {
            if (!(month>=1 && month<=12))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new DateTime(dt.Year, month, dt.Day, dt.Minute, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        

        /// <summary>
        ///     修改日期中的日期
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="day">要修改的日期值,取值范围:1-31,该取值需按照正确日期取值,否则将抛出错误</param>
        /// <returns>返回设置了日期值的新的datetime值</returns>
        public static DateTime SetDay(this DateTime dt, int day)
        {
            if (!(day>=1 && day<=31))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new DateTime(dt.Year, dt.Month, day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        /// <summary>
        ///     修改日期中的小时数
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hours">要修改的小时值,取值范围:0-23</param>
        /// <returns>返回设置了小时值的新的datetime值</returns>
        public static DateTime SetHour(this DateTime dt,int hours)
        {
            if (hours.IsNotBetween(0,23))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new DateTime(dt.Year, dt.Month, dt.Day, hours,dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        /// <summary>
        ///     修改日期中的分钟数
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="minute">要修改的分钟值,取值范围:0-59</param>
        /// <returns>返回设置了小时值的新的datetime值</returns>
        public static DateTime SetMinute(this DateTime dt, int minute)
        {
            if (minute.IsNotBetween(0, 59))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Minute, minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        /// <summary>
        ///     修改日期中的秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="second">要修改的分钟值,取值范围:0-59</param>
        /// <returns>返回设置了秒数值的新的datetime值</returns>
        public static DateTime SetSecond(this DateTime dt, int second)
        {
            if (second.IsNotBetween(0, 59))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second, dt.Millisecond, dt.Kind);
        }

        public static DateTime SetTime(this DateTime dt, int hour = -1, int mintue = -1, int second =-1,int millisecond=-1)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, hour!=-1?hour:dt.Hour, mintue != -1 ? mintue : dt.Minute, second != -1 ? second : dt.Second, millisecond != -1 ? millisecond : dt.Millisecond, dt.Kind);
        }

        public static DateTime SetDate(this DateTime dt, int year = -1, int month = -1, int day = -1)
        {
            return new DateTime(year!=-1? year:dt.Year, month!=-1? month:dt.Month, day!=-1?day:dt.Day, dt.Minute, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }

        public static DateTime SetStartTime(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
        }

        public static DateTime SetEndTime(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
        }

        /// <summary>
        /// 设置每月的第一天,时间为00:00:00
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime SetStartMonthDay(this DateTime dt)
        {
            return dt.SetDay(1).SetStartTime();
        }

        /// <summary>
        /// 返回每月的第一天,时间为00:00:00
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime? SetStartMonthDay(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return dt.Value.SetDay(1).SetStartTime();
        }

        /// <summary>
        /// 返回每月的最后一天,时间为23:59:59
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime SetEndMonthDay(this DateTime dt)
        {
            return dt.AddDays(1 - dt.Day).AddMonths(1).AddDays(-1).SetEndTime();
        }

        /// <summary>
        /// 返回每月的最后一天,时间为23:59:59
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime? SetEndMonthDay(this DateTime? dt)
        {
            if (dt==null)
            {
                return null;
            }

            var tmp = dt.Value;

            return tmp.AddDays(1 - tmp.Day).AddMonths(1).AddDays(-1).SetEndTime();
        }

        public static DateTime ThisWeekStart(this DateTime dt)
        {
            return dt.ThisWeek(DayOfWeek.Sunday);
        }

        public static DateTime ThisWeekEnd(this DateTime dt)
        {
            return dt.ThisWeek(DayOfWeek.Saturday);
        }

        public static DateTime? SetStartTime(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return null;
            }


            return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, 0, 0, 0);
        }

        public static DateTime? SetEndTime(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return null;
            }

            return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, 23, 59, 59);
        }

        public static DateTime NextWeek(this DateTime dt)
        {
            return NextWeek(dt, dt.DayOfWeek);
        }

        public static DateTime NextWeek(this DateTime dt,DayOfWeek week)
        {
            var td = Math.Abs((week == DayOfWeek.Sunday ? 7 : (int)week )- (dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)week));



            return dt.AddDays(td==0?7:td);
        }

        public static DateTime ThisWeek(this DateTime dt,DayOfWeek week)
        {
            if (week==dt.DayOfWeek)
            {
                return dt;
            }

            return dt.AddDays(-1*((int) dt.DayOfWeek - (int) week));
        }

        public static DateTime ThisMonthEnd(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), 23, 59, 59);
        }

        public static DateTime ThisMonthStart(this DateTime dt)
        {
            return new DateTime(dt.Year,dt.Month, 1,0,0,0);
        }

        public static DateTime ThisYearEnd(this DateTime dt)
        {
            return new DateTime(dt.Year, 12, DateTime.DaysInMonth(dt.Year,12), 23, 59, 59);
        }

        public static DateTime ThisYearStart(this DateTime dt)
        {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0);
        }

        public static string ToStringEx(this DateTime dt,string format)
        {
            if (dt==DateTime.MinValue || dt==DateTime.MinValue)
            {
                return "";
            }
            else
            {
                return dt.ToString(format);
            }
        }

        public static string ToStringEx(this DateTime? dt, string format)
        {
            if (!dt.HasValue)
            {
                return "";
            }

            return ToStringEx(dt.Value, format);
        }

        /// <summary>
        ///     将UTC时间转换为当前系统的本地时间
        /// </summary>
        /// <param name="dateTimeUtc">源UTC时间</param>
        /// <returns></returns>
        public static DateTime ToLocalDateTime(this DateTimeOffset dateTimeUtc)
        {
            return ToLocalDateTime(dateTimeUtc,null);
        }

        /// <summary>
        ///     将UTC时间转换为指定的当地时间
        /// </summary>
        /// <param name="dateTimeUtc">源UTC时间</param>
        /// <param name="localTimeZone">目的本地时间</param>
        /// <returns></returns>
        public static DateTime ToLocalDateTime(this DateTimeOffset dateTimeUtc, TimeZoneInfo localTimeZone)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);

            return TimeZoneInfo.ConvertTime(dateTimeUtc, localTimeZone).DateTime;
        }

        /// <summary>
        /// 指定日期是否在范围内
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startDt"></param>
        /// <param name="endDt"></param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime dt, DateTime? startDt, DateTime? endDt)
        {
            if (startDt.HasValue && dt<startDt.Value)
            {
                return false;
            }

            if (endDt.HasValue && dt>endDt.Value)
            {
                return false;
            }

            return true;
        }


        public static bool Equals(this DateTime srcTime,DateTime compareTime,DateTimeCompareParts partsFlag)
        {
            if (partsFlag.IsContans(DateTimeCompareParts.Tick) && srcTime.Ticks != compareTime.Ticks) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Year) && srcTime.Year != compareTime.Year) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Month) && srcTime.Month != compareTime.Month) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Day) && srcTime.Day != compareTime.Day) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Hour) && srcTime.Hour != compareTime.Hour) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Minute) && srcTime.Minute != compareTime.Minute) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Second) && srcTime.Second != compareTime.Second) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Millisecond) && srcTime.Millisecond != compareTime.Millisecond) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Date) && srcTime.Date != compareTime.Date) return false;
            if (partsFlag.IsContans(DateTimeCompareParts.Time) &&
                (srcTime.Hour != compareTime.Hour || srcTime.Minute != compareTime.Minute ||
                 srcTime.Second != compareTime.Second || srcTime.Millisecond != compareTime.Millisecond)) return false;

            return true;
        }

        private static readonly long DatetimeMinTimeTicks =
            (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        /// <summary>
        /// 转为js所使用毫秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToJavaScriptMilliseconds(this DateTime dt)
        {
            return (long)((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
        }

        /// <summary>
        /// 转为js所使用的秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToJavaScriptSeconds(this DateTime dt)
        {
            return (long)((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000000);
        }

        /// <summary>
        /// 从秒数转换为datetime,本地时间
        /// </summary>
        /// <param name="utfSec"></param>
        /// <returns></returns>
        public static DateTime ToLocalDatetimeFromUTCSeconds(this int utfSec)
        {
            var s = (long) utfSec;

            DateTime dt = new DateTime(s*1000000 + DatetimeMinTimeTicks,DateTimeKind.Local);//转化为DateTime

            return dt;
        }

        /// <summary>
        /// 从毫秒数转换为datetime,本地时间
        /// </summary>
        /// <param name="utfSec"></param>
        /// <returns></returns>
        public static DateTime ToLocalDatetimeFromUTCMilliseconds(this int utfSec)
        {
            DateTime dt = new DateTime(utfSec * 1000 + DatetimeMinTimeTicks, DateTimeKind.Local);//转化为DateTime

            return dt;
        }

        /// <summary>
        /// 从秒数转换为datetime,本地时间
        /// </summary>
        /// <param name="utfSec"></param>
        /// <returns></returns>
        public static DateTime ToLocalDatetimeFromUTCSeconds(this long utfSec)
        {
            DateTime dt = new DateTime(utfSec * 1000000 + DatetimeMinTimeTicks, DateTimeKind.Local);//转化为DateTime

            return dt;
        }

        /// <summary>
        /// 从毫秒数转换为datetime,本地时间
        /// </summary>
        /// <param name="utfSec"></param>
        /// <returns></returns>
        public static DateTime ToLocalDatetimeFromUTCMilliseconds(this long utfSec)
        {
            DateTime dt = new DateTime(utfSec * 1000000 + DatetimeMinTimeTicks, DateTimeKind.Local);//转化为DateTime

            return dt;
        }
    }

    [Flags]
    public enum DateTimeCompareParts
    {
        Tick=0,
        Year=1,
        Month=2,
        Day=4,
        Hour=8,
        Minute=16,
        Second=32,
        Millisecond=64,
        Date=128,
        Time=256
    }
}
