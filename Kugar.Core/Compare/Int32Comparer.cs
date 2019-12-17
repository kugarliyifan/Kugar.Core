using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Compare
{
    public class Int32Comparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return CompareTo(x, y);
        }

        public static int CompareTo(int x, int y)
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
