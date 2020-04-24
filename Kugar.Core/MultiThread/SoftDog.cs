using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Kugar.Core.Exceptions;
using Kugar.Core.ExtMethod;
using Kugar.Core.BaseStruct;
using Kugar.Core.Remoting;
using System.Linq;

namespace Kugar.Core.MultiThread
{
	
    /// <summary>
    ///     软件看门狗,用于强制程序必须定时调用激活函数,否则视为超时,并且执行指定的超时操作<br/>
    ///     用于在软件锁死或崩溃时,由该看门狗检测到超时未激活,,则执行指定的操作,比如重启程序,或者强制关闭线程并重启计算之类.
    /// </summary>
    public static class SoftDog
    {
        private static Dictionary<string, SoftDogInterface> _cacheCurrentDogs = null;
        //缓存相关的AppDomain
        private static Dictionary<string,AppDomain> _cacheAppDomain=null;

        static SoftDog()
        {
            _cacheAppDomain=new Dictionary<string, AppDomain>();
            _cacheCurrentDogs=new Dictionary<string, SoftDogInterface>();
        }

        /// <summary>
        ///     根据指定名称从缓存记录中提取指定的看门狗
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static SoftDogInterface GetSoftDogByName(string name)
        {
            return _cacheCurrentDogs.TryGetValue(name);
        }

        public static  bool Contains(string name)
        {
            return _cacheCurrentDogs.ContainsKey(name);
        }

        public static void Remove(string name)
        {
            uninstallDog(name);
            _cacheCurrentDogs.Remove(name);
            _cacheAppDomain.Remove(name);
        }

        public static void Clean()
        {
            if (_cacheAppDomain==null || _cacheCurrentDogs==null)
            {
                return;
            }
            var nameList = _cacheCurrentDogs.Keys.ToArray();

            foreach (var name in nameList)
            {
                Remove(name);
            }
        }

        //卸载看门狗相关应用程序域
        private static void uninstallDog(string name)
        {
            var dogDomain = _cacheAppDomain.TryGetValue(name);

            if (dogDomain == null)
            {
                return;
            }

            var dog = _cacheCurrentDogs.TryGetValue(name);

            dog.Close();

            if (dogDomain!=AppDomain.CurrentDomain)
            {
                AppDomain.Unload(dogDomain);
            }
            
        }

        /// <summary>
        ///     在新建的AppDomain中新建一个看门狗,并在超时后,调用指定Type的指定MethodName函数
        /// </summary>
        /// <param name="targetDomain">指定的目标AppDomain</param>
        /// <param name="softDogName">自定义看门狗名称,不允许重复</param>
        /// <param name="timeOut">超时时间设定,不允许低于1秒</param>
        /// <param name="type">超时后将执行的函数所在类的Type</param>
        /// <param name="methodName">超时后将指定的函数名称</param>
        /// <param name="args">超时后将执行的函数的参数,所有参数值必须为可序列化</param>
        /// <returns></returns>
        public static SoftDogInterface CreateSoftDog(string softDogName, int timeOut, Type type, string methodName,
                                              params object[] args)
        {
            var appDomain = createNewDomain(ShortGuid.NewGuid().ToString());

            return  CreateSoftDog(appDomain, softDogName, timeOut, type, methodName, args);
        }

