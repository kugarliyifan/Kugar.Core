using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    public class ArraySegmentEx<T>
    {
        public ArraySegmentEx(T[] array, int offset, int count)
        {
            this.Array = array;
            Offset = offset;
            Count = count;
        }

        public int Offset { set; get; }

        public int Count { set; get; }

        public T[] Array { get; private set; }

        public static implicit operator ArraySegment<T>(ArraySegmentEx<T> s)
        {
            return new ArraySegment<T>(s.Array, s.Offset, s.Count);
        }
    }
}
