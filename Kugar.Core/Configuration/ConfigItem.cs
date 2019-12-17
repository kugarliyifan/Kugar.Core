using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Configuration
{

    public enum CustomConfigItemStatus
    { 
        Normal,
        Modify
    }

    /// <summary>
    ///     配置项
    /// </summary>
    [Serializable]
    public class CustomConfigItem : INotifyPropertyChanged
    {
        private object _value = null;

        [NonSerialized]
        private CustomConfigItemStatus _status = CustomConfigItemStatus.Normal;

        public CustomConfigItem()
        {
            ConfigType= CustomConfigLevel.System;
        }

        public CustomConfigItem(string name,ConfigItemDataType dataType,object value=null)
        {
            this.Name = name;
            this.DataType = dataType;
            _value =value.Cast(GetDataType());
        }

        /// <summary>
        ///     配置项名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     配置项的值,会对输入的值进行类型比较,<br/>
        ///    如果输入的为非DataType类型的值,则会尝试转换类型,详细的转换类型参考ObjectAbout.Cast函数
        /// </summary>
        /// <see cref="ObjectAbout.Cast"/>
        public object Value
        {
            get { return _value; }

            set
            {

                if (!_value.SafeEquals(value))
                {
                    _value = value;
                    Status = CustomConfigItemStatus.Modify;
                    OnPropertyChanged(Name);
                }

            }

        }

        /// <summary>
        ///     配置项的类型
        /// </summary>
        public ConfigItemDataType DataType { set; get; }

        public CustomConfigLevel ConfigType { private set; get; }

        /// <summary>
        ///     返回配置项值对应的Type
        /// </summary>
        /// <returns></returns>
        public Type GetDataType()
        {
            Type type = null;

            switch (DataType)
            {
                case ConfigItemDataType.Int:
                    type = typeof(int);
                    break;
                case ConfigItemDataType.Decimal:
                    type = typeof(decimal);
                    break;
                case ConfigItemDataType.String:
                    type = typeof(string);
                    break;
                case ConfigItemDataType.Boolean:
                    type = typeof(bool);
                    break;
                default:
                    type = typeof(object);
                    break;

            }

            return type;
        }


        /// <summary>
        ///     当前配置的状态
        /// </summary>
        public CustomConfigItemStatus Status { get; private set; }

        public void SetStatusToNormal()
        {
            Status = CustomConfigItemStatus.Normal;
        }

        [NonSerialized]
        private PropertyChangedEventHandler _propertyChanged=null;

        
        public event PropertyChangedEventHandler PropertyChanged
        {
            add{
                if (_propertyChanged==null)
                {
                    _propertyChanged = value;
                }
                else
                {
                    lock (_propertyChanged)
                    {
                        _propertyChanged += value;
                    }
                }
                
                
            }
            remove{
                if (_propertyChanged==null)
                {
                    return;
                }
                lock (_propertyChanged)
                {
                    _propertyChanged -= value;
                }                
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            //PropertyChangedEventHandler handler = PropertyChanged;
            if (_propertyChanged != null) _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public enum CustomConfigLevel
    {
        System,
        User
    }
}