        /// <summary>
        ///     在指定的AppDomain中创建一个看门狗,并在超时后,调用指定Type的指定MethodName函数
        /// </summary>
        /// <param name="targetDomain">指定的目标AppDomain</param>
        /// <param name="softDogName">自定义看门狗名称,不允许重复</param>
        /// <param name="timeOut">超时时间设定,不允许低于1秒</param>
        /// <param name="type">超时后将执行的函数所在类的Type</param>
        /// <param name="methodName">超时后将指定的函数名称</param>
        /// <param name="args">超时后将执行的函数的参数,所有参数值必须为可序列化</param>
        /// <returns></returns>
        public static SoftDogInterface CreateSoftDog(AppDomain targetDomain, string softDogName, int timeOut, Type type, string methodName, object[] args)
        {
            if (timeOut<1000)
            {
                throw new ArgumentOutOfRangeException("timeOut","超时时间不能小于1秒");
            }

            if (string.IsNullOrWhiteSpace(softDogName))
            {
                throw new ArgumentNullException(softDogName);
            }

            if (_cacheCurrentDogs.ContainsKey(softDogName))
            {
                throw new ArgumentOutOfRangeException("softDogName");
            }

            if (type==null)
            {
                throw new ArgumentNullException("type");
            }

            if (args!=null && args.Length>0)
            {
                foreach (var o in args)
                {
                    if (!o.GetType().IsSerializable)
                    {
                        throw new ArgumentTypeNotMatchException("args", "Serializable");
                    }
                }
            }

            var assemblyName = typeof (SoftDog).Assembly.GetName().FullName;
            var typeName = typeof(SoftDogServorForTypeMethod).FullName;

            var targetAssemblyName = type.Assembly.GetName().FullName;
            var targetTypeName = type.FullName;

            var tempArgs = new object[]
                {
                    softDogName
                    ,timeOut
                    ,targetAssemblyName
                    ,targetTypeName
                    ,methodName
                    ,args
                };

            SoftDogInterface dog;

            try
            {
                dog = (SoftDogInterface)targetDomain.CreateInstanceAndUnwrap(assemblyName,
                                                 typeName
                                                 , false
                                                 , BindingFlags.Default
                                                 , null
                                                 , tempArgs
                                                 , Thread.CurrentThread.CurrentCulture
                                                 , null
                                                 , null);

                _cacheCurrentDogs.Add(softDogName,dog);
                _cacheAppDomain.Add(softDogName,targetDomain);
                return dog;
            }
            catch (Exception)
            {
                
                throw;
            }


        }

        /// <summary>
        ///     在新建的AppDomain中新建一个看门狗,并在超时后,调用指定的函数,该函数可以为当前域中的实例函数或Action对象
        /// </summary>
        /// <param name="softDogName">看门狗名称</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="action">超时后执行的函数</param>
        /// <returns></returns>
        public static SoftDogInterface CreateSoftDog(string softDogName, int timeOut, Action action)
        {

            var appDomain = createNewDomain(ShortGuid.NewGuid().ToString());

            return CreateSoftDog(appDomain, softDogName, timeOut, action);
        }

        /// <summary>
        ///     在指定域中新建一个看门狗,并在超时后,调用指定的函数,该函数可以为当前域中的实例函数或Action对象
        /// </summary>
        /// <param name="targetDomain">目标域</param>
        /// <param name="softDogName">看门狗名称</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="action">超时后执行的函数</param>
        /// <returns></returns>
        public static SoftDogInterface CreateSoftDog(AppDomain targetDomain, string softDogName, int timeOut, Action action)
        {
            

            if (timeOut<1000)
            {
                throw new ArgumentOutOfRangeException("timeOut","超时时间不能小于1秒");
            }

            if (string.IsNullOrWhiteSpace(softDogName))
            {
                throw new ArgumentNullException(softDogName);
            }

            if (_cacheCurrentDogs.ContainsKey(softDogName))
            {
                throw new ArgumentOutOfRangeException("softDogName");
            }


            var assemblyName = typeof (SoftDog).Assembly.GetName().FullName;
            var typeName = typeof(SoftDogServerForInstanceMethod).FullName;

            var call = new CallBackBlock<EventArgs>((sender, o) => action());

            var tempArgs = new object[]
                {
                    softDogName
                    ,timeOut
                    ,call
                };

            SoftDogInterface dog;

            try
            {
                dog = (SoftDogInterface)targetDomain.CreateInstanceAndUnwrap(assemblyName,
                                                 typeName
                                                 , false
                                                 , BindingFlags.Default
                                                 , null
                                                 , tempArgs
                                                 , Thread.CurrentThread.CurrentCulture
                                                 , null
                                                 , null);

                _cacheCurrentDogs.Add(softDogName,dog);

                return dog;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        //static ~SoftDog()
        //{
        //    Clean();
        //}

        //新建一个应用程序域
        private static AppDomain createNewDomain(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "SoftDogDomain_" + ShortGuid.NewGuid().ToString();
            }

            var setup = new AppDomainSetup();
            setup.ApplicationName = "ApplicationLoader";
            setup.LoaderOptimization = LoaderOptimization.MultiDomain;
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.CachePath = setup.ApplicationBase;
            //setup.ShadowCopyFiles = "true";
            //setup.ShadowCopyDirectories = setup.ApplicationBase;

            var appDomain = AppDomain.CreateDomain("SoftDogDomain_" + ShortGuid.NewGuid().ToString(), null, setup);

            return appDomain;   
        }
    }
    
