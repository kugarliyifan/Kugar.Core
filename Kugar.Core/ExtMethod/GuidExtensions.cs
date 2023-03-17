using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class GuidExtensions
    {
        public static Guid Copy(this Guid src)
        {
            var t = src.ToByteArray();

            return new Guid(t);
        }

        public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;

        public static bool IsEmpty(this Guid? guid) => guid == null || guid.Value == Guid.Empty;
    }
}
