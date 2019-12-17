using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Kugar.Core.Collections;

namespace Kugar.Core.BaseStruct
{
    public interface IRecyclablePool<out T> : IDisposable where T : IRecyclable, IDisposable
    {
        /// <summary>
        ///     从已回收的对象池中,取出一个可用的对象
        /// </summary>
        /// <returns></returns>
        T Take();

        /// <summary>
        ///     关闭整个缓冲池
        /// </summary>
        void Close();

        /// <summary>
        ///     回收 一个对象
        /// </summary>
        /// <param name="obj"></param>
        void RecycleObject(IRecyclable obj);

        /// <summary>
        ///     当前总对象数
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     当前空闲的对象数量
        /// </summary>
        int FreeCount { get; }

        /// <summary>
        ///     回收池中对象的最大个数
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        ///     回收池中当前对象个数
        /// </summary>
        int CurrentCount { get; }

        event EventHandler<RecycleObjectEventArgs<IRecyclable>> RecycledObject;
        event EventHandler<DisposeObjectEventArgs<IRecyclable>> DisposedObject;
        event EventHandler<DisposingObjectEventArgs<IRecyclable>> DisposingObject;
    }

    /// <summary>
    ///     可回收对象的管理池,负责管理空闲对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     继承该类之后,需实现CreateRecyclableObject函数,并在子类的构造函数中调用 Init()函数进行初始化
    /// </remarks>
    public abstract class RecyclablePool<T>: IRecyclablePool<T> where T : IRecyclable,IDisposable
    {
        //private static Random rnd=new Random();

        protected HashSet<T> _cacheList = null;
        private LinkedListEx<T> _freeCacheList = null;

        private object lockerObject=new object();
        private SemaphoreSlim _locker = null;
        private int _maxLength = 0;
        private int _minLength = 0;
        private bool _isDisposed = false;

        #region "构造函数"

        protected RecyclablePool():this(100,10)
        {
            
        }

