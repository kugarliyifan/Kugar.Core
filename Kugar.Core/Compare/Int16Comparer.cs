using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Compare
{
    public class Int16Comparer : IComparer<Int16>
    {
        public int Compare(short x, short y)
        {
            return CompareTo(x, y);
        }

        public static int CompareTo(short x, short y)
        {
            if (x > y)
            {
                return 1;
            }
            else if (x < y)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
