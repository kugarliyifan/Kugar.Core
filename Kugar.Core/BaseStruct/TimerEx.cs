using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    ///     扩展了System.Threading.Timer类;<br/>
    ///     增加：<br/>
    ///     1.IsStopWhenRun属性，用于设定，当在定时器回调函数时，定时器是否停止计时（防止重入现象）。省去每次要用Timer的时候，都要在回调的时候去处理定时器的暂停<br/>
    ///     2.Start，Stop函数，用于启动或者关闭定时器。调用Stop函数后，不会打断正在调用的处理函数<br/>
    ///     3.IsRunning属性，用于判断定时器是否已经启动
    /// </summary>
    public class TimerEx:IDisposable
    {
        private Lazy<Timer> baseTimer = null;
        private bool isStopWhenRun = true;
        private TimerCallback _callback = null;
        private object lockerObj=new object();
        private int _runInterval = 0; 
        private bool isDisponsed = false;
        private bool isStop = false;
        private object _state = null;
        
        /// <summary>
        /// 定时器构造函数
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="interval"></param>
        /// <param name="state"></param>
        public TimerEx(TimerCallback callback, int interval,object state=null)
        {
            _state = state;
            _callback = callback;
            _runInterval = interval;

            baseTimer =new Lazy<Timer>(createTimer); //new Timer(timerCallback, state, interval, 0);
        }

        /// <summary>
        /// 用于设置,当定时器调用回调函数时,是否暂停定时器,函数调用完成后,再启动定时器,用于防止重入现象,默认为true
        /// </summary>
        public bool IsStopWhenRun
        {
            set { isStopWhenRun = value; }
            get { return isStopWhenRun; }
        }

        /// <summary>
        /// 开始定时器
        /// </summary>
        public void Start()
        {
            this.Start(false);
        }


        /// <summary>
        /// 开始定时器
        /// </summary>
        /// <param name="isCallFirstTime">如果为true,则定时器开始时,会调用一次回调函数之后,再继续进入定时,注意:回调函数不是在当前线程中被调用</param>
        public void Start(bool isCallFirstTime=false)
        {
            if (isDisponsed)
            {
                throw new Exception("该定时器已被关闭");
            }

            lock (lockerObj)
            {
                isStop = false;
                if (!isCallFirstTime)
                {
                    baseTimer.Value.Change(_runInterval, 0);
                }
                else
                {
                    Task.Run(() =>
                    {
                        _callback?.Invoke(_state);
                        baseTimer.Value.Change(_runInterval, 0);
                    });
                }
                
            }
        }

        /// <summary>
        /// 暂停定时器
        /// </summary>
        public void Stop()
        {
            lock (lockerObj)
            {
                isStop = true;
                if (baseTimer.IsValueCreated)
                {
                    baseTimer.Value.Change(Timeout.Infinite, Timeout.Infinite);
                }
                
            }
        }

        /// <summary>
        /// 返回定时器是否正在运行
        /// </summary>
        public bool IsRunning { get { return !isStop; } }

        /// <summary>
        /// 关闭定时器,相当于调用Dispose
        /// </summary>
        public void Close()
        {
            if (isDisponsed)
            {
                throw new Exception("该定时器已被关闭");
            }
            
            Dispose();
        }
        
        private void timerCallback(object state)
        {
            bool s;

            lock (lockerObj)
            {
                s = this.isStopWhenRun;
            }

            if (s)
            {
                baseTimer.Value.Change(Timeout.Infinite, Timeout.Infinite);
            }

            try
            {
                _callback(state);
            }
            catch (Exception)
            {
                
            }
            
            if (s && !isStop)
            {
                baseTimer.Value.Change(this._runInterval, 0);
            }
        }

        public void Dispose()
        {
            lock (lockerObj)
            {
                if (!isDisponsed)
                {
                    isStop = true;

                    if (baseTimer.IsValueCreated)
                    {
                        baseTimer.Value.Change(Timeout.Infinite, Timeout.Infinite);
                        baseTimer.Value.Dispose();                        
                    }

                    _callback = null;

                    isDisponsed = true;

                    GC.SuppressFinalize(this);
                }
            }

            }

        private Timer createTimer()
        {
            return new Timer(timerCallback, _state, Timeout.Infinite, Timeout.Infinite);
        }

        ~TimerEx()
        {
            Dispose();
        }
    }
}
