using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Kugar.Core.ExtMethod;
using System.Linq;

namespace Kugar.Core.Log
{
    /// <summary>
    ///     用于管理日志记录器的类，可根据配置文件中指定的设置启用不同的日志记录器<br/>
    ///     现在支持NLog<br/>
    ///     具体方式，见Readme.txt文件
    /// </summary>
    public static class LoggerManager
    {
        private static Logger_Empty _defaultLogger=new Logger_Empty();
        ///private static object _lockObj=new object();

        static LoggerManager()
        {
            IsThrowException = false;

            //var t = GetLoggerFactoryFromConfig();

            //if (t!=null)
            //{
            //    try
            //    {
            //        var temp = (ILoggerFactory)Activator.CreateInstance(t);

            //        LoggerFactory = temp;
            //    }
            //    catch (Exception)
            //    {
                    
            //    }
            //}
        }
        
        public static ILoggerFactory LoggerFactory { set; get; }

        private static ILogger _coreLogger;

        ///// <summary>
        /////     获取Kugar.Core系列工程默认的日志记录器
        ///// </summary>
        //public static ILogger CoreLogger => Default;

        /// <summary>
        ///     获取默认的日志记录器
        /// </summary>
        /// <returns></returns>
        public static ILogger GetLogger()
        {
            return GetLogger("KugarCore");
        }

        /// <summary>
        ///     获取指定名称的日志记录器
        /// </summary>
        /// <param name="loggerName">指定的名称</param>
        /// <returns></returns>
        public static ILogger GetLogger(string loggerName)
        {
            if (LoggerFactory==null)
            {
                return _defaultLogger;
            }
            else
            {
                try
                {
                    var logger=LoggerFactory.GetLogger(loggerName);
                    if (logger==null)
                    {
                        if (IsThrowException)
                        {
                            throw new NullReferenceException("无法获取指定的记录器，指定生成器返回空");
                        }
                        else
                        {
                            return _defaultLogger;
                        }
                    }
                    else
                    {
                        return logger;
                    }
                }
                catch (Exception)
                {
                    if (IsThrowException)
                    {
                        throw new NullReferenceException("无法获取指定的记录器，指定生成器返回空");
                    }
                    else
                    {
                        return _defaultLogger;
                    }
                }




            }
        }

        /// <summary>
        /// 默认记录器
        /// </summary>
        public static ILogger Default
        {
            get
            {
                if (LoggerFactory!=null)
                {
                    return LoggerFactory.Default;
                }
                else
                {
                    return _defaultLogger;
                }
            }
        }

        /// <summary>
        /// 创建一个日志构建器
        /// </summary>
        /// <returns></returns>
        public static LogBuilder CreateBuilder()
        {
            return new LogBuilder(Default);
        }

        ///// <summary>
        /////     从配置文件中读取指定的日志类生成器
        ///// </summary>
        ///// <returns></returns>
        //public static Type GetLoggerFactoryFromConfig()
        //{
        //    var t = CustomConfigManager.Default.KugarCoreConfig;

        //    //读取日志记录器生成类的指定Dll文件路径，如果不存在，则返回null
        //    var factoryFilePath = t.GetValueByName<string>("LoggerFactoryFilePath");

        //    //读取是否有指定的日志记录器生成类的类名（需要完整类名），如果有，这读取指定的类，如果没有，则使用搜索到的第一个ILoggerFactory接口的类
        //    var typeName = t.GetValueByName<string>("LoggerFactoryTypeName");

        //    if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(factoryFilePath))
        //    {
        //        var appConfig = CustomConfigManager.Default.AppSettings;

        //        factoryFilePath = appConfig.GetValueByName<string>("LoggerFactoryFilePath");
        //        typeName = appConfig.GetValueByName<string>("LoggerFactoryTypeName");

        //    }

        //    if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(factoryFilePath))
        //    {
        //        return null;
        //    }


        //    Assembly factoryAssembly = null;

        //    try
        //    {
        //        factoryAssembly = Assembly.LoadFrom(factoryFilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (IsThrowException)
        //        {
        //            throw ex;
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }


        //    Type factoryType = null;

        //    if (!typeName.IsNullOrEmpty())
        //    {
        //        factoryType = factoryAssembly.GetType(typeName, false, false);

        //        if (factoryType!=null )
        //        {
        //            var interfaceList = factoryType.GetInterfaces();

        //            if (!interfaceList.Contains(typeof(ILoggerFactory)))
        //            {
        //                factoryType = null;
        //            }                    
        //        }
        //    }

        //    if (factoryType==null)
        //    {
        //        var typeList=factoryAssembly.GetTypes();

        //        foreach (var type in typeList)
        //        {

        //            var interfaceList = type.GetInterfaces();

        //            if (!interfaceList.Contains(typeof(ILoggerFactory)))
        //            {
        //                factoryType = type;
        //                break;
        //            }
        //        }
        //    }

        //    return factoryType;
        //}

