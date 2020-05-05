using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
#if NET4
using System.Threading.Tasks;
#endif



namespace Kugar.Core.BaseStruct
{
    /// <summary>
    ///     提供一个可复用的定时器功能，用于在自定义的时间后，回调指定的函数<br/>
    ///     注：提供回调的时间并不是特别精准，最小分辨为500ms，可适用于经常需要在指定时间后回调指定函数的环境，
    ///            但由对定时的时间精准度要求不高，用CallbackTimer可节省许多timer的定义以及管理
    ///     
    /// </summary>
    public class CallbackTimer : MarshalByRefObject, IDisposable
    {
        //private CallbackBlockPool _pool = new CallbackBlockPool(500, 10);

        #if NET2
            private static CallbackTimer defaultTimer = new CallbackTimer(1024, 2);
        #else
        private static Lazy<CallbackTimer> defaultTimer = new Lazy<CallbackTimer>(OnLazyInit);
        #endif

        private bool _isDisposed = false;
        private int _currentListIndex = -1; //当前最后一次分配回调函数的callBackList的索引;用于将要调用的回调平均的分配到所有列表

        private InternalCallbackTimer[] _internalTimers=null;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="initCount">初始化列表大小</param>
        /// <param name="timerCounts">定时器个数</param>
        public CallbackTimer(int initCount, int timerCounts)
        {
            if (initCount < 0)
            {
                throw new ArgumentOutOfRangeException("initCount");
            }

            if (timerCounts <= 0)
            {
                timerCounts = 1;
            }

            _internalTimers=new InternalCallbackTimer[timerCounts];

            for (int i = 0; i < _internalTimers.Length; i++)
            {
                _internalTimers[i]=new InternalCallbackTimer(Task.Factory);
            }
            
        }

        /// <summary>
        ///     回调指定的函数
        /// </summary>
        /// <param name="spanInSecs">指定时间后调用callback参数指定的函数</param>
        /// <param name="callback">调用的函数</param>
        public ICallbackBlock Call(int spanInMilliSecs, WaitCallback callback)
        {
            return Call(spanInMilliSecs, callback, null);
        }

        /// <summary>
        ///     回调指定的函数
        /// </summary>
        /// <param name="spanInSecs">指定时间后调用callback参数指定的函数</param>
        /// <param name="callback">调用的函数</param>
        /// <param name="state">传递的参数</param>
        public ICallbackBlock Call(int spanInMilliSecs, WaitCallback callback, object state)
        {

            var _tempIndex = Math.Abs(Interlocked.Increment(ref _currentListIndex) % _internalTimers.Length);

            return _internalTimers[_tempIndex].InsertItem(spanInMilliSecs, callback, state);

            //List<CallbackBlock> callbackBlocks = callBackList[_tempIndex];


            ////            for (int i = 0; i < callBackList.Length; i++)
            ////            {
            ////                if (callBackList[i].Count < callbackBlocks.Count)
            ////                {
            ////                    callbackBlocks = callBackList[i];
            ////                }
            ////            }

            //CallbackBlock cb = null;

            //lock (callBackList)
            //{
            //    //cb = new CallbackBlock(callback, state, spanInSecs);

            //    cb = _pool.Take();

            //    cb.PreCall(callback, state, spanInSecs);

            //    callbackBlocks.Add(cb);
            //}

            //return cb;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            lock (this)
            {
                foreach (var timer in _internalTimers)
                {
                    timer.Close();
                }

                //_pool.Dispose();

                _isDisposed = true;

            }
        }

        #endregion

        /// <summary>
        ///     获取一个全局公用的CallbackTimer
        /// </summary>
        public static CallbackTimer Default
        {
            get
            {
                //#if NET2
                return defaultTimer.Value;
                //#else
                //                return defaultTimer.Value;
                //#endif
            }
        }

