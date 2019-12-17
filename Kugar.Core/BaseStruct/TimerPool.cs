using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.Compare;
using Kugar.Core.Compare;

namespace Kugar.Core.BaseStruct
{
    public class TimerPool : MarshalByRefObject, IDisposable
    {
        private List<TimerEx> _checkTimer = null;

        private List<List<TimerPool_Item>> _delgateList = null;
        private bool _isDisposed = false;
        private object lockerObj = new object();

        private static Lazy<TimerPool> _defaultTimerPool = new Lazy<TimerPool>(OnLazyInit);

        public TimerPool()
            : this(1)
        {

        }

        public TimerPool(int timerCount)
        {
            //delgateList = new List<TimerPool_Item>(50);
            //_checkTimer = new TimerEx(CheckerList, 200, null);

            _delgateList = new List<List<TimerPool_Item>>(timerCount);
            _checkTimer = new List<TimerEx>(timerCount);


            var newList = new List<TimerPool_Item>(1000);
            var newtimer = new TimerEx(CheckerList, 200, newList);

            _delgateList.Add(newList);
            _checkTimer.Add(newtimer);

            newtimer.Start();
        }

        public TimerPool_Item Add(int timerInterval, TimerCallback callback, object state=null, bool isStart=true)
        {
            return Add(timerInterval, (s, e) => callback(e), state, isStart);
        }

        public TimerPool_Item Add(int timerInterval, TimerPoolItemCallback callback, object state=null, bool isStart=true)
        {
            var item = new TimerPool_Item(this, callback, state, timerInterval);

            lock (lockerObj)
            {
                var index = -1;

                //查找第一个小于10000个托管定时任务的定时列表
                if (_delgateList.Count > 0)
                {
                    for (int i = 0; i < _delgateList.Count; i++)
                    {
                        if (_delgateList[i].Count < 10000)
                        {
                            index = i;
                            break;
                        }
                    }
                }


                //如果都大于或等于10000的话，则添加一个新的
                if (index == -1)
                {
                    var newList = new List<TimerPool_Item>(1000);

                    _delgateList.Add(newList);

                    index = _delgateList.Count - 1;

                    var newtimer = new TimerEx(CheckerList, 2000, newList);

                    _checkTimer.Add(newtimer);

                    if (isStart)
                    {
                        newtimer.Start();
                    }
                }

                _delgateList[index].Add(item);

                if (_delgateList.Count>3)
                {
                    _delgateList.Sort((x, y) => x.Count.CompareTo(y.Count));
                }
            }

            return item;
        }

        public void Remove(TimerPool_Item timerPoolItem)
        {
            lock (lockerObj)
            {
                List<int> tempLst = null;

                var _delgateListCount = _delgateList.Count;

                for (int i = 0; i < _delgateListCount; i++)
                {
                    var lst = _delgateList[i];

                    lock (lst)
                    {
                        lst.Remove(timerPoolItem);

                        if (lst.Count <= 0 && _delgateListCount > 1)
                        {
                            if (tempLst == null)
                            {
                                tempLst = new List<int>(_delgateList.Count / 2);
                            }

                            tempLst.Add(i);
                        }
                    }
                }

                if (tempLst != null && tempLst.Count > 0)
                {
                    foreach (var index in tempLst)
                    {
                        _delgateList.RemoveAt(index);

                        var timer = _checkTimer[index];

                        timer.Dispose();

                        _checkTimer.RemoveAt(index);
                    }
                }
            }
        }

        public void Resume()
        {
            foreach (var timerEx in _checkTimer)
            {
                timerEx.Start();
            }
        }

        public void Stop()
        {
            foreach (var timerEx in _checkTimer)
            {
                timerEx.Stop();
            }
        }

        private void CheckerList(object state)
        {
            var t = (List<TimerPool_Item>)state;

            if (t.Count <= 0)
            {
                return;
            }

            var lst = t.ToArray();

            foreach (var poolItem in lst)
            {
                if (poolItem == null) continue;
                if (!poolItem.IsStop)
                {
                    Interlocked.Add(ref poolItem.CurrentTimeRemaining, -200);

                    if (poolItem.CurrentTimeRemaining <= 0)
                    {
                        poolItem.ResetTime();

                        ThreadPool.QueueUserWorkItem(ExecuteItemFunc, poolItem);
                    }
                }
            }
        }

        private void ExecuteItemFunc(object state)
        {
            var item = (TimerPool_Item)state;

            if (item != null)
            {
                try
                {
                    item.Execute();
                }
                catch (Exception)
                {
                }
            }

        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            lock (lockerObj)
            {
                foreach (var timerEx in _checkTimer)
                {
                    timerEx.Dispose();
                }

                _checkTimer.Clear();

                foreach (var lst in _delgateList)
                {
                    foreach (var item in lst)
                    {
                        item.Dispose();
                    }

                    lst.Clear();
                }

                _delgateList.Clear();
                _delgateList = null;

                _isDisposed = true;

            }
        }

        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        public static TimerPool Default
        {
            get { return _defaultTimerPool.Value; }
        }

        private static TimerPool OnLazyInit()
        {
            return new TimerPool();
        }
    }

    public delegate void TimerPoolItemCallback(TimerPool_Item timer, object state);

    public class TimerPool_Item : MarshalByRefObject, IDisposable
    {
        private TimerPool _pool = null;
        private TimerPoolItemCallback _callback = null;
        private object _state = null;
        private int _timerInterval = 0;
        //private int _currentTimeRemaining = 0;
        private bool _isDisposed = false;
        private bool _isStop = false;

        internal TimerPool_Item(TimerPool pool, TimerPoolItemCallback callback, object state, int timerInterval)
        {
            _pool = pool;
            _callback = callback;
            _state = state;
            _timerInterval = timerInterval;
            CurrentTimeRemaining = timerInterval;
        }



        public object State
        {
            get { return _state; }
        }

        public bool IsStop { get { return _isStop; } }

        public void Stop()
        {
            _isStop = true;
        }

        public void Start()
        {
            if (this._isDisposed)
            {
                return;
            }
            else
            {
                _isStop = false;
                ResetTime();
            }

        }

        public void Close()
        {
            if (_pool == null || (_pool.IsDisposed || this._isDisposed))
            {
                return;
            }

            _isStop = true;

            _pool.Remove(this);

            _callback = null;
            _pool = null;
            _timerInterval = 0;



            _isDisposed = true;
        }

        public int TimerInterval
        {
            get
            {
                return _timerInterval;
            }
            set { Interlocked.Exchange(ref _timerInterval, value); }
        }

        internal TimerPoolItemCallback Callback { get { return _callback; } }

        internal void Execute()
        {
            if (_callback != null)
            {
                _callback(this,_state);
            }
        }

        public void ResetTime()
        {
            Interlocked.Exchange(ref CurrentTimeRemaining, _timerInterval);
        }

        internal int CurrentTimeRemaining;// { get; set; }

        #region Implementation of IDisposable

        public void Dispose()
        {
            this.Close();
        }

        #endregion
    }
}
