using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    /// 自动过期的值,,用于比如token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoExpireValue<T>
    {
        private T _value = default(T);
        private int _expireSec = 0;
        private Func<T> _creator = null;
        private bool _isCreated = false;
        private object _locker = new object();
        private TimerPool_Item _timerItem = null;

        /// <summary>
        /// 创建自动过期
        /// </summary>
        /// <param name="creator">创建函数</param>
        /// <param name="expireSec">过期秒数,单位是秒</param>
        public AutoExpireValue(Func<T> creator, int expireSec)
        {
            _creator = creator;
            _expireSec = expireSec;
            _timerItem = TimerPool.Default.Add(_expireSec * 1000, timer_Callback, null, true);
           
        }

        public T Value
        {
            get
            {
                if (!_isCreated)
                {
                    lock (_locker)
                    {
                        if (!_isCreated)
                        {
                            _value = _creator();
                            _isCreated = true;
                        }
                    }
                }

                return _value;
            }
            set
            {
                lock (_locker)
                {
                    _isCreated = true;
                    _value = value;
                }

            }
        }

        public void Reset()
        {
            lock (_locker)
            {
                _isCreated = false;
                onExpire();
            }
        }

        /// <summary>
        /// 过期时,触发该函数
        /// </summary>
        public event EventHandler ExpireCallback;

        private void timer_Callback(object state)
        {
            lock (_locker)
            {
                _isCreated = false;
                onExpire();
            }

        }

        private void onExpire()
        {
            ExpireCallback?.Invoke(this, EventArgs.Empty);
        }
    }
}
