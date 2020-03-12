using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    public class ArraySlice<T>:IEnumerable<T>
    {
        public ArraySlice(T[] source, int offset, int count)
        {
            Offset = offset;
            Source = source;
            Count = count;
        }
        
        public int Offset { private set; get; }

        public int Count { private set; get; }

        public T[] Source {private set; get; }

        public T this[int index]
        {
            get { return Source[index + Offset]; }
            set { Source[index + Offset] = value; }
        }

        public static implicit operator ArraySegment<T>(ArraySlice<T> source)
        {
            return new ArraySegment<T>(source.Source,source.Offset,source.Count);
        }

        public static implicit operator ArraySlice<T>(ArraySegment<T> source)
        {
            return new ArraySlice<T>(source.Array, source.Offset, source.Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Offset; i < Offset+Count; i++)
            {
                yield return Source[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
