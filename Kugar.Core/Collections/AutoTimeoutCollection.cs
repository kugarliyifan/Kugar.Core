using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.BaseStruct;
using System.Threading;

namespace Kugar.Core.Collections
{
    public class AutoTimeoutCollection<T>:IEnumerable<T>
    {
        private static TimerPool _timerPool=new TimerPool(2);
        private LinkedListEx<TimeoutItem<T>> _link = new LinkedListEx<TimeoutItem<T>>();
        private ReaderWriterLockSlim lockSlim=new ReaderWriterLockSlim();

        private int _defaultTimeout = 10000;

        private TimerPool_Item timerPoolItem = null;

        public AutoTimeoutCollection()
        {
            timerPoolItem = _timerPool.Add(500, timerCallback, null, true);
        }

        public IAutoTimeoutCollectionItem<T> Add(T value)
        {
            return Add(value, _defaultTimeout, null, null);
        }

        public IAutoTimeoutCollectionItem<T> Add(T value, int timeout)
        {
            return Add(value, timeout, null, null);
        }

        public IAutoTimeoutCollectionItem<T> Add(T value, WaitCallback callback, object state)
        {
            return Add(value, _defaultTimeout, callback, state);
        }

        public IAutoTimeoutCollectionItem<T> Add(T value, int timeout, WaitCallback callback, object state)
        {
            var t = new TimeoutItem<T>(value, timeout, callback, state);

            _link.AddLast(t);

            return t;
        }

        public void Remove(IAutoTimeoutCollectionItem<T> item)
        {
            _link.Remove((TimeoutItem<T>)item);
        }

        public void Remove(T value)
        {
            LinkNodeEx<TimeoutItem<T>> item = null;

            foreach (var t in _link)
            {
                if(t.Value.Value.Equals(value))
                {
                    item = t;
                }
            }

            if (item!=null)
            {
                _link.Remove(item);
            }
        }

        public void Clear()
        {
            _link.Clear();
        }

        public void Close()
        {
            _link.Clear();
            timerPoolItem.Close();
        }

        public int DefaultTimeout
        {
            get { return _defaultTimeout; }
            set
            {
                Interlocked.Exchange(ref _defaultTimeout, value);
            }
        }

        public event EventHandler<AutoTimeoutCollectionItemEventArgs<T>> Timeout;

        private void timerCallback(object state)
        {
            foreach (var node in _link)
            {
                if(node.Value.Decrement())
                {
                    var v = node.Value;
                    v.Execute();
                    Events.EventHelper.RaiseAsync(Timeout,this,()=> new AutoTimeoutCollectionItemEventArgs<T>(v.value,v.TimeoutCount));
                }
            }
        }

        private class TimeoutItem<T> : IAutoTimeoutCollectionItem<T>
        {
            private int _current;
            public T value;
            private int _defaultTime;
            private WaitCallback callback;
            private object _state;
            private int _timeoutCount = 0;

            public TimeoutItem(T value,int defaultTime, WaitCallback callback, object state )
            {
                _defaultTime = defaultTime;
                this.callback = callback;
                _state = state;
                this.value = value;
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

            public T Value
            {
                get { return value; }
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var node in _link)
            {
                yield return node.Value.value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class AutoTimeoutCollectionItemEventArgs<T>:EventArgs
    {
        private T _value;
        private int _timeoutCount;

        public AutoTimeoutCollectionItemEventArgs(T value, int timeoutCount)
        {
            _value = value;
            _timeoutCount = timeoutCount;
        }

        public T Value
        {
            get { return _value; }
        }

        public int TimeoutCount
        {
            get { return _timeoutCount; }
        }
    }

    public interface IAutoTimeoutCollectionItem<T>
    {
        void Active();
        T Value { get; }
    }
}
