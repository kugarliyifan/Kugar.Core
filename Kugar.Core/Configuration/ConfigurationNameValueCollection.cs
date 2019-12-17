using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;

namespace Kugar.Core.Configuration
{
    public class NameValueCollectionConfiguration : ConfigurationSection
    {
        public NameValueCollectionConfiguration()
        {
            ValueCollection.ConfigChanged += ConfigChanged;
        }

        public new string this[string _key]
        {
            get
            {
                return ValueCollection[_key];
            }
            set
            {
                ValueCollection[_key] = value;
            }

        }

        [ConfigurationProperty("CustomAttribute", DefaultValue = "kugar")]
        public string CustomAttribute
        {
            get
            {
                return (string)base["CustomAttribute"];
            }
            set
            {
                base["CustomAttribute"] = value;
            }

        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ElementValueCollection ValueCollection
        {//添加了一个新属性，返回类型为一个继承自ConfigurationElementCollection的类

            get{ return (ElementValueCollection)base[""]; }
        }

        public NameValueCollection ToNameValueCollection()
        {
            var n = new NameValueCollection(5);

            foreach (SubConfigurationElement s in ValueCollection)
            {
                n.Add(s.key, s.value);
            }

            return n;
        }

        public Dictionary<string, string> ToDictionary()
        {
            var n = new Dictionary<string, string>(5);

            foreach (SubConfigurationElement s in ValueCollection)
            {
                n.Add(s.key, s.value);
            }

            return n;
        }

        public static NameValueCollectionConfiguration Load(NameValueCollection f)
        {
            if (f==null)
            {
                return null;
            }

            var temp = new NameValueCollectionConfiguration();

            foreach (string s in f.Keys)
            {
                temp.ValueCollection.Add(s,f[s]);
            }

            return temp;

        }

        public static NameValueCollectionConfiguration Load(IDictionary<string,string>f)
        {
            if (f==null)
            {
                return null;
            }

            var temp = new NameValueCollectionConfiguration();

            foreach (var entry in f)
            {
                temp.ValueCollection.Add(entry.Key,entry.Value);
            }

            return temp;
        }

        public event EventHandler<ConfigEditEvntArg> ConfigChanged;
            
    }

    public class SubConfigurationElement : ConfigurationElement
    {
        public SubConfigurationElement() { }

        public SubConfigurationElement(string _key, string _value)
        {
            key = _key;
            value = _value;
        }

        [ConfigurationProperty("key",IsRequired =true)]
        public string key
        {
            set
            {
                this["key"] = value;
            }
            get
            {
                return (string)this["key"];
            }
        }

        [ConfigurationProperty("value",DefaultValue = "")]
        public string value
        {
            set
            {
                this["value"] = value;

                if (ConfigChanged!=null)
                {
                    ConfigChanged(this,new ConfigEditEvntArg(this));
                }

            }
            get
            {
                return (string)this["value"];
            }
        }

        public event EventHandler<ConfigEditEvntArg> ConfigChanged;
    }

    public class ElementValueCollection : ConfigurationElementCollection
    {
        public event EventHandler<ConfigEditEvntArg> ConfigChanged;

        public new string this[string _key]
        {
            get
            {

                return ((SubConfigurationElement)base.BaseGet(_key)).value;
            }
            set
            {
                base[_key] = value;
            }
        }

        public void Add(SubConfigurationElement el)
        {
            el.ConfigChanged += elementchange;

            base.BaseAdd(el, false);
        }

        public void Add(string _key, string _value)
        {
            Add(new SubConfigurationElement(_key, _value));
        }

        public void Remove(string _key)
        {
            var el =(SubConfigurationElement) base.BaseGet(_key);

            if (ConfigChanged!=null)
            {
                ConfigChanged(this, new ConfigEditEvntArg(el));
            }

            base.BaseRemove(_key);
        }

        public void Clear()
        {
            if (ConfigChanged != null)
            {
                ConfigChanged(this, new ConfigEditEvntArg(null));
            }

            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SubConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SubConfigurationElement)element).key;

        }

        private void elementchange(object sender,ConfigEditEvntArg e)
        {
            if (ConfigChanged!=null)
            {
                ConfigChanged(this, e);
            }
        }
    }

    public class ConfigEditEvntArg:EventArgs
    {
        public SubConfigurationElement AffetElement { get; private set; }

        public ConfigEditEvntArg(SubConfigurationElement ev)
        {
            AffetElement = ev;
        }
    }
}
