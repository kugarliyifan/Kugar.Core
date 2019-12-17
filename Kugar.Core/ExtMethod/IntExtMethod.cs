using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.Exceptions;

namespace Kugar.Core.ExtMethod
{
    public static class IntExtMethod
    {
        public static T ToEnum<T>(this int value, T defaultValue)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentTypeNotMatchException("T","Enum");
            }

            if (Enum.IsDefined(typeof(T), value))
            {
                return (T)Enum.ToObject(typeof(T), value);
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
