using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kugar.Core.MultiThread
{
    public delegate T SimpleTaskWaitForInvoke<T>(T lastValue);

    public class SimpleTask
    {
        public static SimpleTask<T> BeginTask<T>(SimpleTaskWaitForInvoke<T> method)
        {
            return new SimpleTask<T>(null, method);

            //T retv = default(T);

            //bool completed = false;

            //object sync = new object();

            //IAsyncResult asyncResult = method.BeginInvoke(
            //        iAsyncResult =>
            //        {
            //            lock (sync)
            //            {
            //                completed = true;
            //                retv = method.EndInvoke(iAsyncResult);
            //                Monitor.Pulse(sync);
            //            }
            //        }, null);

            //return delegate
            //{
            //    lock (sync)
            //    {
            //        if (!completed)
            //        {
            //            Monitor.Wait(sync);
            //        }

            //        return retv;
            //    }
            //};
        }
        
        
    }

    public class SimpleTask<T>
    {
        internal SimpleTask(SimpleTask<T> parent, SimpleTaskWaitForInvoke<T> method)
        {
            this.Parent = parent;
            callback = method;
        }

        private SimpleTaskWaitForInvoke<T> callback;

        internal SimpleTask<T> Parent { get; set; }

        public T Invoke(T startValue)
        {
            T retv = default(T);

            if (Parent != null)
            {
                retv = Parent.Invoke(startValue);
            }

            bool completed = false;

            //object sync = new object();

            //Monitor.Enter(sync);

            var au = new AutoResetEvent(false);

            IAsyncResult asyncResult = callback.BeginInvoke(retv,
                    iAsyncResult =>
                    {
                        completed = true;
                        retv = callback.EndInvoke(iAsyncResult);
                        //Monitor.Pulse(sync);
                        au.Set();
                    }, null);


            //Monitor.Wait(sync);

            //Monitor.Exit(sync);

            au.WaitOne();

            if (asyncResult.IsCompleted)
            {

            }

            return retv;

        }

        public void Invoke()
        {
            this.Invoke(default(T));
        }

        public SimpleTask<T> AfterInvoke(SimpleTaskWaitForInvoke<T> method)
        {
            return new SimpleTask<T>(this, method);
        }
    }

}