    /// <summary>
    ///     看门狗接口
    /// </summary>
    public interface SoftDogInterface
    {
        
        string Name { get; }

        /// <summary>
        ///     开始启动计时
        /// </summary>
    	void Start();

        /// <summary>
        ///     停止计时器
        /// </summary>
    	void Stop();

        /// <summary>
        ///     激活看门狗
        /// </summary>
    	void Active();

        /// <summary>
        ///     关闭
        /// </summary>
    	void Close();

    }
    	
    
    public abstract class SoftDogServerBase:MarshalByRefObject, SoftDogInterface
    {
    	private int _currentTime = 0;
    	private int _maxTime=0;
        
		private TimerEx _timer=null;

        public SoftDogServerBase(string name, int timeOut)
		{
			_maxTime=timeOut;
			_timer=new TimerEx(callback,300,null);
			_timer.IsStopWhenRun=true;
            _name = name;
		}

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
        	_timer.Stop();
        }

		public void Active()
		{
			Interlocked.Exchange(ref _currentTime,0);
			
		}
    	
		public void Close()
		{
			_timer.Stop();
			_timer.Dispose();
		}
		
        private void callback(object state)
        {
        	var tempTime= Interlocked.Add(ref _currentTime,200);
        	
        	if (tempTime>_maxTime) {
        		Interlocked.Exchange(ref _currentTime,0);
        		TimeOutInvoke();
        	}
        }
		
		protected abstract void TimeOutInvoke();
    }

    /// <summary>
    ///     按类型及函数名称直接调用的看门狗,不建议直接调用该类,如需使用,请调用SoftDog类的相关函数
    /// </summary>
    public class SoftDogServorForTypeMethod : SoftDogServerBase
    {
    	private string _typeFullName;
    	private string _methodName;
    	private string _assemblyFullName;
    	private object[] _paramList=null;
    	private object _instance=null;

        public SoftDogServorForTypeMethod(string name, int timeout, string assemblyFullName, string typeFullName, string methodName, params object[] paramList)
            : base(name, timeout)
    	{
    		if (string.IsNullOrWhiteSpace(assemblyFullName)) {
    			throw new ArgumentNullException("assemblyFullName");
    		}	
    		
    		if (string.IsNullOrWhiteSpace("typeFullName")) {
    			throw new ArgumentNullException("typeFullName");
    		}
    		
    		if (string.IsNullOrWhiteSpace("methodName")) {
    			throw new ArgumentNullException("methodName");
    		}
    		_paramList=paramList;
    		
    		var assembly=AppDomain.CurrentDomain.Load(_assemblyFullName);
    		var type=assembly.GetType(_typeFullName);
    		
    		_instance=Activator.CreateInstance(type);

    		_methodName=methodName;

    	}
    	
		    	
		protected override void TimeOutInvoke()
		{
			try {
				_instance.FastInvoke(_methodName,_paramList);
			} catch (Exception) {
				
			}
			
		}
    }
    
    /// <summary>
    ///     调用远程函数的看门狗,不建议直接调用该类,如需使用,请调用SoftDog类的相关函数
    /// </summary>
    public class SoftDogServerForInstanceMethod:SoftDogServerBase
    {
        private CallBackBlock<EventArgs> _func = null;

        public SoftDogServerForInstanceMethod(string name, int timeout, CallbackTimer.CallbackBlock<EventArgs> func)
            : base(name, timeout)
    	{
            _func = func;
    	}
    	
		protected override void TimeOutInvoke()
		{
			try {
				_func.OnCallBack(this,EventArgs.Empty);
			} catch (Exception) {
				
			}
		}
    }
    	
}
