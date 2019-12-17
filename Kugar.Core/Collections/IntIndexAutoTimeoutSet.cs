using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.Collections
{
    public class IntIndexAutoTimeoutSet<TValue> : IAutoTimeoutSet<int, TValue>
    {
        //private HashSet<int> _cacheCollection = null;
        private static TimerPool _timerPool=new TimerPool(3);
        private int currentValue = 0;
        private bool _isDisposed = false;
        private int _maxTestCount = int.MaxValue - 10;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private Dictionary<int, TimeoutItem<int,TValue>> _cacheCollection = null;
        private TimerPool_Item timerPoolItem = null;
        private int _timeOut;

        public IntIndexAutoTimeoutSet(int timeout)
        {
            if (timeout < 1000)
            {
                throw new ArgumentOutOfRangeException("timeout", "timeout参数取值不能小于1000ms");
            }
            _timeOut = timeout;
            _cacheCollection = new Dictionary<int, TimeoutItem<int,TValue>>();
            timerPoolItem = _timerPool.Add(1000, timerCallback, null, true);
        }

        #region Implementation of IAutoTimeoutSet<int,TValue>

        public int TaskItem(TValue value)
        {
            int getCount = 0;

            readerWriterLock.EnterWriteLock();

            int tempValue = currentValue;

            do
            {
                //unchecked
                //{
                Interlocked.Increment(ref tempValue);
                //tempValue++;
                //}

                if (!_cacheCollection.ContainsKey(tempValue))
                {
                    _cacheCollection.Add(tempValue, new TimeoutItem<int,TValue>(tempValue, DateTime.Now, value));
                    Interlocked.Exchange(ref currentValue, tempValue);
                    break;
                }
                else
                {
                    getCount++;

                    if (getCount > _maxTestCount)
                    {
                        readerWriterLock.ExitWriteLock();
                        throw new OverflowException("超过测试次数，请将不需要使用的ID进行回收删除");
                    }
                }

            } while (true);

            readerWriterLock.ExitWriteLock();
            return tempValue;
        }

        public bool ActiveItem(int key)
        {
            return ActiveItem(key, true);
        }

        public bool ActiveItem(int key, bool isCleanTimeoutCount)
        {
            readerWriterLock.EnterWriteLock();

            TimeoutItem<int, TValue> item = null;
            bool isSuccess = false;

            if (_cacheCollection.TryGetValue(key, out item))
            {
                item.LastActiveTime = DateTime.Now;

                if (isCleanTimeoutCount)
                {
                    Interlocked.Exchange(ref item.TimeoutCount, 0);
                }

                isSuccess = true;
            }

            readerWriterLock.ExitWriteLock();

            return isSuccess;
        }

        public TValue this[int key]
        {
            get
            {
                TimeoutItem<int, TValue> value;

                if (_cacheCollection.TryGetValue(key,out value))
                {
                    return value.Value;
                }
                else
                {
                    return default(TValue);
                }

                //return _cacheCollection
            }
            set
            {
                TimeoutItem<int, TValue> tempvalue;

                if (_cacheCollection.TryGetValue(key, out tempvalue))
                {
                    tempvalue.Value = value;
                }
            }
        }

        public DateTime GetItemLastActiveTime(int key)
        {
            readerWriterLock.EnterReadLock();

            TimeoutItem<int,TValue> item = null;

            if (_cacheCollection.TryGetValue(key, out item))
            {
                readerWriterLock.ExitReadLock();
                return item.LastActiveTime;

            }
            else
            {
                readerWriterLock.ExitReadLock();
                return DateTime.MinValue;
            }
        }

        public int GetItemTimeoutCount(int key)
        {
            readerWriterLock.EnterReadLock();

            TimeoutItem<int, TValue> item = null;

            if (_cacheCollection.TryGetValue(key, out item))
            {
                readerWriterLock.ExitReadLock();
                return item.TimeoutCount;

            }
            else
            {
                readerWriterLock.ExitReadLock();
                return int.MaxValue;
            }
        }

        public int Timeout
        {
            get { return timerPoolItem.TimerInterval; }
            set { timerPoolItem.TimerInterval = value; }
        }

        public void Clear()
        {
            readerWriterLock.EnterWriteLock();

            _cacheCollection.Clear();

            currentValue = 0;

            readerWriterLock.ExitWriteLock();
        }

        public bool ContainsKey(int key)
        {
            return _cacheCollection.ContainsKey(key);
        }

        public event EventHandler<IntIndexAutoTimeoutSetTimeoutEventArgs<int,TValue>> ItemTimeout;

        public bool ContainsID(int key)
        {
            readerWriterLock.EnterReadLock();
            var b = _cacheCollection.ContainsKey(key);

            readerWriterLock.ExitReadLock();
            return b;
        }

        public void ReturnValue(int value)
        {
            readerWriterLock.EnterWriteLock();
            _cacheCollection.Remove(value);
            readerWriterLock.ExitWriteLock();
        }

        public bool AddValue(int key, TValue value)
        {
            readerWriterLock.EnterWriteLock();

            var ret = false;
            if (_cacheCollection.ContainsKey(key))
            {
                ret = false;
            }
            else
            {
                _cacheCollection.Add(key, new TimeoutItem<int, TValue>(key, DateTime.Now, value));
                ret = true;

            }

            
            readerWriterLock.ExitWriteLock();

            return ret;
        }

        public void AddOrUpdate(int key,TValue value)
        {
            readerWriterLock.EnterWriteLock();

            if (!_cacheCollection.ContainsKey(key))
            {
                _cacheCollection.Add(key, new TimeoutItem<int, TValue>(key, DateTime.Now, value));
            }
            else
            {
                TimeoutItem<int, TValue> n;

                if (_cacheCollection.TryGetValue(key,out n))
                {
                    n.Value = value;
                }
            }

            readerWriterLock.ExitWriteLock();
        }


        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _cacheCollection.Clear();
            TimerPool.Default.Remove(timerPoolItem);

        }

        #endregion



        private void timerCallback(object state)
        {
            if (_cacheCollection.Count <= 0)
            {
                return;
            }

            timerPoolItem.Stop();
            var n = DateTime.Now.AddMilliseconds(-1 * _timeOut);

            readerWriterLock.EnterReadLock();

            foreach (var itemTimeout in _cacheCollection)
            {
                TimeoutItem<int, TValue> v=itemTimeout.Value;

                if (v.LastActiveTime < n)
                {
                    Interlocked.Increment(ref v.TimeoutCount);
                    Events.EventHelper.RaiseAsync(ItemTimeout, this,
                                                  new IntIndexAutoTimeoutSetTimeoutEventArgs<int,TValue>(itemTimeout.Key,
                                                                                                 v.TimeoutCount,
                                                                                                 v.LastActiveTime,
                                                                                                 v.Value));
                    v.LastActiveTime = DateTime.Now;
                }
            }

            readerWriterLock.ExitReadLock();

            timerPoolItem.Start();
        }

        private class TimeoutItem<TKey, TValue>
        {
            public TimeoutItem(TKey key, DateTime lastActiveTime, TValue state)
            {
                LastActiveTime = lastActiveTime;
                TimeoutCount = 0;
                Value = state;
                Key = key;
            }

            public DateTime LastActiveTime;
            public int TimeoutCount;
            public TValue Value;
            public TKey Key;
        }

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            readerWriterLock.EnterReadLock();

            if (_cacheCollection.Count > 0)
            {
                foreach (var value in _cacheCollection)
                {
                    yield return new KeyValuePair<int, TValue>(value.Key,value.Value.Value);
                }
            }
            else
            {
                yield break;
            }


            readerWriterLock.ExitReadLock();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IAutoTimeoutSet<TKey, TValue> : IDisposable,IEnumerable<KeyValuePair<TKey,TValue>>
    {
        TValue this[TKey key] { get; set; }
        TKey TaskItem(TValue state);
        bool ActiveItem(TKey key);
        bool ActiveItem(TKey key, bool isCleanTimeoutCount);
        DateTime GetItemLastActiveTime(TKey key);
        int Timeout { get; set; }
        bool ContainsID(TKey id);
        int GetItemTimeoutCount(TKey key);
        void ReturnValue(TKey key);
        bool AddValue(TKey key, TValue value);
        void AddOrUpdate(int key, TValue value);
        void Clear();
        bool ContainsKey(TKey key);

        event EventHandler<IntIndexAutoTimeoutSetTimeoutEventArgs<TKey, TValue>> ItemTimeout;
    }



    public class IntIndexAutoTimeoutSetTimeoutEventArgs<TKey, TValue> : EventArgs
    {
        public IntIndexAutoTimeoutSetTimeoutEventArgs(TKey itemkey, int timeoutCount, DateTime lastActiveTime, TValue value)
        {
            TimeoutCount = timeoutCount;
            LastActiveTime = lastActiveTime;
            ItemKey = itemkey;
            Value = value;
        }

        public TKey ItemKey { get; private set; }
        public int TimeoutCount { get; private set; }
        public DateTime LastActiveTime { get; private set; }
        public TValue Value { get; private set; }
    }
}