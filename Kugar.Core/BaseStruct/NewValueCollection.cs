using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;

namespace Kugar.Core.BaseStruct
{
    

    /// <summary>
    ///     用于返回一个不重复的值的类，，<br/>
    ///     该类提供了将获取的值进行存储，每次获取值的时候，都会对本身缓存进行对比，防止出现重复<br/>
    ///     在ID使用完之后，必须调用ReturnValue函数进行回收
    /// </summary>
    public static class NewIDCollection
    {
        public static INewIDCollection<short> CreateInt16IDCollection()
        {
            return new Int16NewValueCollection();
        }

        public static INewIDCollection<int> CreateInt32IDCollection()
        {
            return new Int32NewValueCollection();
        }
    }

    public interface INewIDCollection<T> : IDisposable
    {
        T TakeValue();

        void ReturnValue(T value);

        bool ContainsID(T id);
    }

    internal  class Int16NewValueCollection : INewIDCollection<short>
    {
        private HashSet<short> _cacheCollection = null;
        private volatile short currentValue = 0;
        private bool _isDisposed = false;
        private object _lockerObj=new object();
        private short _maxTestCount = short.MaxValue/2;

        internal Int16NewValueCollection()
        {
            _cacheCollection = new HashSet<short>();
        }

        public short TakeValue()
        {
            short getCount = 0;

            Monitor.Enter(_lockerObj);

            short tempValue=currentValue;
            do
            {
                unchecked
                {
                    tempValue++;
                }

                if (_cacheCollection.Add(tempValue))
                {
                    currentValue = tempValue;
                    break;
                }
                else
                {
                    getCount++;

                    if (getCount > _maxTestCount)
                    {
                        Monitor.Exit(_lockerObj);
                        throw new OverflowException("超过测试次数，请将不需要使用的ID进行回收删除");
                    }
                }

            } while (true);

            Monitor.Exit(_lockerObj);
            return tempValue;
        }

        public void ReturnValue(short value)
        {
            _cacheCollection.Remove(value);
        }

        public bool ContainsID(short id)
        {
            return _cacheCollection.Contains(id);
        }



        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _cacheCollection.Clear();
            _cacheCollection = null;
        }

        #endregion
    }

    internal class Int32NewValueCollection : INewIDCollection<int>
    {
        private HashSet<int> _cacheCollection=null;
        private int currentValue = 0;
        private bool _isDisposed = false;
        private int _maxTestCount = int.MaxValue / 2;

        internal Int32NewValueCollection()
        {
            _cacheCollection=new HashSet<int>();
        }

        public int TakeValue()
        {
            var getCount = 0;
            int tempValue;
            do
            {
                tempValue = Interlocked.Increment(ref currentValue);

                if (_cacheCollection.Add(tempValue))
                {
                    break;
                }
                else
                {
                    getCount++;

                    if (getCount > _maxTestCount)
                    {
                        throw new OverflowException("超过测试次数，请将不需要使用的ID进行回收删除");
                    }
                }
            } while (true);

            return tempValue;
        }

        public void ReturnValue(int value)
        {
            _cacheCollection.Remove(value);
        }

        public bool ContainsID(int id)
        {
            return _cacheCollection.Contains(id);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _cacheCollection.Clear();
            _cacheCollection = null;
        }

        #endregion
    }
}
