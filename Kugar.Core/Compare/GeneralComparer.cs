using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Compare
{
    public delegate int GetHashCodeDelgate<T>(T obj);

    public delegate bool EqualsFunc<T>(T x, T y);

    public delegate int CompareFunc<T>(T x, T y);

    /// <summary>
    ///     自定义相等判断的比较类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomEqualityComparer<T> : EqualityComparer<T>
    {
        private EqualsFunc<T> _equalFunc;
        private GetHashCodeDelgate<T> _gethashCode;

        /// <summary>
        ///     定义比较函数
        /// </summary>
        /// <param name="equalFunc">Equals的实现函数</param>
        public CustomEqualityComparer(EqualsFunc<T> equalFunc)
            : this(equalFunc, null)
        { }

        /// <summary>
        ///     定义比较函数
        /// </summary>
        /// <param name="equalFunc">Equals的实现函数</param>
        /// <param name="gethashCode">GetHashCode的实现函数</param>
        public CustomEqualityComparer(EqualsFunc<T> equalFunc, GetHashCodeDelgate<T> gethashCode)
        {
            _equalFunc = equalFunc;
            _gethashCode = gethashCode;
        }

        #region Implementation of IEqualityComparer<T>

        public override bool Equals(T x, T y)
        {
            if (_equalFunc != null)
            {
                return _equalFunc(x, y);
            }

            //如果未定义实现函数,则返回false
            return false; 
            return object.Equals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            if (_gethashCode != null)
            {
                return _gethashCode(obj);
            }

            //如果未定义实现函数,则返回0
            return 0; 
            return obj.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    ///     通用的相等比较类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        #region Functions

        public bool Equals(T x, T y)
        {
            if (!typeof(T).IsValueType
                || (typeof(T).IsGenericType
                && typeof(T).GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (Object.Equals(x, default(T)))
                    return Object.Equals(y, default(T));
                if (Object.Equals(y, default(T)))
                    return false;
            }
            if (x.GetType() != y.GetType())
                return false;
            if (x is IEnumerable && y is IEnumerable)
            {
                GenericEqualityComparer<object> Comparer = new GenericEqualityComparer<object>();
                IEnumerator XEnumerator = ((IEnumerable)x).GetEnumerator();
                IEnumerator YEnumerator = ((IEnumerable)y).GetEnumerator();
                while (true)
                {
                    bool XFinished = !XEnumerator.MoveNext();
                    bool YFinished = !YEnumerator.MoveNext();
                    if (XFinished || YFinished)
                        return XFinished & YFinished;
                    if (!Comparer.Equals(XEnumerator.Current, YEnumerator.Current))
                        return false;
                }
            }
            if (x is IEquatable<T>)
                return ((IEquatable<T>)x).Equals(y);
            if (x is IComparable<T>)
                return ((IComparable<T>)x).CompareTo(y) == 0;
            if (x is IComparable)
                return ((IComparable)x).CompareTo(y) == 0;
            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    ///     自定义大小的比较类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomComparer<T>:IComparer<T>
    {
        /// <summary>
        ///     定义判断函数
        /// </summary>
        /// <param name="compareFunc">比较大小的自定义函数</param>
        public CustomComparer(CompareFunc<T> compareFunc)
        {
            CompareFunc = compareFunc;
        }

        public CompareFunc<T> CompareFunc { set; get; }

        public int Compare(T x, T y)
        {
            if (CompareFunc!=null)
            {
                return this.CompareFunc(x, y);
            }

            return 0;
        }
    }

    /// <summary>
    ///     通用的大小比较类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericComparer<T> : IComparer<T> where T : IComparable
    {
        #region Functions

        public int Compare(T x, T y)
        {
            if (!typeof(T).IsValueType
                || (typeof(T).IsGenericType
                && typeof(T).GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (Object.Equals(x, default(T)))
                    return Object.Equals(y, default(T)) ? 0 : -1;
                if (Object.Equals(y, default(T)))
                    return -1;
            }
            if (x.GetType() != y.GetType())
                return -1;
            if (x is IComparable<T>)
                return ((IComparable<T>)x).CompareTo(y);
            return x.CompareTo(y);
        }

        #endregion
    }

}
