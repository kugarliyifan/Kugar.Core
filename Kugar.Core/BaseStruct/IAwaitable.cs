using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kugar.Core.BaseStruct
{
    public class AwaiterBase<T> : IAwaitable<T>, IAwaiter<T>, INotifyCompletion
    {
        private Action _continuation = null;
        private bool _isCompleted = false;
        private SynchronizationContext _syncContext = null;
        private T _result = default(T);
        private Exception _error = null;

        public AwaiterBase()
        {
            _syncContext=SynchronizationContext.Current;
        }

        public IAwaiter<T> GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;

        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                
                var oldValue = _isCompleted;

                _isCompleted = value;

                if (!oldValue && value)
                {
                    Task.Factory.StartNew(invoke, _continuation);

                    //new Task(invoke, _continuation).Start();
                }
            }
        }

        private void invoke(object state)
        {
            try
            {
                if (_syncContext != null)
                {
                    //_syncContext.OperationStarted();

                    //if (sameStackFrame)
                    //    _syncContext.Post(invoke, _continuation);
                    //else
                    //SynchronizationContext.SetSynchronizationContext(_syncContext);

                    //var c = (SendOrPostCallback) state;

                    _syncContext.Send((b) => { ((Action)state)(); }, state);

                    
                    //_syncContext.Send(invoke, _continuation);

                    //Task.Factory.StartNew(_continuation,taskcr TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)



                    //_syncContext.Send(invoke, _continuation);

                    //_syncContext.OperationCompleted();

                    //_syncContext = null;
                }
                else
                {
                    if (state != null && state is Action tmp)
                    {
                        tmp();
                    }
                }
                
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public virtual T GetResult()
        {
            if (_error!=null)
            {
                throw _error;
            }

            return _result;
        }

        public virtual Exception GetError()
        {
            return _error;
        }

        public virtual void SetResult(T result, bool isCompleted = true)
        {
            _result = result;

            IsCompleted = isCompleted;
        }

        public virtual void SetError(Exception error)
        {
            _error = error;

            IsCompleted = true;
        }

        /// <summary>
        /// 创建一个过期的Awaiter
        /// </summary>
        /// <param name="timeout">过期时间,时间为毫秒</param>
        /// <param name="onTimeoutCallback">过期时,回调</param>
        /// <returns></returns>
        public static AwaiterBase<TType> Create<TType>(int timeout, EventHandler onTimeoutCallback = null)
        {
            if (timeout<=0)
            {
                return new TimeoutAwaiter<TType>(timeout,onTimeoutCallback);
            }
            else
            {
                return new AwaiterBase<TType>();
            }
        }

        /// <summary>
        /// 创建一个不过期的Awaiter
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static AwaiterBase<TType> Create<TType>()
        {
            return new AwaiterBase<TType>();
        }
    }

    /// <summary>
    /// 可设置超时的Awaiter,当超时时,将抛出错误
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeoutAwaiter<T> : AwaiterBase<T>
    {
        private ICallbackBlock block = null;
        private EventHandler _onTimeoutCallback = null;
        private bool _whenTimeoutThrowException = true;

        public TimeoutAwaiter(int timeout,EventHandler onTimeoutCallback=null, bool whenTimeoutThrowException = true) : base()
        {
            if (timeout>0)
            {
                block=CallbackTimer.Default.Call(timeout, timeoutCallback);
            }

            _onTimeoutCallback = onTimeoutCallback;
            _whenTimeoutThrowException = whenTimeoutThrowException;
        }

        private void timeoutCallback(object state)
        {
            if (!IsCompleted)
            {
                try
                {
                    _onTimeoutCallback?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    
                }
                finally
                {
                    block.Stop();
                    block.Dispose();

                    if (_whenTimeoutThrowException)
                    {
                        base.SetError(new TimeoutException());
                    }
                    else
                    {
                        IsCompleted = true;
                    }
                }
            }
        }

        public override void SetResult(T result, bool isCompleted = true)
        {
            if (isCompleted)
            {
                block?.Dispose();
            }
            
            base.SetResult(result, isCompleted);

        }

        public override void SetError(Exception error)
        {
            block?.Dispose();
            base.SetError(error);

        }
    }

    /// <summary>
    /// 使用滑动的超时机制,最后一次调用SetResult后的timeout毫秒后,触发超时机制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SlideTimeoutAwaiter<T> : AwaiterBase<T>
    {
        private ICallbackBlock block = null;
        private EventHandler _onTimeoutCallback = null;
        private bool _whenTimeoutThrowException = true;

        public SlideTimeoutAwaiter(int timeout, EventHandler onTimeoutCallback = null, bool whenTimeoutThrowException=true) : base()
        {
            if (timeout > 0)
            {
                block = CallbackTimer.Default.Call(timeout, timeoutCallback);
            }

            _onTimeoutCallback = onTimeoutCallback;
            _whenTimeoutThrowException = whenTimeoutThrowException;
        }

        private void timeoutCallback(object state)
        {
            if (!IsCompleted)
            {


                try
                {
                    _onTimeoutCallback?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (_whenTimeoutThrowException)
                    {
                        base.SetError(new TimeoutException());
                    }
                    else
                    {
                        IsCompleted = true;
                        //_onTimeoutCallback?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public override void SetResult(T result, bool isCompleted = true)
        {
            if (isCompleted)
            {
                block?.Dispose();
            }
            else
            {
                block?.Reset();
            }

            base.SetResult(result, isCompleted);

        }

        public override void SetError(Exception error)
        {
            block?.Dispose();
            base.SetError(error);

        }
    }


    public interface IAwaitable<out TResult>
    {
        IAwaiter<TResult> GetAwaiter();
    }

    public interface IAwaiter<out TResult> : INotifyCompletion // or ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }

        TResult GetResult();
    }
}