        /// <summary>
        ///     当出现错误时，是否抛出错误，建议调试时设置为true，发布时设置为false
        /// </summary>
        public static bool IsThrowException { set; get; }
    }

    /// <summary>
    ///     空的日志记录类，用于防止在没有指定配置LoggerFactory的情况下返回空的日志记录器，引发调用出错
    /// </summary>
    public class Logger_Empty : LoggerBase
    {
        public Logger_Empty()
        {
            
        }

        //protected override void DebugInternal(string message, IFormatProvider formatProvider, params object[] args)
        //{
        //    System.Diagnostics.Debug.WriteLine(string.Format(formatProvider,message,args));
        //}

        //protected override void TraceInternal(string message, IFormatProvider formatProvider, params object[] args)
        //{
        //    System.Diagnostics.Debug.WriteLine(string.Format(formatProvider, message, args));
        //}

        //protected override void WarnInternal(string message, IFormatProvider formatProvider, params object[] args)
        //{
        //    System.Diagnostics.Debug.WriteLine(string.Format(formatProvider, message, args));
        //}

        //protected override void ErrorInternal(Exception error, string message, IFormatProvider formatProvider, params object[] args)
        //{
        //    System.Diagnostics.Debug.WriteLine(string.Format(formatProvider, message, args));
        //}

        protected override void DebugInternal(string message, KeyValuePair<string, object>[] extData = null)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format(formatProvider, message, args));
        }

        protected override void TraceInternal(string message, KeyValuePair<string, object>[] extData = null)
        {
            //throw new NotImplementedException();
        }

        protected override void WarnInternal(string message, Exception error, KeyValuePair<string, object>[] extData)
        {
            //throw new NotImplementedException();
        }

        protected override void ErrorInternal(string message, Exception error, KeyValuePair<string, object>[] extData)
        {
            //throw new NotImplementedException();
        }
    }

    public class LogBuilder
    {
        private string _msg = "";
        private Lazy<List<KeyValuePair<string,object>>> _propList=new Lazy<List<KeyValuePair<string, object>>>();
        private LogLevel _type =0;
        private Exception _error = null;
        private ILogger _logger = null;

        internal LogBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public LogBuilder SetMessage(string msg)
        {
            _msg = msg;

            return this;
        }

        public LogBuilder SetProperty(string name, object data)
        {
            _propList.Value.Add(new KeyValuePair<string, object>(name,data));

            return this;
        }

        public LogBuilder SetType(LogLevel type)
        {
            _type = type;

            return this;
        }

        public LogBuilder SetException(Exception error)
        {
            _error = error;

            _type = LogLevel.Error;

            return this;
        }
        
        public void Submit()
        {
            switch (_type)
            {
                case LogLevel.Trace:
                    _logger.Trace(_msg,_propList.IsValueCreated?_propList.Value.ToArrayEx():null);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(_msg, _propList.IsValueCreated ? _propList.Value.ToArrayEx() : null);
                    break;
                case LogLevel.Warn:
                    _logger.Warn(_msg,_error, _propList.IsValueCreated ? _propList.Value.ToArrayEx() : null);
                    break;
                case LogLevel.Error:
                    _logger.Error(_msg, _error, _propList.IsValueCreated ? _propList.Value.ToArrayEx() : null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum LogLevel
    {
        Trace,

        Debug,

        Warn,

        Error
    }

    ///// <summary>
    /////     空的日志记录类，用于防止在没有指定配置LoggerFactory的情况下返回空的日志记录器，引发调用出错
    ///// </summary>
    //public class Logger_Empty:ILogger
    //{
    //    private bool _isDebugEnable;

    //    private bool _isTraceEnable;

    //    private bool _isWarnEnable;

    //    private bool _isErrorEnable;

    //    public void Debug(string message)
    //    {
            
    //    }

    //    public void Debug(string message, params object[] args)
    //    {
            
    //    }

    //    public void Debug(string message, IFormatProvider formatProvider, params object[] args)
    //    {
            
    //    }

    //    public void Trace(string message)
    //    {
            
    //    }

    //    public void Trace(string message, params object[] args)
    //    {
            
    //    }

    //    public void Trace(string message, IFormatProvider formatProvider, params object[] args)
    //    {
            
    //    }

    //    public void Warn(string message)
    //    {
            
    //    }

    //    public void Warn(string message, params object[] args)
    //    {
            
    //    }

    //    public void Warn(string message, IFormatProvider formatProvider, params object[] args)
    //    {
            
    //    }

    //    public void Error(string message)
    //    {
            
    //    }

    //    public void Error(string message, params object[] args)
    //    {
            
    //    }

    //    public void Error(string message, IFormatProvider formatProvider, params object[] args)
    //    {
            
    //    }

    //    public bool IsDebugEnable
    //    {
    //        get { return _isDebugEnable; }
    //        set { _isDebugEnable = value; }
    //    }

    //    public bool IsTraceEnable
    //    {
    //        get { return _isTraceEnable; }
    //        set { _isTraceEnable = value; }
    //    }

    //    public bool IsWarnEnable
    //    {
    //        get { return _isWarnEnable; }
    //        set { _isWarnEnable = value; }
    //    }

    //    public bool IsErrorEnable
    //    {
    //        get { return _isErrorEnable; }
    //        set { _isErrorEnable = value; }
    //    }
    //}

    
}
