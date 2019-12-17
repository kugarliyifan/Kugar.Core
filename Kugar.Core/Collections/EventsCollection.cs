using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;

#if NET45
using System.Runtime.Remoting;      
  #endif


namespace Kugar.Core.BaseStruct
{
    public class EventsCollection<T> : MarshalByRefObject, IDisposable where T:EventArgs
    {
        private Dictionary<string, EventHandler<T>> eventList;
        private object lockerObj = new object();
        private bool _isDisponsed = false;

        public EventsCollection()
        {
            eventList = new Dictionary<string, EventHandler<T>>(10, StringComparer.CurrentCultureIgnoreCase);
        }

        public void AddEventHandler(string eventName, EventHandler<T> eventHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                if (eventList.ContainsKey(eventName))
                {
                    var handler = eventList[eventName];
                    handler += eventHandler;
                }
                else
                {
                    eventList.Add(eventName, eventHandler);
                }
            }
        }

        public void RemoveEventHandler(string eventName, EventHandler<T> eventHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                if (eventList.ContainsKey(eventName))
                {
                    var handler = eventList[eventName];
                    handler -= eventHandler;

                    var tlst = handler.GetInvocationList();

                    if (tlst.IsEmptyData())
                    {
                        eventList.Remove(eventName);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public void RaiseEvent(string eventName, object sender, T eventArgs, bool isAutoRemoveHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            if (!eventList.ContainsKey(eventName))
            {
                return;
                //throw new ArgumentNullException(@"eventName", @"不存在指定名称的事件");
            }

            Delegate[] handlerList = null;
            EventHandler<T> handler = null;

            lock (lockerObj)
            {
                handler = eventList[eventName];
                if (handler != null)
                {
                    handlerList = handler.GetInvocationList();
                }
                else
                {
                    return;
                }
            }

            if (handlerList.IsEmptyData())
            {
                return;
            }

            foreach (EventHandler<T> method in handlerList)
            {
                if (method == null)
                {
                    continue;
                }

                try
                {
                    method(sender, eventArgs);
                }
#if NET45
      catch (RemotingException)
                {
                    if (isAutoRemoveHandler)
                    {
                        handler -= method;
                    }
                }
#endif

                catch (Exception)
                {
                    continue;
                }

            }
        }

        public void Dispose()
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                foreach (var eventHandler in eventList)
                {
                    var method = eventHandler.Value;

                    if (method == null)
                    {
                        continue;
                    }

                    var lst = method.GetInvocationList();

                    foreach (EventHandler<T> handler in lst)
                    {
                        method -= handler;
                    }
                }
            }
        }
    }

    public class EventsCollection:MarshalByRefObject,IDisposable
    {
        private Dictionary<string, EventHandler> eventList;
        private object lockerObj=new object();
        private bool _isDisponsed = false;

        public EventsCollection()
        {
            eventList=new Dictionary<string, EventHandler>(10,StringComparer.CurrentCultureIgnoreCase);
        }

        public void AddEventHandler(string eventName,EventHandler eventHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                if (eventList.ContainsKey(eventName))
                {
                    var handler = eventList[eventName];
                    handler += eventHandler;
                }
                else
                {
                    eventList.Add(eventName, eventHandler);
                }
            }
        }

        public void RemoveEventHandler(string eventName,EventHandler eventHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                if (eventList.ContainsKey(eventName))
                {
                    var handler = eventList[eventName];
                    handler -= eventHandler;

                    var tlst = handler.GetInvocationList();

                    if (tlst.IsEmptyData())
                    {
                        eventList.Remove(eventName);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public void RaiseEvent(string eventName,object sender,EventArgs eventArgs,bool isAutoRemoveHandler)
        {
            if (_isDisponsed)
            {
                return;
            }

            if (!eventList.ContainsKey(eventName))
            {
                return;
                //throw new ArgumentNullException(@"eventName", @"不存在指定名称的事件");
            }

            Delegate[] handlerList = null;
            EventHandler handler = null;

            lock(lockerObj)
            {
                handler = eventList[eventName];
                if (handler!=null)
                {
                    handlerList = handler.GetInvocationList();
                }
                else
                {
                    return;
                }
            }

            if (handlerList.IsEmptyData())
            {
                return;
            }

            foreach (EventHandler method in handlerList)
            {
                if (method==null)
                {
                    continue;
                }

                try
                {
                    method(sender, eventArgs);
                }
#if NET45
      catch (RemotingException)
                {
                    if (isAutoRemoveHandler)
                    {
                        handler -= method;
                    }
                }
#endif

                catch (Exception)
                {
                    continue;
                }

            }
        }

        public void Dispose()
        {
            if (_isDisponsed)
            {
                return;
            }

            lock (lockerObj)
            {
                foreach (var eventHandler in eventList)
                {
                    var method = (EventHandler) eventHandler.Value;

                    if (method==null)
                    {
                        continue;
                    }

                    var lst = method.GetInvocationList();

                    foreach (EventHandler handler in lst)
                    {
                        method -= handler;
                    }
                }
            }
        }
    }
}
