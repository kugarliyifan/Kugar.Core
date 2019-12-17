using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration
{
    public interface ICustomConfigSection : IEnumerable<KeyValuePair<string, CustomConfigItem>>
    {
        string Name { get; }
        bool ContainsKey(string configName);
        CustomConfigItem this[string configName] { get; set; }
        T GetValueByName<T>(string configName, T defaultValue=default(T));
        string GetValueByName(string configName, string defaultValue = "");
        IEnumerable<CustomConfigItem> GetModifyConfig();
    }


    /// <summary>
    ///     自定义的配置节,允许全部序列化
    /// </summary>
    [Serializable]
    public class CustomConfigSection : ICustomConfigSection
    {
        private Dictionary<string, CustomConfigItem> _cache =
            new Dictionary<string, CustomConfigItem>(StringComparer.CurrentCultureIgnoreCase);


        public CustomConfigSection(string sectionName)
        {

            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentNullException("sectionName");
            }

            Name = sectionName;
        }

        public string Name { get; private set; }

        public CustomConfigItem this[string configName]
        {
            get
            {
                var item = _cache.TryGetValue(configName);

                if (item == null)
                {
                    throw new ArgumentOutOfRangeException("configName");
                }

                return item;
            }
            set
            {
                var oldValue = _cache.TryGetValue(configName);

                if (!oldValue.SafeEquals(value))
                {
                    _cache[configName] = value;
                    value.PropertyChanged += config_PropertyChanged;

                    if (oldValue!=null)
                    {
                        oldValue.PropertyChanged -= config_PropertyChanged;
                    }
                }

            }
        }

        void config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public T GetValueByName<T>(string configName, T defaultValue = default(T))
        {
            if (ContainsKey(configName))
            {
                T value;

                try
                {
                    value = (T)_cache[configName].Value;
                    
                }
                catch (Exception)
                {
                    value = defaultValue;
                   
                }

                return (T)value;

            }
            else
            {
                return defaultValue;
            }
        }

        public string GetValueByName(string configName, string defaultValue = "")
        {
            if (ContainsKey(configName))
            {
                return _cache[configName].Value.ToStringEx();
            }
            else
            {
                return defaultValue;
            }
        }

        public IEnumerable<CustomConfigItem> GetModifyConfig()
        {
            return _cache.Where(x => x.Value.Status == CustomConfigItemStatus.Modify).Select(x => x.Value);
        }

        #region IEnumerable<KeyValuePair<string,object>> 成员

        public IEnumerator<KeyValuePair<string, CustomConfigItem>> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICustomConfigSection 成员


        public bool ContainsKey(string configName)
        {
            return _cache.ContainsKey(configName);
        }

        #endregion
    }

    ///// <summary>
    /////     本地的配置节,配置节中的数据不被序列化
    ///// </summary>
    //[Serializable]
    //public class LocalCustomSection : MarshalByRefObject, ICustomConfigSection
    //{
    //    [NonSerialized] private Dictionary<string, CustomConfigItem> _cache =
    //        new Dictionary<string, CustomConfigItem>(StringComparer.CurrentCultureIgnoreCase);


    //    public LocalCustomSection(string sectionName)
    //    {
    //        Name = sectionName;
    //    }

    //    #region ICustomConfigSection 成员

    //    public string Name { get; private set; }

    //    public CustomConfigItem this[string configName]
    //    {
    //        get { throw new NotImplementedException(); }
    //        set { throw new NotImplementedException(); }
    //    }

    //    public IEnumerable<CustomConfigItem> GetModifyConfig()
    //    {
    //        return _cache.Where(x => x.Value.Status == CustomConfigItemStatus.Modify).Select(x => x.Value);
    //    }

    //    #endregion

    //    #region IEnumerable<KeyValuePair<string,object>> 成员

    //    public IEnumerator<KeyValuePair<string, CustomConfigItem>> GetEnumerator()
    //    {
    //        return _cache.GetEnumerator();
    //    }

    //    #endregion

    //    #region IEnumerable 成员

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }

    //    #endregion

    //    #region ICustomConfigSection 成员


    //    public bool ContainsKey(string configName)
    //    {
    //        return _cache.ContainsKey(configName);
    //    }

    //    #endregion
    //}


}

