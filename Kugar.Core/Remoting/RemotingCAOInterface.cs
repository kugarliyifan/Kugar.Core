using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Remoting
{


    public class RemotingCAOInterfaceOrInstance<T> : MarshalByRefObject, ICAOInterfaceFactory where T : MarshalByRefObject, new()
    {
        //private static Dictionary<Type, object> cacheObj = new Dictionary<Type, object>();

        public object CreateInstance()
        {
            CallContext.FreeNamedDataSlot("ObservedIP");
            return new T(); // class factory create
        }

        public object CreateInstance(string serverIPAddress)
        {
            CallContext.SetData("ObservedIP", serverIPAddress);
            return new T(); // class factory create
        }

        //public object CreateInstance()
        //{
        //    CallContext.FreeNamedDataSlot("ObservedIP");

        //    object value;

        //    lock (cacheObj)
        //    {
        //        var targetType = typeof(T);

        //        if (cacheObj.ContainsKey(targetType))
        //        {
        //            value = cacheObj[targetType];
        //        }
        //        else
        //        {
        //            value = new T();

        //            cacheObj.Add(targetType, value);
        //        }
        //    }

        //    return value;
        //}

        //public object CreateInstance(string serverIPAddress)
        //{
        //    CallContext.SetData("ObservedIP", serverIPAddress);

        //    //object value;

        //    //lock (cacheObj)
        //    //{
        //    //    var targetType = typeof (T);

        //    //    if (cacheObj.ContainsKey(targetType))
        //    //    {
        //    //        value= cacheObj[targetType];
        //    //    }
        //    //    else
        //    //    {
        //    //        value =new T();

        //    //        cacheObj.Add(targetType,value);
        //    //    }
        //    //}

        //    return new T();
        //}
    }

    internal class RemotingCAOSingletonHelper
    {
        private static Dictionary<Type, object> cacheObj = new Dictionary<Type, object>();
        private static readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        public static void AddOrUpdate(Type type,MarshalByRefObject value)
        {
            readerWriterLock.EnterWriteLock();
            if (cacheObj.ContainsKey(type))
            {
                cacheObj[type] = value;
            }
            else
            {
                cacheObj.Add(type, value);
            }

            readerWriterLock.ExitWriteLock();
        }

        public static void AddOrUpdate<T1>(T1 value) where T1 : MarshalByRefObject
        {
            AddOrUpdate(typeof (T1), value);
        }

        public static bool UpdateValue(Type type,MarshalByRefObject value)
        {
            bool isSuccess;
            readerWriterLock.EnterWriteLock();

            if (cacheObj.ContainsKey(type))
            {
                cacheObj[type] = value;
                isSuccess = true;
            }
            else
            {
                isSuccess = false;
            }

            readerWriterLock.ExitWriteLock();

            return isSuccess;
        }


        public static bool UpdateValue(MarshalByRefObject oldValue,MarshalByRefObject newValue,Type[] types)
        {
            readerWriterLock.EnterWriteLock();

            foreach (var o in cacheObj)
            {
                if (o.Value==oldValue &&  o.Key.CheckIn(types))
                {
                    cacheObj[o.Key] = newValue;
                }
            }

            readerWriterLock.ExitWriteLock();

            return true;
        }

        public static void Remove(Type type)
        {
            readerWriterLock.EnterWriteLock();
            cacheObj.Remove(type);
            readerWriterLock.ExitWriteLock();
        }

        public static object GetTypeValue(Type type, object defaultValue)
        {
            readerWriterLock.EnterReadLock();

            object v = null;

            if (!cacheObj.TryGetValue(type, out v))
            {
                v = defaultValue;
            }

            readerWriterLock.ExitReadLock();

            return v;
        }
    }

    public class RemotingCAOSingleton : MarshalByRefObject, ICAOSingletonFactory 
    {
        //public RemotingCAOSingleton(MarshalByRefObject value)
        //{
        //    RemotingCAOSingleton.AddOrUpdate(typeof(T),value);
        //}

        #region Implementation of ICAOInterfaceFactory

        public object CreateInstance(Type type)
        {
            CallContext.FreeNamedDataSlot("ObservedIP");

            var v = RemotingCAOSingletonHelper.GetTypeValue(type, null);

            if (v == null)
            {
                throw new TargetException("不存在指定类型的值，请确保单实例的值为存在的");
            }

            return v;
        }

        public object CreateInstance(Type type, string serverIPAddress)
        {
            CallContext.SetData("ObservedIP", serverIPAddress);

            var v = RemotingCAOSingletonHelper.GetTypeValue(type, null);

            if (v == null)
            {
                throw new TargetException("不存在指定类型的值，请确保单实例的值为存在的");
            }

            return v;
        }

        #endregion
    }

    public interface ICAOSingletonFactory
    {
        object CreateInstance(Type type);
        object CreateInstance(Type type, string serverIPAddress);

    }


    public interface ICAOInterfaceFactory
    {
        object CreateInstance();
        object CreateInstance(string serverIPAddress);

    }
}
