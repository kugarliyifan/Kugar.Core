using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Linq
{
    internal static class Error
    {
        public static Exception ArgumentNull(string argS)
        {
            return new ArgumentNullException(argS);
        }

        public static Exception ArgumentOutOfRange(string argName)
        {
            return new ArgumentOutOfRangeException(argName);
        }

        public static Exception NotSupported()
        {
            return new NotSupportedException();
        }
    }
}
