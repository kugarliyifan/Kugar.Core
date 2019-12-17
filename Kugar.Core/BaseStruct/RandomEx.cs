using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    public static class RandomEx
    {
        private static readonly Random defaultValue=new Random();

        public static int Next(int maxValue)
        {
            return defaultValue.Next(maxValue);
        }

        public static int Next()
        {
            return defaultValue.Next();
        }

        public static int Next(int minValue,int maxValue)
        {
            return defaultValue.Next(minValue, maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            defaultValue.NextBytes(buffer);
        }

        public static double NextDouble()
        {
            return defaultValue.NextDouble();
        }

        public static decimal NextDecimal(decimal minValue,decimal maxValue)
        {

            return (decimal)defaultValue.NextDouble()*(maxValue - minValue) + minValue;
            //return Math.Round(, Len);
        }

        public static decimal NextDecimal(decimal minValue,decimal maxValue,int len)
        {

            return Math.Round((decimal)defaultValue.NextDouble()*(maxValue - minValue) + minValue,len);
            //return Math.Round(, Len);
        }

        public static char[] DefaultCharacter = {'1', '2', '3', '4', '5', '6', '8', '9', 
                                                                     'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H','I', 'J', 'K', 'L', 'M', 'N','O', 'P','Q', 'R', 'S', 'T','U','V', 'W', 'X', 'Y' ,'Z',
                                                                     'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h','i', 'j', 'k', 'l', 'm', 'n','o', 'p','q', 'r', 's', 't','u','v', 'w', 'x', 'y','z' 
                                                                    };

        /// <summary>
        ///     生成随机字符
        /// </summary>
        /// <param name="charLength"></param>
        /// <returns></returns>
        public static string NextString(int charLength)
        {
            return NextString(DefaultCharacter, charLength);
        }

        /// <summary>
        ///     生成随机字符
        /// </summary>
        /// <param name="randomCharSet"></param>
        /// <param name="charLength"></param>
        /// <returns></returns>
        public static string NextString(char[] randomCharSet,int charLength)
        {
            if (charLength<=0)
            {
                throw new ArgumentOutOfRangeException("charLength");
            }

            var s = new char[charLength];

            var length = randomCharSet.Length;
            for (int i = 0; i < charLength; i++)
            {
                s[i] = randomCharSet[Next(0, length)];
            }

            return new string(s);
        }
    }
}
