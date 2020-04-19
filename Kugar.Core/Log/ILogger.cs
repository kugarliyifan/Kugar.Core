using System;
using System.Collections.Generic;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Log
{
    public interface ILogger
    {

        #region "Debug"

        void Debug(string message, KeyValuePair<string, object>[] extData = null);

        #endregion

        #region “Trace”

        void Trace(string message, KeyValuePair<string, object>[] extData = null);

        #endregion

        #region “Warn”

        void Warn(string message, Exception error = null, KeyValuePair<string, object>[] extData = null);

        #endregion

        #region “Error”

        void Error(Exception error, string message = "", KeyValuePair<string, object>[] extData = null);

        void Error(string message, Exception error = null, KeyValuePair<string, object>[] extData = null);

        #endregion

        /// <summary>
        ///     是否输出Debug类型的消息
        /// </summary>
        bool IsDebugEnable { set; get; }

        bool IsTraceEnable { set; get; }

        bool IsWarnEnable { set; get; }

        bool IsErrorEnable { set; get; }
    }

    public abstract class LoggerBase: ILogger
    {
        private bool _isTraceEnable = true;

        private bool _isWarnEnable = true;

        private bool _isErrorEnable = true;

        private bool _isDebugEnable = true;

        public LoggerBase()
        {
            var loggerLevel = CustomConfigManager.Default["Logging:LogLevel:KugarLogger"].IfEmptyOrWhileSpace("Trace").ToLower();

            switch (loggerLevel)
            {
                case "trace":
                {
                    IsTraceEnable = true;
                    IsWarnEnable = true;
                    IsDebugEnable = true;
                    IsErrorEnable = true;
                    break;
                }
                case "debug":
                {
                    IsTraceEnable = false;
                    IsDebugEnable = true;
                    IsWarnEnable = true;
                    IsErrorEnable = true;
                    break;
                }
                case "warn":
                {
                    IsTraceEnable = true;
                    IsWarnEnable = true;
                    IsDebugEnable = true;
                    IsErrorEnable = true;
                    break;
                }
                case "error":
                {
                    IsTraceEnable = false;
                    IsWarnEnable = false;
                    IsDebugEnable = false;
                    IsErrorEnable = true;
                    break;
                }
                default:
                {
                    IsTraceEnable = true;
                    IsWarnEnable = true;
                    IsDebugEnable = true;
                    IsErrorEnable = true;
                    break;
                }
            }
        }

        public void Debug(string message, KeyValuePair<string, object>[] extData = null)
        {
            if (_isDebugEnable)
            {
                DebugInternal(message, extData);
            }
        }

        protected abstract void DebugInternal(string message, KeyValuePair<string, object>[] extData = null);
        

        public void Trace(string message, KeyValuePair<string, object>[] extData = null)
        {
            if (_isTraceEnable)
            {
                TraceInternal(message, extData);
            }
        }

        protected abstract void TraceInternal(string message,KeyValuePair<string, object>[] extData = null);


        public void Warn(string message,Exception error=null, KeyValuePair<string, object>[] extData=null)
        {
            if (_isWarnEnable)
            {
                WarnInternal(message,error, extData);
            }
        }
        
        protected abstract void WarnInternal(string message,Exception error, KeyValuePair<string, object>[] extData);
        
        [Obsolete]
        public void Error(Exception error, string message="",KeyValuePair<string,object>[] extData=null)
        {
            if (_isErrorEnable)
            {
                ErrorInternal(message, error, extData);
            }
        }

        public void Error(string message, Exception error=null, KeyValuePair<string, object>[] extData=null)
        {
            if (_isErrorEnable)
            {
                ErrorInternal( message,error, extData);
            }
        }


        //public void Error(Exception error, string message, IFormatProvider formatProvider, params object[] args)
        //{
        //    if (_isErrorEnable)
        //    {
        //        ErrorInternal(error,message, formatProvider, args);
        //    }
        //}

        protected abstract void ErrorInternal(string message, Exception error, KeyValuePair<string, object>[] extData);


        public bool IsDebugEnable
        {
            get { return _isDebugEnable; }
            set { _isDebugEnable = value; }
        }

        public bool IsTraceEnable
        {
            get { return _isTraceEnable; }
            set { _isTraceEnable = value; }
        }

        public bool IsWarnEnable
        {
            get { return _isWarnEnable; }
            set { _isWarnEnable = value; }
        }

        public bool IsErrorEnable
        {
            get { return _isErrorEnable; }
            set { _isErrorEnable = value; }
        }
    }
    
}