        //        private void OnCheckTimer(object state)
//        {
//            if (state == null)
//            {
//                return;
//            }

//            CallbackBlock[] itemList = null;

//            var timerItem = (timerItem)state;

//            lock (timerItem.lst)
//            {

//                itemList = timerItem.lst.ToArray();
//            }

//            var tempList = new List<CallbackBlock>(itemList.Length / 2);

//#if NET4
//            Parallel.ForEach(itemList, (s) =>
//                                           {
//                                               if (s.IsStop)
//                                               {
//                                                   tempList.Add(s);
//                                                   //timerItem.lst.Remove(s);
//                                               }
//                                               else
//                                               {
//                                                   s.LeftTime -= 300;

//                                                   if (s.LeftTime <= 0)
//                                                   {
//                                                       tempList.Add(s);

//                                                       s.CallbackAction(s.Param);

//                                                       s.Dispose();

//                                                   }
//                                               }


//                                           });
//#else
//            for (int i = 0; i < itemList.Length; i++)
//            {
//                var item = itemList[i];

//                if (item.IsStop)
//                {
//                    timerItem.lst.Remove(item);
//                    continue;
//                }

//                item.LeftTime -= 100;

//                if (item.LeftTime<=0)
//                {
//                    tempList.Add(item);

//                    //CallBackAction(item);

//                     ThreadPool.QueueUserWorkItem(CallAction, block);
//                }
//            }

//#endif

//            if (tempList.Count > 0)
//            {
//                lock (timerItem.lst)
//                {
//                    for (int i = 0; i < tempList.Count; i++)
//                    {
//                        timerItem.lst.Remove(tempList[i]);
//                    }
//                }
//            }

//            timerItem.timer.Change(300, 0);
//        }

//        private void CallAction(object state)
//        {
//            var t = (CallbackBlock)state;

//            t.CallbackAction(t.Param);

//            t.Dispose();
//        }

        private static CallbackTimer OnLazyInit()
        {
            return new CallbackTimer(1024, 2);
        }



        //internal class CallbackBlockPool : RecyclablePool<CallbackBlock>
        //{
        //    internal CallbackBlockPool(int maxLength, int minLength) : base(maxLength, minLength)
        //    {
        //        base.Init();
        //    }


        //    #region Overrides of RecyclablePool<CallbackBlock>

        //    /// <summary>
        //    ///     构建一个新的对象,该函数必须由继承的类实现
        //    /// </summary>
        //    /// <returns></returns>
        //    protected override CallbackBlock CreateRecyclableObject()
        //    {
        //        return new CallbackBlock(this);
        //    }

        //    #endregion
            
        //}

        //private class timerItem
        //{
        //    public Timer timer;
        //    public List<CallbackBlock> lst;
        //}

        internal struct CallbackBlock : ICallbackBlock  //, IRecyclable
        {
            internal bool _isDisposed;
            //private CallbackBlockPool _pool = null;
            

            internal void PreCall(WaitCallback callbackAction, object param, int leftTime)
            {
                CallbackAction = callbackAction;
                Param = param;
                _leftTime = leftTime;
                _orgTime = leftTime;
            }

            internal WaitCallback CallbackAction;
            internal object Param;
            internal int _leftTime;
            private int _orgTime;

            internal bool IsStop;

            public bool DecreaseTime(int time)
            {
                return Interlocked.Add(ref _leftTime,-1*time)<=0;
            }

            public void Reset()
            {
                Interlocked.Exchange(ref _leftTime, _orgTime);
            }
            
            public void Stop()
            {
                //lock (this)
                {
                    Interlocked.Exchange(ref _leftTime, int.MaxValue);
                    //_leftTime = int.MaxValue;
                    IsStop = true;
                }
            }


            #region Implementation of IDisposable

            public void Dispose()
            {
                CallbackAction = null;
                Param = null;
                //_leftTime = int.MaxValue;
                Stop();

                //_pool.RecycleObject(this);
            }

            #endregion

            //#region Implementation of IRecyclable

