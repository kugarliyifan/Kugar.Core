using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration.Providers
{
    /// <summary>
    ///     读取.net自带的.config文件的AppSection节的配置
    /// </summary>
    public class DotNetConfigAppSettingsProvider :MarshalByRefObject, ILocalCustomConfigProvider
    {
        string _path = "";

        System.Configuration.Configuration configManager = null;

        public DotNetConfigAppSettingsProvider()
        {
            _path = null;

            Debugger.Break();

            try
            {
#if NETCOREAPP
                var configname = "";
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                //configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config");

                if (!File.Exists(configname))
                {
                    configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
                }

                if (!File.Exists(configname))
                {
                    configname = Path.Combine(Assembly.GetEntryAssembly().Location , $"{Assembly.GetEntryAssembly().GetName().Name}.dll.config");

                    Console.WriteLine(configname);
                }

                map.ExeConfigFilename = configname;

                configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

#endif
#if NET45
                configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
#endif

                //if (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.Request.PhysicalPath.Equals(string.Empty))
                //    configManager = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                //else
                //    configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


            }
            catch (Exception)
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();

                var configname = "";

#if NET45
      configname=AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif

#if NETCOREAPP 
                //configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config");

                if (!File.Exists(configname))
                {
                    configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
                }

                if (!File.Exists(configname))
                {
                    configname = $"{Assembly.GetExecutingAssembly().GetName().Name}.dll.config";
                }
  #endif

                map.ExeConfigFilename = configname; 

                configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


            }
            
        }

        public DotNetConfigAppSettingsProvider(string path)
        {
            _path = path;


            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = _path;

            configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


        }

        /// <summary>
        /// 使用传入的Type所在{Assembly}.config
        /// </summary>
        /// <param name="type"></param>
        public DotNetConfigAppSettingsProvider(Type type)
        {
            _path = type.Assembly.Location;


            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = _path;

            configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

        }

                /// <summary>
        /// 使用传入的Type所在{Assembly}.config
        /// </summary>
        /// <param name="type"></param>
        public DotNetConfigAppSettingsProvider(Assembly assembly)
        {
            _path = assembly.Location;


            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = _path;

            configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

        }

        public IEnumerable<CustomConfigItem> Load()
        {
            List<CustomConfigItem> tempList = new List<CustomConfigItem>();



            //ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            //map.ExeConfigFilename = _path;

            //var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


            if (configManager?.AppSettings != null)
            {
                foreach (KeyValueConfigurationElement c in configManager.AppSettings.Settings)
                {
                    tempList.Add(new CustomConfigItem(c.Key, ConfigItemDataType.String, c.Value));
                }
            }

            return tempList;
        }

        public bool Write(IEnumerable<CustomConfigItem> configList)
        {
            if (configList != null)
            {
                try
                {
                    //ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                    //map.ExeConfigFilename = _path;

                    //var configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                    var allkeys = configManager.AppSettings.Settings.AllKeys;

                    var appSections = configManager.AppSettings;

                    foreach (var config in configList)
                    {
                        if (allkeys.Contains(config.Name))
                        {
                            appSections.Settings[config.Name].Value = config.Value.ToStringEx();
                        }
                        else
                        {
                            appSections.Settings.Add(config.Name, config.Value.ToStringEx());
                        }
                    }

                    configManager.Save(ConfigurationSaveMode.Modified);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}