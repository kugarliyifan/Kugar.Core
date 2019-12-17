using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration.Providers
{
    public class DotNetConfigKugarCoreConfigProvider : MarshalByRefObject, ILocalCustomConfigProvider
    {
        string _path = "";

        System.Configuration.Configuration configManager = null;

        public DotNetConfigKugarCoreConfigProvider()
        {
            _path = null;

            try
            {
                configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = "web.config";

                configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


            }
        }

        public DotNetConfigKugarCoreConfigProvider(string path)
        {
            _path = path;


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


            if (configManager.Sections["KugarConfig"] != null)
            {
                foreach (KeyValueConfigurationElement c in ((AppSettingsSection)configManager.Sections["KugarConfig"]).Settings)
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

                    var kugarSections = (AppSettingsSection)configManager.Sections["KugarConfig"];

                    foreach (var config in configList)
                    {
                        if (allkeys.Contains(config.Name))
                        {
                            kugarSections.Settings[config.Name].Value = config.Value.ToStringEx();
                        }
                        else
                        {
                            kugarSections.Settings.Add(config.Name, config.Value.ToStringEx());
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