            //public IRecyclablePool<IRecyclable> Pool
            //{
            //    get { return _pool; }
            //    set { }
            //}

            ///// <summary>
            /////     当对象真正释放的时候，回收器会调用该函数
            ///// </summary>
            //public void DisposeObject()
            //{
            //    if (_isDisposed)
            //    {
            //        return;
            //    }

            //    CallbackAction = null;
            //    Param = null;
            //    _leftTime = int.MaxValue;

            //    _isDisposed = true;

            //    GC.SuppressFinalize(this);
            //}

            //#endregion
        }

        internal class InternalCallbackTimer
        {
            //private CallbackBlockPool _pool = null;
            private List<CallbackBlock> _callBackLst = new List<CallbackBlock>();
            private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
            private TimerEx _timer = null;
            private List<CallbackBlock> _removableLst = new List<CallbackBlock>();
            private List<CallbackBlock> _invokeableList = new List<CallbackBlock>();
            private TaskFactory _factory = null;

            public InternalCallbackTimer(/*CallbackBlockPool pool,*/TaskFactory factory)
            {
                _factory = factory;
                //_pool = pool;
                _timer = new TimerEx(onTmer, 450, null);
                _timer.IsStopWhenRun = true;
                _timer.Start();
                
            }

            public ICallbackBlock InsertItem(int spanInSecs, WaitCallback callback, object state)
            {
                _lock.EnterWriteLock();

                try
                {
                    var cb = new CallbackBlock();

                    cb.PreCall(callback, state, spanInSecs);

                    _callBackLst.Add(cb);

                    return cb;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }


            }

            public void Close()
            {
                _lock.EnterWriteLock();

                try
                {
                    _timer.Stop();
                    _timer.Dispose();

                    foreach (var block in _callBackLst)
                    {
                        block.Stop();
                        block.Dispose();
                    }

                    _callBackLst.Clear();
                    _removableLst.Clear();
                    _invokeableList.Clear();

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _lock.ExitWriteLock();

                    _lock.Dispose();
                }
            }

            private void onTmer(object state)
            {
                _lock.EnterUpgradeableReadLock();

                try
                {
                    if (_callBackLst.Count > 0)
                    {

                        if (_callBackLst.Count < 400)
                        {
                            foreach (var block in _callBackLst)
                            {
                                if (block.IsStop)
                                {
                                    _removableLst.Add(block);
                                }
                                else
                                {
                                    //block.LeftTime -= 500;

                                    if (block.DecreaseTime(500))
                                    {
                                        _invokeableList.Add(block);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Parallel.ForEach(_callBackLst, (block) =>
                            {

                                if (block.IsStop)
                                {
                                    _removableLst.Add(block);
                                }
                                else
                                {
                                    if (block.DecreaseTime(500))
                                    {
                                        _invokeableList.Add(block);
                                    }
                                }
                            });
                        }


                        if (_removableLst.Count > 0 || _invokeableList.Count > 0)
                        {
                            _lock.EnterWriteLock();

                            try
                            {
                                if (_removableLst.Count > 0)
                                {
                                    foreach (var block in _removableLst)
                                    {
                                        _callBackLst.Remove(block);

                                        block.Dispose();
                                    }
                                }

                                if (_invokeableList.Count > 0)
                                {
                                    foreach (var block in _invokeableList)
                                    {
                                        _callBackLst.Remove(block);
                                        
                                        _factory.StartNew(onInvokeCallBlock,block);
                                    }
                                }
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            finally
                            {
                                _removableLst.Clear();
                                _invokeableList.Clear();

                                _lock.ExitWriteLock();

                            }
                        }

                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            private void onInvokeCallBlock(object state)
            {
                var tmp = (CallbackBlock) state;

                tmp.CallbackAction(tmp.Param);

                tmp.Dispose();
            }


        }
    }

    public interface ICallbackBlock : IDisposable
    {
        void Stop();

        void Reset();
    }


}