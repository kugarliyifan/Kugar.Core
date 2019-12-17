using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  System.Data.SqlClient;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    ///     一个简单的对象缓存类,允许按键-值存放缓存数据
    /// </summary>
    public class GlobalCache : IEnumerable<KeyValuePair<string, object>>
    {
        private ConcurrentDictionary<string, ICacheItem> _cacheData = null;

        private TimerEx _timer = null;

        private static Lazy<GlobalCache> _lazy = new Lazy<GlobalCache>(() => new GlobalCache());

        public GlobalCache()
        {
            _cacheData = new ConcurrentDictionary<string, ICacheItem>();

            _timer = new TimerEx(Timer_Callback, 300, null);

            _timer.IsStopWhenRun = true;

            _timer.Start();

            //System.Data.SqlClient.SqlDependency
        }

        public bool Add(string key, object value, int timeout = -1)
        {
            return _cacheData.TryAdd(key, new TimeoutCachedItem(value, timeout));
        }

        public bool Add(string key, object value,SqlDependency dependency)
        {
            var cachedItem = new SqlDesCacheedItem(value, dependency);

            return _cacheData.TryAdd(key, cachedItem);
        }
        
        public void Remove(string key)
        {
            ICacheItem oldValue = null;

            _cacheData.TryRemove(key, out oldValue);
        }

        public bool Contain(string key)
        {
            return _cacheData.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            ICacheItem c;

            if (_cacheData.TryGetValue(key,out c))
            {
                value= c.Value;

                return true;
            }
            else
            {
                value = null;
                return false;
            }

           // return  _cacheData.TryGetValue(key, out value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            ICacheItem item;

            var ret = _cacheData.TryGetValue(key, out item);

            if (ret)
            {
                value = (T)item.Value;
            }
            else
            {
                value = default(T);
            }
            

            return ret;
        }
        
        public object TryGetValue(string key,object defaultValue=null)
        {
            object o;

            if (TryGetValue(key,out o))
            {
                return o;
            }
            else
            {
                return defaultValue;
            }

        }

        public T TryGetValue<T>(string key,T defaultValue=default(T))
        {
            T v;

            if (TryGetValue<T>(key,out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var o in _cacheData)
            {
                yield return new KeyValuePair<string, object>(o.Key,o.Value.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private void Timer_Callback(object state)
        {
            if (_cacheData.Count>0)
            {
                var lst = new List<string>();

                if (_cacheData.Count<10)
                {
                    foreach (var item in _cacheData)
                    {
                        if (item.Value.IsOverdue)
                        {
                            lst.Add(item.Key);
                        }  
                    }
                }
                else
                {
                     Parallel.ForEach(_cacheData, (item, s) =>
                    {
                        if ( item.Value.IsOverdue)
                        {
                            lst.Add(item.Key);
                        }  
                    });
                }
                
                if (lst.Count>0)
                {
                    foreach (var v in lst)
                    {
                        this.Remove(v);
                    }
                }
            }
        }


        /// <summary>
        ///     返回一个全局默认的缓存器
        /// </summary>
        public static GlobalCache Default
        {
            get { return _lazy.Value; }
        }

        private class TimeoutCachedItem : ICacheItem
        {
            public TimeoutCachedItem(object value, int timeout)
            {
                Value = value;
                Timeout = timeout;
                StartDt = DateTime.Now;
            }

            public object Value { set; get; }
            public bool IsOverdue 
            { 
                get { return Timeout>0 && StartDt.AddMilliseconds(Timeout) < DateTime.Now; }
            }
            public int Timeout { set; get; }
            public DateTime StartDt {private set; get; }
        }

        private class  SqlDesCacheedItem:ICacheItem
        {
            public SqlDesCacheedItem(object value, SqlDependency dependency)
            {
                Value = value;

                dependency.OnChange += dependency_OnChange;
            }

            

            public object Value { get; private set; }

            public bool IsOverdue { get; private set; }

            private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
            {
                IsOverdue = true;
            }
        }

        private interface ICacheItem
        {
            object Value { get; }
            bool IsOverdue { get; }
        }
    }
}
