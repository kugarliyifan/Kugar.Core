using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration.Providers
{
    /// <summary>
    ///     读取.net自带的.config文件的ConnectionSection节的配置
    /// </summary>
    public class DotNetConfigConnectionSectionProvider : MarshalByRefObject, ILocalCustomConfigProvider
    { 
        string _path = "";

        System.Configuration.Configuration configManager = null;

        public DotNetConfigConnectionSectionProvider()
        {
            _path = null;

            try
            {
                configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                //ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                //map.ExeConfigFilename = "web.config";

                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                var configname = "";

#if NET45
      configname=AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif

#if NETCOREAPP2_0 || NETCOREAPP2_1
                configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config");

                if (!File.Exists(configname))
                {
                    configname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config");
                }
#endif

                map.ExeConfigFilename = configname;

                configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


            }
        }

        public DotNetConfigConnectionSectionProvider(string path)
        {
            _path = path;

            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = _path;

            configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


        }

        public IEnumerable<CustomConfigItem> Load()
        {
            List<CustomConfigItem> tempList = new List<CustomConfigItem>();

            if (configManager.ConnectionStrings != null)
            {
                var s = configManager.ConnectionStrings.ConnectionStrings;

                for (int i = 0; i < configManager.ConnectionStrings.ConnectionStrings.Count; i++)
                {
                    ConnectionStringSettings c = configManager.ConnectionStrings.ConnectionStrings[i];

                    tempList.Add(new CustomConfigItem(c.Name, ConfigItemDataType.String, c.ConnectionString));
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

                    var connSections = configManager.ConnectionStrings;

                    foreach (var config in configList)
                    {
                        int index=-1;

                        for (int i = 0; i < connSections.ConnectionStrings.Count; i++)
                        {
                            if(connSections.ConnectionStrings[i].Name==config.Name)
                            {
                                index=i;
                                break;
                            }
                        }

                        if (index==-1)
                        {
                            connSections.ConnectionStrings.Add(new ConnectionStringSettings(config.Name,config.Value.ToStringEx()));
                        }
                        else
                        {
                            connSections.ConnectionStrings[index].ConnectionString=config.Value.ToStringEx();
                        }

                        configManager.Save(ConfigurationSaveMode.Modified);
                    }
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