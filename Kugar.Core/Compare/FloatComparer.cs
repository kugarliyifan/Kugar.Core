using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Compare
{
    public class FloatComparer : IComparer<float>
    {
        private static float _compareValue = (float)Math.Pow(10, -6);

        public int Compare(float x, float y)
        {
            return CompareTo(x, y);
        }

        public static int CompareTo(float x, float y)
        {
            if ((x - y) < _compareValue)
            {
                return 0;
            }

            if (x > y)
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }
    }
}
