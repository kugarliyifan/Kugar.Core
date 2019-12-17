using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kugar.Core
{
    public class MutileThreadingTimer:IDisposable
    {
        private Timer timer = null;
        private Stopwatch sw=new Stopwatch();
        private bool isDisposed = false;
        private long _timerUsed = 0;
        private bool _isPaused = false;
        private object lockerObj=new object();
        private long _interval = 0;

        public MutileThreadingTimer()
        {
            timer = new Timer(Timer_CallBack);
        }

        private void Timer_CallBack(object state)
        {
            sw.Reset();

            CallBack(state);

            sw.Reset();
            sw.Start();
        }

        public void Pause ()
        {
            lock (lockerObj)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);

                sw.Stop();

                _timerUsed = sw.ElapsedMilliseconds;
                _isPaused = true;                
            }


        }

        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            lock (lockerObj)
            {
                var nextInterval = _interval - _timerUsed;

                timer.Change(nextInterval, 0);

                sw.Start();

                _isPaused = false;
                _timerUsed = 0;                
            }


        }

        public void Start()
        {
            lock (lockerObj)
            {
                _timerUsed = 0L;
                _isPaused = false;

                sw.Reset();
                sw.Start();

                timer.Change(_interval, 0);


            }
        }

        public void Stop()
        {
            lock (lockerObj)
            {
                _timerUsed = 0L;
                _isPaused = false;

                sw.Stop();
                sw.Reset();

                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

        }

        public long Interval
        {
            set
            {
                if (value<=0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _interval = value;
            }
            get { return _interval; }

        }

        public TimerCallback CallBack { set; get; }
        

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            if (timer != null) timer.Dispose();

            sw.Stop();

            isDisposed = true;

        }
    }
}