        protected RecyclablePool(int maxLength,int minLength)
        {
            if (maxLength<=0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _maxLength = maxLength;
            _minLength = minLength;



            _cacheList = new HashSet<T>();

            _freeCacheList = new LinkedListEx<T>();

            //var lockerName = "KugarPoolBase" + rnd.Next();

            _locker = new SemaphoreSlim(0, MaxLength);

        }

        #endregion

        /// <summary>
        ///     初始化连接池
        /// </summary>
        protected virtual void Init()
        {
            for (int i = 0; i < _minLength; i++)
            {
                NewItem(true);
            }

        }

        /// <summary>
        ///     从已回收的对象池中,取出一个可用的对象
        /// </summary>
        /// <returns></returns>
        public virtual T Take()
        {
            T ret = default(T);

            lock (lockerObject)
            {
                if (_freeCacheList.Count <= 0 && _cacheList.Count < _maxLength)
                {
                    ret=NewItem(false);

                    return ret;
                }                
            }

            _locker.Wait();

            lock (lockerObject)
            {
                ret = _freeCacheList.RemoveFirst(false).Value;
            }

            return ret;
        }

        /// <summary>
        ///     关闭整个缓冲池
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        void IRecyclablePool<T>.RecycleObject(IRecyclable obj) 
        {
            RecycleObject((T)obj);
        }

        /// <summary>
        ///     回收 一个对象
        /// </summary>
        /// <param name="obj"></param>
        public void RecycleObject(T obj)
        {
            lock (lockerObject)
            {
                if (!_cacheList.Contains(obj))
                {
                    return;
                }            
                
                _freeCacheList.AddLast(obj);
                _locker.Release();
            }
   
            
            OnRecycledObject(obj);

        }

        /// <summary>
        ///     实现IDisposable接口
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_locker != null)
            {
                //locker.Release();
                _locker.Dispose();
                //locker.Dispose();
                _locker = null;

                _freeCacheList.Clear();

                _freeCacheList = null;

                foreach (var item in _cacheList)
                {
                    DisposeObject(item);
                }

                _cacheList.Clear();

                //cacheList.Clear();

                _isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        ///     当前总对象数
        /// </summary>
        public int Count
        {
            get
            {
                    return _cacheList.Count;
            }
        }

        /// <summary>
        ///     当前空闲的对象数量
        /// </summary>
        public int FreeCount
        {
            get
            {
                lock (_freeCacheList)
                {
                    return _freeCacheList.Count;
                }
            }
        }

        /// <summary>
        ///     回收池中对象的最大个数
        /// </summary>
        public int MaxLength {get { return _maxLength; }}

        /// <summary>
        ///     回收池中当前对象个数
        /// </summary>
        public int CurrentCount { get { return _locker.CurrentCount; }}

        /// <summary>
        ///     创建一个新的对象
        /// </summary>
        /// <param name="isIntoFreeQueue">新创建的对象是否进入空闲列表</param>
        /// <returns></returns>
        protected virtual T NewItem(bool isIntoFreeQueue)
        {
            T obj = default(T);

            if (Count<=_maxLength)
            {
                 obj = CreateRecyclableObject();

                //obj.Pool = this;

                //lock (_cacheList)
                    _cacheList.Add(obj);               
            }



            if (isIntoFreeQueue)
            {
                lock (lockerObject)
                {
                    _freeCacheList.AddLast(obj);
                    _locker.Release(1);
                }
            }

            return obj;
        }

        /// <summary>
        ///     构建一个新的对象,该函数必须由继承的类实现
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateRecyclableObject();

        /// <summary>
        ///     真正调用对象的Dispose方法
        /// </summary>
        /// <param name="obj"></param>
        protected void DisposeObject(T obj)
        {
            var t = OnDisposingObject(obj);

            if (t)
            {
                return;
            }

            //cacheList.Remove(obj);


            obj.DisposeObject();

            OnDisposedObject(obj);

            GC.SuppressFinalize(obj);
        }

        #region "事件"

        protected virtual void OnRecycledObject(T obj)
        {
            if (RecycledObject != null)
            {
                var e = new RecycleObjectEventArgs<IRecyclable>(obj);

                RecycledObject(this, e);
            }
        }

        protected virtual void OnDisposedObject(T obj)
        {
            if (DisposedObject != null)
            {
                var e = new DisposeObjectEventArgs<IRecyclable>(obj);

                DisposedObject(this, e);
            }
        }

        protected virtual bool OnDisposingObject(T obj)
        {
            if (DisposingObject != null)
            {
                var e = new DisposingObjectEventArgs<IRecyclable>(obj);

                DisposingObject(this, e);

                return e.Cancel;
            }

            return false;
        }

        public event EventHandler<RecycleObjectEventArgs<IRecyclable>> RecycledObject;
        public event EventHandler<DisposeObjectEventArgs<IRecyclable>> DisposedObject;
        public event EventHandler<DisposingObjectEventArgs<IRecyclable>> DisposingObject;

        #endregion

        ~RecyclablePool()
        {
            Dispose();
        }
    }

    public class RecycleObjectEventArgs<T> : EventArgs where T : IRecyclable
    {
        public RecycleObjectEventArgs(T data)
        {
            Data = data;
        }

        /// <summary>
        ///     当前被回收的对象
        /// </summary>
        public T Data {protected set; get; }
    }

    /// <summary>
    ///     释放对象时，传递的参数对象
    /// </summary>
    public class DisposeObjectEventArgs<T> : EventArgs where T : IRecyclable
    {
        public DisposeObjectEventArgs(T data)
        {
            Data = data;
        }

        /// <summary>
        ///     当前被回收的对象
        /// </summary>
        public T Data {protected set; get; }
    }

    public class DisposingObjectEventArgs<T> : DisposeObjectEventArgs<T> where T : IRecyclable
    {
        public DisposingObjectEventArgs(T data)
            : base(data)
        {
            Cancel = false;
        }

        /// <summary>
        ///     如果不想释放对象，可将该值设置为false，则该对象不会从缓冲池中移除并释放
        /// </summary>
        public bool Cancel { set; get; }


    }

    public interface IRecyclable:IDisposable //where T:IDisposable
    {
        IRecyclablePool<IRecyclable> Pool { get; set; }

        /// <summary>
        ///     当对象真正释放的时候，回收器会调用该函数
        /// </summary>
        void DisposeObject();

    }
}
