using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    /// 用于在foreach或者需要传入IEnumerable的时候,合并多个列表,或多个数据,避免使用 <br/>
    /// 避免使用concat函数造成多个数组的浪费
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JoinMultiSouceToEnumable<T>:IEnumerable<T>
    {
        private IEnumerable<T> _src1 = null;
        private IEnumerable<T> _src2 = null;

        public JoinMultiSouceToEnumable(T src1, IEnumerable<T> src2)
        {
            _src1 = new JoinMultiEnumable_1<T>(src1);
            _src2 = src2;
        }

        public JoinMultiSouceToEnumable(T src1, T src2, IEnumerable<T> src3)
        {
            _src1 = new JoinMultiEnumable_2<T>(src1, src2);
            _src2 = src3;
        }

        public JoinMultiSouceToEnumable(T src1, T src2, T src3, IEnumerable<T> src4)
        {
            _src1 = new JoinMultiEnumable_3<T>(src1, src2,src3);
            _src2 = src4;
        }

        public JoinMultiSouceToEnumable(T src1, T src2, T src3, T src4, IEnumerable<T> src5)
        {
            _src1 = new JoinMultiEnumable_4<T>(src1, src2, src3,src4);
            _src2 = src5;
        }

        public JoinMultiSouceToEnumable(IEnumerable<T> src1,params T[] src2)
        {
            _src1 = src1;
            _src2 = src2;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_src1!=null)
            {
                foreach (var item in _src1)
                {
                    yield return item;
                }
            }

            if (_src2!=null)
            {
                foreach (var item in _src2)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class JoinMultiEnumable_1<T> : IEnumerable<T>
        {
            private T _src1;

            public JoinMultiEnumable_1(T src1)
            {
                _src1 = src1;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return _src1;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class JoinMultiEnumable_2<T> : IEnumerable<T>
        {
            private T _src1;
            private T _src2;

            public JoinMultiEnumable_2(T src1, T src2)
            {
                _src1 = src1;
                _src2 = src2;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return _src1;
                yield return _src2;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class JoinMultiEnumable_3<T> : IEnumerable<T>
        {
            private T _src1;
            private T _src2;
            private T _src3;

            public JoinMultiEnumable_3(T src1, T src2, T src3)
            {
                _src1 = src1;
                _src2 = src2;
                _src3 = src3;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return _src1;
                yield return _src2;
                yield return _src3;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class JoinMultiEnumable_4<T> : IEnumerable<T>
        {
            private T _src1;
            private T _src2;
            private T _src3;
            private T _src4;

            public JoinMultiEnumable_4(T src1, T src2, T src3,T src4)
            {
                _src1 = src1;
                _src2 = src2;
                _src3 = src3;
                _src4 = src4;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return _src1;
                yield return _src2;
                yield return _src3;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
