using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.Configuration.Providers
{
    public class DotNetConfigNameValueProvider:ICustomConfigProvider
    {
        string _path = "";
        private string _sectionName = "";

        System.Configuration.Configuration configManager = null;

        public DotNetConfigNameValueProvider(string sectionName)
        {
            _path = null;

            

            try
            {
                //if (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.Request.PhysicalPath.Equals(string.Empty))
                //    configManager = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                //else
                //    configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                var configname = "";

#if NET45
      configname=AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif

#if NETCOREAPP2_0
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

        public DotNetConfigNameValueProvider(string sectionName,string path)
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
        public DotNetConfigNameValueProvider(string sectionName,Type type)
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
        public DotNetConfigNameValueProvider(string sectionName,Assembly assembly)
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

            var section=configManager.GetSection(_sectionName) as NameValueCollectionConfiguration;

            if (section!=null)
            {
                foreach (var c in section.ToDictionary())
                {
                    tempList.Add(new CustomConfigItem(c.Key, ConfigItemDataType.String, c.Value));
                }
            }
            
            return tempList;
        }

        public bool Write(IEnumerable<CustomConfigItem> configList)
        {
            throw new NotImplementedException();
        }
    }
}
