using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    /// 一个定时过期的Lazy加载器,创建的值,可以在指定的时间后过期,下次调用时,将自动重新调用valueFactory函数创建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExpiringLazy<T> :IDisposable
    {
        private T _value = default(T);
        private ReaderWriterLockSlim _locker=new ReaderWriterLockSlim();
        private ICallbackBlock _block = null;
        private bool _isValueCreated = false;
        private int _timeout = -1;
        private Func<T> _valueFactory;
        private bool _isDisposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="expireMillsecond">过期时间,单位为毫秒</param>
        /// <param name="valueFactory">值创建的函数</param>
        public ExpiringLazy(int expireMillsecond,Func<T> valueFactory)
        {
            if (expireMillsecond<1000)
            {
                throw new ArgumentOutOfRangeException(nameof(expireMillsecond),"过期时间不能低于1秒钟");
            }

            _timeout = expireMillsecond;

            _valueFactory = valueFactory;
        }

        /// <summary>
        /// 返回当前值,如果值未创建,则自动调用函数获取值
        /// </summary>
        public T Value
        {
            get
            {
                _locker.EnterUpgradeableReadLock();

                if (_isDisposed)
                {
                    throw new Exception("对象已Dispose,无法继续调用");
                }

                if (_isValueCreated)
                {
                    _locker.ExitUpgradeableReadLock();

                    return _value;
                }

                _locker.EnterWriteLock();

                try
                {
                    _value = _valueFactory();

                    _isValueCreated = true;

                    _block = CallbackTimer.Default.Call(_timeout, callbackInvoker, _value);

                    return _value;
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _locker.ExitWriteLock();

                    _locker.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        /// 返回值是否已创建
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                _locker.EnterReadLock();

                var v = _isValueCreated;

                _locker.ExitReadLock();

                return v;


            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            _locker.EnterWriteLock();

            _isValueCreated = false;
            _block.Dispose();
            _value = default(T);

            _locker.ExitWriteLock();
        }

        private void callbackInvoker(object state)
        {
            _locker.EnterUpgradeableReadLock();

            //如果回调的时候的当前值不是原来传入回调时的值,则退出操作,防止出现并发
            if (!_value.Equals(state))
            {
                _locker.ExitUpgradeableReadLock();

                return;
            }

            _locker.EnterWriteLock();

            _isValueCreated=false;
            _value = default(T);
            
            _block.Dispose();
            _block = null;

            _locker.ExitWriteLock();

            _locker.ExitUpgradeableReadLock();
        }

        public void Dispose()
        {
            _locker.EnterWriteLock();

            _value = default(T);
            _isValueCreated = false;
            _block.Dispose();

            _isDisposed = true;
            
            _locker.ExitWriteLock();
        }
    }
}
