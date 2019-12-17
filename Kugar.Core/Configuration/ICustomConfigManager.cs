using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Kugar.Core.Configuration.Providers;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration
{
    public interface ICustomConfigManager : IEnumerable<KeyValuePair<string, ICustomConfigSection>>
    {
        ICustomConfigSection this[string name] { get; }

        /// <summary>
        ///     使用Provider支持器添加一个配置节,如出现重复名称
        /// </summary>
        /// <param name="sectionName">指定配置节的名称</param>
        /// <param name="provider">配置节的支持器</param>
        void Add(string sectionName, ICustomConfigProvider provider);

        /// <summary>
        ///     不使用支持器,直接添加一个配置节点
        /// </summary>
        /// <param name="section"></param>
        void Add(ICustomConfigSection section);

        /// <summary>
        ///     返回当前配置节点的个数
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     移出指定名称的节点
        /// </summary>
        /// <param name="sectionName">指定配置节名称</param>
        void Remove(string sectionName);
        void Clear();
        bool Save(string sectionName);
        void SaveAll();

        /// <summary>
        /// 是否包含指定名称的配置节
        /// </summary>
        /// <param name="sectionName">配置节名称</param>
        /// <returns></returns>
        bool ContainsSection(string sectionName);
        /// <summary>
        /// 设置指定名称配置节的支持器
        /// </summary>
        /// <param name="sectionName">指定配置节的名称</param>
        /// <param name="provider">新的支持器</param>
        void SetProvider(string sectionName, ICustomConfigProvider provider);

        ICustomConfigProvider GetProvider(string sectionName);
    }

    /// <summary>
    ///     自定义配置的管理类
    /// </summary>
    public class CustomConfigManager : MarshalByRefObject, ICustomConfigManager
    {
        private static Lazy<CustomConfigManager> _defauleManager = null;
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        static CustomConfigManager()
        {
            _defauleManager = new Lazy<CustomConfigManager>(() =>
            {
                var c = new CustomConfigManager();

                c.Add("AppSettings", new DotNetConfigAppSettingsProvider());
                c.Add("ConnectionStrings", new DotNetConfigConnectionSectionProvider());
                c.Add("KugarCoreConfig", new DotNetConfigKugarCoreConfigProvider());
                return c;
            });
        }

        private Dictionary<string, ICustomConfigSection> _cache = null;
        private Dictionary<string, ICustomConfigProvider> _cacheProvider = null;

        public CustomConfigManager()
        {
            _cache = new Dictionary<string, ICustomConfigSection>();
            _cacheProvider = new Dictionary<string, ICustomConfigProvider>();
        }

        public ICustomConfigSection this[string sectionName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    throw new ArgumentOutOfRangeException("sectionName");
                }



                return _cache.TryGetValue(sectionName);
            }
        }

        public ICustomConfigSection CurrentDLLAppSettings
        {
            get
            {
                ICustomConfigSection section = null;

                _locker.EnterUpgradeableReadLock();

                try
                {
                    var assembly = Assembly.GetCallingAssembly();

                    if (assembly.IsDynamic)
                    {
                        throw new Exception("动态生成的类,无法获取当前配置文件");
                    }

                    var filePath = assembly.Location;
                
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                
                    if(!_cache.TryGetValue(fileName,out section))
                    {
                        _locker.EnterWriteLock();

                        if (_cache.ContainsKey(fileName))
                        {
                            return _cache.TryGetValue(fileName);
                        }

                        var provider=new DotNetConfigAppSettingsProvider(assembly);

                        Add(fileName, provider);

                        section= _cache[fileName];

                        _locker.ExitWriteLock();
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _locker.ExitUpgradeableReadLock();
                }

                return section;                
            }


        }

        /// <summary>
        ///     使用Provider支持器添加一个配置节,如出现重复名称
        /// </summary>
        /// <param name="sectionName">指定配置节的名称</param>
        /// <param name="provider">配置节的支持器</param>
        public void Add(string sectionName, ICustomConfigProvider provider=null)
        {
            if (provider==null)
            {
                provider=new DotNetConfigNameValueProvider(sectionName);
            }

            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentNullException("sectionName");
            }

            if (_cache.ContainsKey(sectionName))
            {
                throw new ArgumentOutOfRangeException("sectionName", "不允许出现相同名称的配置节");
            }

            ICustomConfigSection section = new CustomConfigSection(sectionName);

            //if (provider is ILocalCustomConfigProvider)
            //{
            //    section = new LocalCustomSection(sectionName);
            //}
            //else
            //{
            //    section = new CustomConfigSection(sectionName);
            //}



            IEnumerable<CustomConfigItem> configList = null;

            try
            {
                configList = provider.Load();
            }
            catch (Exception ex)
            {
                //Kugar.Core.Log.LoggerManager.GetLogger().Error(string.Format("加载节点 {0} 的配置提供器 时出现错误{1}", sectionName,ex.Message));
            }

            if (configList != null)
            {
                foreach (var config in configList)
                {
                    section[config.Name] = config;
                }
            }

            _cache.Add(sectionName, section);

            _cacheProvider.Add(sectionName, provider);

        }

        /// <summary>
        ///     不使用支持器,直接添加一个配置节点
        /// </summary>
        /// <param name="section"></param>
        public void Add(ICustomConfigSection section)
        {
            if (_cache.ContainsKey(section.Name))
            {
                throw new ArgumentOutOfRangeException("section", "不允许出现相同名称的配置节");
            }

            _cache.Add(section.Name, section);
        }

        /// <summary>
        ///     返回当前配置节点的个数
        /// </summary>
        public int Count { get { return _cache.Count; } }

        /// <summary>
        ///     是否包含指定名称的配置节
        /// </summary>
        /// <param name="sectionName">配置节名称</param>
        /// <returns></returns>
        public bool ContainsSection(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentOutOfRangeException("sectionName");
            }

            return _cache.ContainsKey(sectionName);
        }

        /// <summary>
        ///     移出指定名称的节点
        /// </summary>
        /// <param name="sectionName">指定配置节名称</param>
        public void Remove(string sectionName)
        {
            _cache.Remove(sectionName);
            _cacheProvider.Remove(sectionName);
        }

        /// <summary>
        ///     设置指定名称配置节的支持器
        /// </summary>
        /// <param name="sectionName">指定配置节的名称</param>
        /// <param name="provider">新的支持器</param>
        public void SetProvider(string sectionName, ICustomConfigProvider provider)
        {
            _cacheProvider[sectionName] = provider;
        }

        public ICustomConfigProvider GetProvider(string sectionName)
        {
            return _cacheProvider.TryGetValue(sectionName);
        }

        /// <summary>
        ///     返回默认当前.config中AppSettings节的配置值
        /// </summary>
        public ICustomConfigSection AppSettings
        {
            get
            {
                var item = _cache.TryGetValue("AppSettings");

                return item;
            }
        }

        /// <summary>
        ///     返回默认当前.config中ConnectionSettings节的配置值
        /// </summary>
        public ICustomConfigSection ConnectionSettings
        {
            get
            {
                var item = _cache.TryGetValue("ConnectionStrings");

                return item;

                //return this["ConnectionStrings"];
            }
        }

        public ICustomConfigSection KugarCoreConfig
        {
            get
            {

                var item = _cache.TryGetValue("KugarConfig");

                return item;
            }
        }

        public void Clear()
        {
            _cache.Clear();
            _cacheProvider.Clear();
        }

        public bool Save(string sectionName)
        {
            var configSection = _cache.TryGetValue(sectionName);

            if (configSection == null)
            {
                throw new ArgumentOutOfRangeException("sectionName");
            }

            var provider = _cacheProvider.TryGetValue(sectionName);

            if (provider == null)
            {
                throw new ArgumentException("指定配置段不存在支持器,无法保存配置");
            }

            if (configSection != null && provider != null)
            {
                return provider.Write(configSection.Where(x => x.Value.Status == CustomConfigItemStatus.Modify).Select(x => x.Value).ToArray());
            }
            else
            {
                return false;
            }

        }

        public void SaveAll()
        {
            foreach (var providerItem in _cacheProvider)
            {
                var config = _cache.TryGetValue(providerItem.Key);

                if (config != null)
                {
                    providerItem.Value.Write(config.Where(x => x.Value.Status == CustomConfigItemStatus.Modify).Select(x => x.Value).ToArray());
                }
            }
        }

        /// <summary>
        ///     返回默认的配置节点,默认会自动添加.config文件中的AppSettings和ConnectionSettings节点
        /// </summary>
        public static CustomConfigManager Default { get { return _defauleManager.Value; } }

        public IEnumerator<KeyValuePair<string, ICustomConfigSection>> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}