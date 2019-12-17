using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.Collections
{
    public class AutoTimeoutDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
    {
        private static TimerPool _timerPool=new TimerPool(3);
        private Dictionary<TKey, TimeoutItem<TKey,TValue>> _cacheValue = null;
        private ReaderWriterLockSlim lockSlim=new ReaderWriterLockSlim();
        //private ICollection<KeyValuePair<TKey, TValue>> _collection = null;
        private int _defaultTimeout = 10000;
        private TimerPool_Item _timerPoolItem = null;

        public AutoTimeoutDictionary(int capacity=16, int defaultTimeout=10000, IEqualityComparer<TKey> comparer=null)
        {
            if (comparer!=null)
            {
                _cacheValue = new Dictionary<TKey, TimeoutItem<TKey,TValue>>(capacity, comparer);
            }
            else
            {
                _cacheValue = new Dictionary<TKey, TimeoutItem<TKey, TValue>>(capacity);
            }

            //_collection = (ICollection<KeyValuePair<TKey, TValue>>) _cacheValue;
            _defaultTimeout = defaultTimeout;
            _timerPoolItem = _timerPool.Add(500, timerCallback, null);
        }

        public void Clear()
        {
            lockSlim.EnterWriteLock();

            _cacheValue.Clear();

            lockSlim.ExitWriteLock();
        }

        public int Count
        {
            get { return _cacheValue.Count; }
        }

        public bool ContainsKey(TKey key)
        {
            var isContain = false;

            lockSlim.EnterReadLock();

            isContain = _cacheValue.ContainsKey(key);

            lockSlim.ExitReadLock();

            return isContain;
        }

        public bool TryGetValue(TKey key, out IAutoTimeoutDictionaryItem<TKey, TValue> value)
        {
            var isContrans = false;

            lockSlim.EnterReadLock();

            TimeoutItem<TKey, TValue> tempItem = null;

            isContrans = _cacheValue.TryGetValue(key, out tempItem);

            lockSlim.ExitReadLock();

            value = tempItem;

            return isContrans;
        }

        public IAutoTimeoutDictionaryItem<TKey, TValue> this[TKey key]
        {
            get
            {
                return _cacheValue[key];
            }
        }

        public IAutoTimeoutDictionaryItem<TKey, TValue> Add(TKey key, TValue value)
        {
            return Add(key,value, _defaultTimeout, null, null);
        }

        public IAutoTimeoutDictionaryItem<TKey, TValue> Add(TKey key, TValue value, int timeout)
        {
            return Add(key, value, timeout, null, null);
        }

        public IAutoTimeoutDictionaryItem<TKey, TValue> Add(TKey key, TValue value, WaitCallback callback, object state)
        {
            return Add(key, value, _defaultTimeout, callback, state);
        }

        public IAutoTimeoutDictionaryItem<TKey, TValue> Add(TKey key,TValue value, int timeout, WaitCallback callback, object state)
        {
            lockSlim.EnterWriteLock();

            var t = new TimeoutItem<TKey, TValue>(key, value, timeout, callback, state);

            _cacheValue.Add(key, t);

            lockSlim.ExitWriteLock();

            return t;
        }

        public bool Remove(TKey key)
        {
            var isSuccess = false;

            lockSlim.EnterWriteLock();

            isSuccess = _cacheValue.Remove(key);

            lockSlim.ExitWriteLock();

            return isSuccess;
        }

        public void Remove(IAutoTimeoutDictionaryItem<TKey, TValue> item)
        {
            lockSlim.EnterWriteLock();

            _cacheValue.Remove(item.Key);

            lockSlim.ExitWriteLock();
        }

        public void Remove(TValue value)
        {
            lockSlim.EnterWriteLock();

            KeyValuePair<TKey, TimeoutItem<TKey, TValue>> tempItem = default(KeyValuePair<TKey, TimeoutItem<TKey, TValue>>);

            var isContains = false;

            foreach (var item in _cacheValue)
            {
                if (item.Value.Value.Equals(value))
                {
                    tempItem = item;
                    isContains = true;
                }
            }

            if (isContains)
            {
                _cacheValue.Remove(tempItem.Key);
            }

            lockSlim.ExitWriteLock();

        }

        public void Close()
        {
            lockSlim.EnterWriteLock();

            _timerPoolItem.Close();
            _cacheValue.Clear();

            lockSlim.ExitWriteLock();
        }

        public int DefaultTimeout
        {
            get { return _defaultTimeout; }
            set
            {
                Interlocked.Exchange(ref _defaultTimeout, value);
            }
        }

        public event EventHandler<AutoTimeoutDictionaryItemEventArgs<TKey, TValue>> Timeout;

        private void timerCallback(object state)
        {
            lockSlim.EnterReadLock();

            foreach (var node in _cacheValue)
            {
                if (node.Value.Decrement())
                {
                    var v = node.Value;
                    v.Execute();
                    Events.EventHelper.RaiseAsync(Timeout, this, () => new AutoTimeoutDictionaryItemEventArgs<TKey, TValue>(v.Key,v.value, v.TimeoutCount));
                }
            }

            lockSlim.ExitReadLock();
        }

        private class TimeoutItem<TKey, TValue> : IAutoTimeoutDictionaryItem<TKey, TValue>
        {
            private int _current;
            public TValue value;
            private int _defaultTime;
            private WaitCallback callback;
            private object _state;
            private int _timeoutCount = 0;
            private TKey _key;

            public TimeoutItem(TKey key,TValue value, int defaultTime, WaitCallback callback, object state)
            {
                _defaultTime = defaultTime;
                this.callback = callback;
                _state = state;
                this.value = value;
                _key = key;
            }

            public void Execute()
            {
                if (callback!=null)
                {
                    ThreadPool.QueueUserWorkItem(callback, _state);
                }
            }

            public int TimeoutCount
            {
                get { return _timeoutCount; }
            }

            public bool Decrement()
            {
                if (Interlocked.Add(ref _current, -500) <= 0)
                {
                    Interlocked.Exchange(ref _current, _defaultTime);
                    Interlocked.Increment(ref _timeoutCount);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Active()
            {
                Interlocked.Exchange(ref _current, _defaultTime);
                Interlocked.Exchange(ref _timeoutCount, 0);
            }

            public TValue Value
            {
                get { return value; }
            }

            #region Implementation of IAutoTimeoutDictionaryItem<TKey,TValue>

            public TKey Key
            {
                get { return _key; }
            }

            #endregion
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lockSlim.EnterReadLock();

            foreach (var timeoutItem in _cacheValue)
            {
                yield return new KeyValuePair<TKey, TValue>(timeoutItem.Key,timeoutItem.Value.Value);
            }

            lockSlim.ExitReadLock();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IAutoTimeoutDictionaryItem<TKey, TValue> : IAutoTimeoutCollectionItem<TValue>
    {
        TKey Key { get; }
    }

    public class AutoTimeoutDictionaryItemEventArgs<TKey,TValue>:AutoTimeoutCollectionItemEventArgs<TValue>
    {
        private TKey _key;

        public AutoTimeoutDictionaryItemEventArgs(TKey key, TValue value, int timeoutCount) : base(value, timeoutCount)
        {
            _key = key;
        }

        public TKey Key
        {
            get { return _key; }
        }
    }
}
