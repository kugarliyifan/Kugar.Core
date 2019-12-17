using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.MultiThread
{

    /// <summary>
    ///     使用额外线程执行工作项
    /// </summary>
    public interface IThreadTaskQueue:IDisposable
    {
        /// <summary>
        ///     执行指定的函数
        /// </summary>
        /// <param name="method">函数</param>
        /// <param name="state">传入的值</param>
        void Execute(WaitCallback method, object state);

        /// <summary>
        ///     使用自定义工作项执行操作
        /// </summary>
        /// <param name="executeItem"></param>
        void Execute(IThreadQueueExecuteItem executeItem);

        /// <summary>
        ///     当前队列中还有多少个待执行的工作项
        /// </summary>
        int Count { get; }

        void Clear();

        void Close();
    }

    /// <summary>
    ///     额外线程执行的工作项接口
    /// </summary>
    public interface IThreadQueueExecuteItem : IDisposable
    {
        /// <summary>
        ///     传入的额外值
        /// </summary>
        object State { get; }

        /// <summary>
        ///     执行的过程
        /// </summary>
        /// <param name="state"></param>
        void Execute(object state);
    }


    /// <summary>
    ///     在单一线程里,执行队列中的排序执行指定的操作
    /// </summary>
    public class SingleThreadQueue : IThreadTaskQueue
    {
#if NET2
        private static IThreadTaskQueue defaultQueue=new SingleThreadQueue();
#else
        private static Lazy<IThreadTaskQueue> defaultTimer = new Lazy<IThreadTaskQueue>(OnLazyInit, true);
#endif
        private Queue<IThreadQueueExecuteItem> workQueue = new Queue<IThreadQueueExecuteItem>();
        private AutoResetEvent au=new AutoResetEvent(false);
        private Thread runThread = null;
        private bool isDisposed = false;
        private object lockerObj=new object();
        private bool _isClosed = false;

        public SingleThreadQueue()
        {
            runThread = new Thread(run);
            runThread.Start();
        }

        public void Execute(WaitCallback method, object state)
        {
            //lock (workQueue)
            //{
            //    workQueue.Enqueue(new ThreadQueueExecuteItem(method, state));
            //    au.Set();
            //}

            var item = new ThreadQueueExecuteItem(method, state);

            this.Execute(item);

        }

        public void Execute(IThreadQueueExecuteItem executeItem)
        {
            if (_isClosed)
            {
                return;
            }

            lock (lockerObj)
            {
                workQueue.Enqueue(executeItem);
                au.Set();
            }
        }

        public int Count
        {
            get
            {
                lock (lockerObj)
                {
                    return workQueue.Count;
                }
            }
        }

        public void Clear()
        {
            lock (lockerObj)
            {
                workQueue.Clear();
            }
        }

        public void Close()
        {
            _isClosed = true;

            Clear();

            lock (lockerObj)
            {
                if (!isDisposed)
                {
                    runThread.Abort();
                    au.Set();
                    isDisposed = true;
                    workQueue = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        ~SingleThreadQueue()
        {
            Dispose();
        }

        /// <summary>
        ///     获取一个全局公用的CallbackTimer
        /// </summary>
        public static IThreadTaskQueue Default
        {
            get
            {
#if NET2
                return defaultQueue;
#else
                return defaultTimer.Value;
#endif
            }
        }


#if !NET2
        private static IThreadTaskQueue OnLazyInit()
        {
            return new SingleThreadQueue();
        }
#endif

        private void run(object state)
        {
            IThreadQueueExecuteItem processItem = null;

            while (true)
            {
                lock (workQueue)
                {
                    if (workQueue.Count<=0)
                    {
                        processItem = null;
                    }
                    else
                    {
                        processItem = workQueue.Dequeue();
                    }
                }

                if (processItem==null)
                {
                    au.WaitOne(500);
                }
                else
                {
                    try
                    {
                        processItem.Execute(state);
                    }
                    catch
                    {
                        
                    }
                    
                    Thread.Yield();
                    Thread.Sleep(100);
                }
            }
        }

    }

    /// <summary>
    ///     使用指定数量的多线程,执行队列中的工作项<br/>
    ///     该类用于处理,当有N多个任务时,但同时又需要限制并发执行的任务的个数时,可以使用<br/>
    ///     如果限制的同时执行任务数为1个,请使用SingleThreadQueue对象,大于1个的,使用MultiThreadTaskQueue<br/>
    ///     注:使用的场景如:下载一个网页,网页上有100个图片需要下载,,这时候,由于性能的问题,同时需限制为同时只能下载10个图片,则可以使用本类
    /// </summary>
    public class MultiThreadTaskQueue : IThreadTaskQueue
    {
        #if NET4
        private const int _defaultQueueMaxThreadCount = 3;

        private static Lazy<IThreadTaskQueue> _defaultQueue = new Lazy<IThreadTaskQueue>(OnLazyInit, true);
        #endif

        private int _maxThreadCount;
        private ConcurrentQueue<IThreadQueueExecuteItem> _queue = new ConcurrentQueue<IThreadQueueExecuteItem>();
        private Thread[] _threads = null;
        private bool _isClosed = false;

        /// <summary>
        ///     并发执行的任务个数
        /// </summary>
        /// <param name="maxThreadCount"></param>
        public MultiThreadTaskQueue(int maxThreadCount)
        {
            _maxThreadCount=maxThreadCount;
            
            _threads=new Thread[maxThreadCount];

            for (int i = 0; i < maxThreadCount; i++)
            {
                _threads[i] = new Thread(threadItemExecute);
                _threads[i].Start();
            }
        }

        public int MaxThreadCount { get { return _maxThreadCount; } }

        public void Execute(WaitCallback method,object state)
        {
            var t = new ThreadQueueExecuteItem(method,state);
            Execute(t);
            //_queue.Enqueue(t);
        }

        public void Execute(IThreadQueueExecuteItem executeItem)
        {
            if (_isClosed)
            {
                return;
            }

            _queue.Enqueue(executeItem);
        }

        #if NET4

        public static IThreadTaskQueue Default
        {
            get { return _defaultQueue.Value; }
        }

        private static IThreadTaskQueue OnLazyInit()
        {
            return new MultiThreadTaskQueue(_defaultQueueMaxThreadCount);
        }

        #endif

        public int Count{get { return _queue.Count; }}
        public void Clear()
        {
            try
            {
                if (!_queue.IsEmpty)
                {
                    IThreadQueueExecuteItem item = null;
                    while (!_queue.IsEmpty)
                    {

                        _queue.TryDequeue(out item);
                    }
                }
            }
            catch (Exception)
            {
               
            }
        }

        public void Close()
        {
            _isClosed = true;

            _isStop = true;

            Thread.Sleep(2000);

            Clear();

            foreach (var thread in _threads)
            {
                try
                {
                    thread.Abort();
                }
                catch (Exception)
                {
                    continue;
                }
            }


        }

        private bool _isStop = false;
        private void threadItemExecute(object state)
        {
            while (!_isStop)
            {
                IThreadQueueExecuteItem item = null;

                if(_queue.TryDequeue(out item) && !_isStop)
                {
                    try
                    {
                        item.Execute(item.State);
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                    Thread.Sleep(100);
                    
                }
                else
                {
                    Thread.Yield();
                    Thread.Sleep(200);
                }
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();

        }

        #endregion

    }


    internal class ThreadQueueExecuteItem : IThreadQueueExecuteItem
    {
        private WaitCallback _method;
        private object _state = null;

        public ThreadQueueExecuteItem(WaitCallback method, object state)
        {
            _method = method;
            _state = state;
        }

        public object State{get { return _state; }}

        public void Execute(object state)
        {
            _method(_state);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _method = null;
        }

        #endregion
    }

    public class AsyncThreadQueueExecuteItem:IThreadQueueExecuteItem
    {
        #region Implementation of IDisposable

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IThreadQueueExecuteItem

        /// <summary>
        ///     传入的额外值
        /// </summary>
        public object State { get; private set; }

        /// <summary>
        ///     执行的过程
        /// </summary>
        /// <param name="state"></param>
        public void Execute(object state)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
