using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class ByteArrayAbout
    {
        public static string JoinToString(this byte[] n)
        {
            if (n == null || n.Length <= 0)
            {
                return string.Empty;
            }

            return JoinToString(n, "");
        }

        /// <summary>
        /// 保存指定二进制数据到文件中，如果文件所在文件夹不存在，则自动创建
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath"></param>
        public static void SaveToFile(this byte[] data,string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllBytes(filePath, data);
        }
        
        public static string JoinToString(this byte[] n, string splite)
        {
            if (n == null || n.Length <= 0)
            {
                return string.Empty;
            }

            return JoinToString(n, splite, 0, n.Length);
        }

        public static string JoinToString(this byte[] n, string splite, int start, int length)
        {
            return JoinToString(n, splite, start, length, "X2");
        }

        /// <summary>
        ///     将一个byte数组链接成一个字符串
        /// </summary>
        /// <param name="n">源byte数组</param>
        /// <param name="splite">用于两个byte中间的字符串</param>
        /// <param name="start">起始索引</param>
        /// <param name="length">结束索引</param>
        /// <param name="strformat">byte的格式化字符串</param>
        /// <returns></returns>
        public static string JoinToString(this byte[] n, string splite, int start, int length, string strformat)
        {
            if (n == null || n.Length <= 0 || start > n.Length || start < 0 || length <= 0 || start + length > n.Length)
            {
                return string.Empty;
            }

            var s = new StringBuilder(length * 2);

            for (int i = start; i < start + length; i++)
            {
                s.Append(n[i].ToString(strformat) + splite);
            }

            //s = s.Remove(s.Length - 1, 1);

            

            if (!string.IsNullOrEmpty(splite))
            {
                //if (s[s.Length - 1] == splite[0])
                {
                    s = s.Remove(s.Length - 1, splite.Length);
                }
                
            }


            return s.ToStringEx();
        }

    }
}
