using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kugar.Core.BaseStruct
{
    public abstract class AsyncResultBase : IAsyncResult
    {
        private Lazy<AutoResetEvent> _lazyLock = new Lazy<AutoResetEvent>(getLocker);
        private bool _isCompleted = false;
        private object _state = null;
        private bool _completedSynchronously = false;
        private Exception _error = null;
        private AsyncCallback _callback;

        protected AsyncResultBase(AsyncCallback callback, object state=null)
        {
            _callback = callback;
            _state = state;
        }

        protected virtual void Execute(Exception error)
        {
            _error = error;

            this.IsCompleted = true;

            if (_callback!=null)
            {
                ThreadPool.QueueUserWorkItem((o) => _callback(this), null);
            }
        }

        #region Implementation of IAsyncResult

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;

                if (value && _lazyLock.IsValueCreated)
                {
                    _lazyLock.Value.Set();
                }
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _lazyLock.Value; }
        }

        public object AsyncState
        {
            get { return _state; }
        }

        public bool CompletedSynchronously
        {
            get { return _completedSynchronously; }
            set { _completedSynchronously = value; }
        }

        #endregion

        public Exception Error
        {
            get { return _error; }
        }

        private static AutoResetEvent getLocker()
        {
            return new AutoResetEvent(false);
        }
    }

    public abstract class AsyncResultInherit:IAsyncResult
    {
        private IAsyncResult _iar = null;

        public AsyncResultInherit(IAsyncResult baseIar)
        {
        }

        public bool IsCompleted
        {
            get { return _iar.IsCompleted; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _iar.AsyncWaitHandle; }
        }

        public object AsyncState
        {
            get { return _iar.AsyncState; }
        }

        public bool CompletedSynchronously
        {
            get { return _iar.CompletedSynchronously; }
        }

        public IAsyncResult BaseAsyncResult { get { return _iar; } }
    }
}
