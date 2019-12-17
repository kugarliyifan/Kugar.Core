using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{
    public class AutoResetEventPool:IDisposable
    {
        private static Lazy<AutoResetEventPool> defaultPool = new Lazy<AutoResetEventPool>(OnDefaultValueCreate);
        private Dictionary<int, AutoResetEventItem> cacheItemPool = null;
        private Stack<AutoResetEventItem> freeItemList = null;
        private object lockerObj=new object();
        private TimerEx timer = null;
        private bool _isDisposed = false;

        public AutoResetEventPool():this(50){}

        public AutoResetEventPool(int initPoolCount)
        {
            if (initPoolCount<0)
            {
                throw new ArgumentOutOfRangeException("initPoolCount");
            }

            cacheItemPool=new Dictionary<int, AutoResetEventItem>();
            freeItemList = new Stack<AutoResetEventItem>();

            for (int i = 0; i < initPoolCount; i++)
            {
                var tempItem = new AutoResetEventItem();
                tempItem.LastUsedTime = DateTime.Now;

                freeItemList.Push(tempItem);
            }

            timer = new TimerEx(OnTimerCallback, 60000, null);

            timer.Start();
        }

        public AutoResetEventItem GetLockItem()
        {
            AutoResetEventItem autoItem = null;

            try
            {
                lock (lockerObj)
                {
                    autoItem = freeItemList.Pop();

                    //cacheItemPool.Add(autoItem.ID, autoItem);
                }
            }
            catch (Exception)
            {
                autoItem = new AutoResetEventItem();
                autoItem.LockStart += autoItem_LockStart;
                autoItem.UnLock += autoItem_UnLock;
            }

            autoItem.LastUsedTime = DateTime.Now;

            return autoItem;
        }

        void autoItem_UnLock(object sender, EventArgs e)
        {
            var au = (AutoResetEventItem) sender;

            lock (lockerObj)
            {
                cacheItemPool.Remove(au.ID);

                au.State = null;

                freeItemList.Push(au);

            }
        }

        void autoItem_LockStart(object sender, EventArgs e)
        {
            var au = (AutoResetEventItem) sender;

            lock (lockerObj)
            {
                cacheItemPool.Remove(au.ID);
                cacheItemPool.Add(au.ID, au);
            }
        }

        public bool Set(int autoResetEventID)
        {
            var item = cacheItemPool.TryGetValue(autoResetEventID,null);

            if (item==null)
            {
                throw new ArgumentOutOfRangeException("autoResetEventID");
            }

            return item.Set();
        }

        public static AutoResetEventPool Default
        {
            get { return defaultPool.Value; }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                lock (lockerObj)
                {
                    if (!_isDisposed)
                    {
                        _isDisposed = true;

                        timer.Stop();
                        timer.Dispose();

                        foreach (var item in cacheItemPool)
                        {
                            item.Value.Dispose();
                        }

                        foreach (var item in freeItemList)
                        {
                            item.Dispose();
                        }

                        cacheItemPool.Clear();
                        freeItemList.Clear();


                    }
                }
            }
        }

        #endregion

        private static AutoResetEventPool OnDefaultValueCreate()
        {
            return new AutoResetEventPool(50);
        }

        private void OnTimerCallback(object state)
        {
            lock (lockerObj)
            {
                var ar = freeItemList.ToArray();
                var lst = new List<AutoResetEventItem>(ar);

                var limitTime = DateTime.Now.AddMinutes(8);

                foreach (var item in ar)
                {
                    if (item.LastUsedTime>= limitTime)
                    {
                        lst.Remove(item);
                        item.Dispose();
                    }
                }

                if (lst.Count!=ar.Length)
                {
                    freeItemList.Clear();

                    foreach (var eventItem in lst)
                    {
                        freeItemList.Push(eventItem);
                    }
                }

                lst.Clear();

            }
        }

        public class AutoResetEventItem:IDisposable
        {
            private AutoResetEvent _locker;

            internal AutoResetEventItem()
            {
                ID = RandomEx.Next();
                _locker = new AutoResetEvent(false);
            }

            public int ID {private set; get; }
            //private AutoResetEvent Locker { private set; get; }
            public DateTime LastUsedTime { set; get; }

            public bool WaitOne(int timeOut)
            {
                return WaitOne(timeOut, false);
            }

            public bool WaitOne(int timeOut,bool exitContext)
            {
                Events.EventHelper.Raise(LockStart, this);

                return _locker.WaitOne(timeOut, exitContext);
            }

            public bool Set()
            {
                var t = _locker.Set();

                Events.EventHelper.Raise(UnLock,this);

                return t;

            }

            public object State { set; get; }

            public event EventHandler LockStart;
            public event EventHandler UnLock;

            //public bool IsUsed { set; get; }

            #region Implementation of IDisposable

            public void Dispose()
            {
                Delegate.RemoveAll(LockStart, LockStart);
                Delegate.RemoveAll(UnLock, UnLock);

                _locker.Set();
                _locker = null;


            }

            #endregion
        }

        
    }
}
