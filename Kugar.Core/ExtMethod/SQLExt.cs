using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public class SQLExt
    {
        /// <summary>
        /// 将SQL的timestamp类型字段转换为long型
        /// </summary>
        /// <param name="timestamp">时间戳字段值,该字段映射到EF中为byte数组</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static long SQLTimestampToLong(byte[] timestamp, long defaultValue = 0l)
        {
            var s = string.Concat(timestamp.Select(b => b.ToString("X1")).ToArray());

            var dt = 0L;

            if (Int64.TryParse(s, NumberStyles.HexNumber, null, out dt))
            {
                return dt;
            }
            else
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// 将long型值转换为EF中timestamp类型对应的byte数组
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static byte[] LongToSQLTimestampBytes(long timestamp)
        {
            var bytes = BitConverter.GetBytes(timestamp).Reverse().ToArray();

            return bytes;
        }

        /// <summary>
        /// 将long型值转换为EF中timestamp类型对应的字符串格式为 0xXXXXXXX <br/>
        /// 转换后的字符串,可用于查询语句的拼接
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string LongToSQLTimestampStr(long timestamp, string defaultValue)
        {
            var bytes = LongToSQLTimestampBytes(timestamp);

            var str = "0x" + string.Concat(bytes.Select(b => b.ToString("X1")).ToArray()).PadLeft(16, '0');

            return str;
        }
    }
}
