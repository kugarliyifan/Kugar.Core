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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.Configuration
{
    public interface ICustomConfigManager
    {
        string this[string configName] { get; }

        ICustomConfigSection AppSettings { get; }

        ICustomConfigSection ConnectionSettings { get; }

        ICustomConfigManager AddJsonFile(string jsonFileName, string path = "", bool optional = true,
            bool reloadOnChange = false);
    }

    /// <summary>
    ///     自定义配置的管理类
    /// </summary>
    public class CustomConfigManager : MarshalByRefObject, ICustomConfigManager
    {
        private ConfigurationBuilder _configBuilder = new ConfigurationBuilder();
        private static ICustomConfigSection _appSettings = null;
        private static ICustomConfigSection _connSettings = null;
        private IConfigurationRoot _configuration = null;

        private static Lazy<CustomConfigManager> _defauleManager = null;
        //private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        static CustomConfigManager()
        {


            _appSettings = Build("AppSettings", new DotNetConfigAppSettingsProvider());
            _connSettings = Build("ConnectionSection", new DotNetConfigConnectionSectionProvider());


            _defauleManager = new Lazy<CustomConfigManager>(() =>
            {
                var c = new CustomConfigManager();

                return c;
            });
        }

        public CustomConfigManager()
        {
            AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        }

        public string this[string configName]
        {
            get
            {
                if (string.IsNullOrEmpty(configName))
                {
                    throw new ArgumentOutOfRangeException("configName");
                }

                return _configuration[configName].ToStringEx();
            }
        }

        public TValue GetValue<TValue>(string configName, TValue defaultValue = default)
        {
            var type = typeof(TValue);

            if (type == typeof(string))
            {
                return (TValue)Convert.ChangeType(_configuration[configName], type);
            }
            else
            {  
                var section = getSection(configName);

                if (section == null)
                {
                    return defaultValue;
                }
                else
                {
                    return section.Get<TValue>();
                }
            }
        }

        public TValue[] GetArray<TValue>(string configName)
        {
            var section = getSection(configName);

            if (section==null)
            {
                return Array.Empty<TValue>();
            }
            else
            {
                return section.Get<TValue[]>();
            }
        }

        private IConfiguration getSection(string configName)
        { 
            var configNames = configName.Split(':');

            IConfiguration section = _configuration.GetSection(configNames[0]);

            if (section != null)
            {
                foreach (var name in configNames.Skip(1))
                {
                    section = section.GetSection(name);

                    if (section == null)
                    {
                        break;
                    }
                }
            }

            return section;
        }

        /// <summary>
        ///     返回默认当前.config中AppSettings节的配置值
        /// </summary>
        public ICustomConfigSection AppSettings => _appSettings;

        /// <summary>
        ///     返回默认当前.config中ConnectionSettings节的配置值
        /// </summary>
        public ICustomConfigSection ConnectionSettings => _connSettings;

        /// <summary>
        /// 添加一个json文件配置文件
        /// </summary>
        /// <param name="configName">配置项名称</param>
        /// <param name="path">配置项路径,如果是空,则为当前路径下</param>
        /// <returns></returns>
        public ICustomConfigManager AddJsonFile(string jsonFileName, string path = "", bool optional = true, bool reloadOnChange = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            _configBuilder.AddJsonFile(Path.Combine(path, jsonFileName), optional, reloadOnChange);

            _configuration = _configBuilder.Build();

            if (reloadOnChange)
            {
                ChangeToken.OnChange(() => _configuration.GetReloadToken(), () =>
                {
                    this.OnChanged?.Invoke(this, _configuration);
                });
            }

            return this;
        }

        public event EventHandler<IConfigurationRoot> OnChanged;


        /// <summary>
        ///     返回默认的配置节点,默认会自动添加.config文件中的AppSettings和ConnectionSettings节点
        /// </summary>
        public static CustomConfigManager Default => _defauleManager.Value;

        private static ICustomConfigSection Build(string sectionName, ICustomConfigProvider provider = null)
        {
            if (provider == null)
            {
                provider = new DotNetConfigNameValueProvider(sectionName);
            }

            ICustomConfigSection section = new CustomConfigSection(sectionName);

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

            return section;
        }
    }


}