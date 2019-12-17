using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Events
{
    /// <summary>
    ///     EventHandler类的扩展类
    /// </summary>
    public static class EventHelper
    {

        /// <summary>
        ///     引发指定的函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="EventArgs"></param>
        /// <param name="Delegate"></param>
        public static void Raise<T>(T EventArgs, Action<T> Delegate) where T : class
        {
            if (Delegate != null)
            {
                Delegate(EventArgs);
            }
        }

        public static void Raise<T>(EventHandler<T> Delegate, object Sender, T EventArg)
            #if !NET45
            where T : System.EventArgs
            #endif
        {
            if (Delegate != null)
            {
                Delegate(Sender, EventArg);
            }
        }

        public static void Raise(EventHandler eventHandler,object sender)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, new EventArgs());
            }
        }

        public static void Raise<T>(EventHandler<T> eventHandler, object sender, Func<T> eventArgsFactory)
#if !NET45
            where T : System.EventArgs
#endif
        {
            if (eventHandler != null)
            {
                var e = eventArgsFactory();

                eventHandler(sender, e);
            }
        }

        public static void RaiseIgoneError(EventHandler eventHandler,object sender)
        {
            if (eventHandler != null)
            {
                var methodList = eventHandler.GetInvocationList();
                var e = new EventArgs();

                foreach (var method in methodList)
                {
                    try
                    {
                        ((EventHandler)method)(sender, e);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public static void RaiseIgoneError<T>(EventHandler<T> eventHandler, object sender, T eventArg)
#if !NET45
            where T : System.EventArgs
            #endif
        {
            if (eventHandler != null)
            {
                var methodList = eventHandler.GetInvocationList();

                foreach (var method in methodList)
                {
                    try
                    {
                        ((EventHandler<T>)method)(sender, eventArg);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public static void RaiseIgoneError<T>(EventHandler<T> eventHandler, object sender, Func<T> eventArgsFactory)
#if !NET45
            where T : System.EventArgs
            #endif
        {
            if (eventHandler != null)
            {
                var methodList = eventHandler.GetInvocationList();
                var e = eventArgsFactory();
                foreach (var method in methodList)
                {
                    try
                    {
                        ((EventHandler<T>)method)(sender, e);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public static void RaiseAsync(EventHandler eventHandler, object sender)
        {
            if (eventHandler != null)
            {

                var invokeList = eventHandler.GetInvocationList();

                if (!invokeList.HasData())
                {
                    return;
                }

                if (invokeList.Length==1)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ((EventHandler) invokeList.FirstOrDefault())(sender, EventArgs.Empty);
                        }
                        catch (Exception)
                        {
                        }

                    });
                }
                else
                {
                    Task.Factory.ContinueWhenAll(
                        invokeList.Select(x => Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                ((EventHandler)x)(sender, EventArgs.Empty);
                            }
                            catch (Exception)
                            {
                            }

                        })).ToArray(), (t) => { });                    
                }


            }
        }

        /// <summary>
        ///    异步引发事件,,并忽略错误
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Delegate"></param>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public static void RaiseAsync<T>(EventHandler<T> eventHandler, object sender, T eventArgs)
#if !NET45
            where T : System.EventArgs
#endif
        {
            if (eventHandler != null)
            {
                var invokeList = eventHandler.GetInvocationList();

                if (!invokeList.HasData())
                {
                    return;
                }

                Task.Factory.ContinueWhenAll(
                    invokeList.Select(x => Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ((EventHandler<T>) x)(sender, eventArgs);
                        }
                        catch (Exception)
                        {
                        }
                        
                    })).ToArray(), (t) => { });

                //ThreadPool.QueueUserWorkItem(asyncCallback<T>, new AsyncEventArgs<T>(sender, eventArgs,Delegate));
            }
        }

        /// <summary>
        ///    异步引发事件,,并忽略错误
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Delegate"></param>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public static void RaiseAsync<T>(EventHandler<T> eventHandler, object sender, Func<T> eventArgsFactory, Action afterRaise = null)
#if !NET45
            where T : System.EventArgs
#endif
        {
            if (eventHandler != null)
            {
                var invokeList = eventHandler.GetInvocationList();

                if (!invokeList.HasData())
                {
                    return;
                }

                var e = eventArgsFactory();

                Task.Factory.ContinueWhenAll(
                    invokeList.Select(x => Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ((EventHandler<T>) x)(sender, e);
                        }
                        catch (Exception)
                        {
                        }
                        
                    })).ToArray(),
                    (t) => { if (afterRaise != null) afterRaise(); });
                    

                //Task.Factory.ContinueWhenAll(
                //    new[]
                //    {
                //        Task.Factory.StartNew(asyncCallback<T>, new AsyncEventArgs<T>(sender, e, eventHandler))
                //    },
                //    (t) => { if (afterRaise != null) afterRaise(); });

                //ThreadPool.QueueUserWorkItem(asyncCallback<T>, new AsyncEventArgs<T>(sender, e, eventHandler));
            }
        }



        private static void asyncCallback<T>(object state)
#if !NET45
            where T : System.EventArgs
            #endif
        {
            if (state is AsyncEventArgs<T>)
            {
                var temp = (AsyncEventArgs<T>)state;

                temp.Execute();
            }
        }

        private static void asyncCallback(object state)
        {
            if (state is AsyncEventArgs)
            {
                var temp = (AsyncEventArgs)state;

                temp.Execute();
            }
        }

        public static T2 Raise<T1, T2>(T1 EventArgs, Func<T1, T2> Delegate)
        {
            if (Delegate != null)
            {
                return Delegate(EventArgs);
            }
            return default(T2);
        }

        internal class AsyncEventArgs<T>
#if !NET45
            where T : System.EventArgs
#endif
        {
            public AsyncEventArgs(object sender, T e,EventHandler<T> d) 
            {
                this.sender = sender;
                this.e = e;
                this.delgate = d;

            }

            public virtual void Execute()
            {
                if (delgate!=null)
                {
                    delgate(this.sender, this.e);
                }
            }

            public object sender;
            public T e;
            public EventHandler<T> delgate;
        }

        internal class AsyncEventArgs
        {
            public AsyncEventArgs(object sender, EventHandler d)
            {
                this.sender = sender;
                this.delgate = d;
            }

            public virtual void Execute()
            {
                if (delgate != null)
                {
                    delgate(this.sender, new EventArgs());
                }
            }

            public object sender;
            public EventHandler delgate;
        }
    }
}
