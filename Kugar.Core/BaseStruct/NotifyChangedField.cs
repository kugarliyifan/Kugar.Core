using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kugar.Core.BaseStruct
{

    public interface INotifyChangedField:INotifyPropertyChanged,INotifyPropertyChanging
    {
        string Name { get; }

        object Value { get; set; }

        bool ReadOnly { get; }
    }
    
    public interface INotifyChangedField<T>:INotifyPropertyChanged,INotifyPropertyChanging
    {
        string Name { get; }

        T Value { get; set; }

        bool ReadOnly { get; }
    }

    /// <summary>
    ///     创建一个带修改时事件通知的字段,允许设置是否可读
    /// </summary>
    public static class NotifyChangedField
    {
        /// <summary>
        ///     创建一个可读写的普通Object类型值的字段
        /// </summary>
        /// <param name="propertyName">字段属性名</param>
        /// <returns></returns>
        public static INotifyChangedField Create(string propertyName, object defaultValue=null)
        {
            return new InternalNotifyChangedField(propertyName,defaultValue,false);
        }

        /// <summary>
        ///     创建一个只读的普通Object类型值的字段
        /// </summary>
        /// <param name="propertyName">字段属性名</param>
        /// <returns></returns>
        public static INotifyChangedField CreateReadOnly(string propertyName, object defaultValue = null)
        {
            return new InternalNotifyChangedField(propertyName, defaultValue, true);
        }

        /// <summary>
        ///     创建一个可读写的泛型类型值的字段
        /// </summary>
        /// <param name="propertyName">字段属性名</param>
        /// <returns></returns>
        public static INotifyChangedField<T> Create<T>(string propertyName,T defaultValue=default(T))
        {
            return new InternalNotifyChangedField<T>(propertyName,defaultValue,false);
        }

        /// <summary>
        ///     创建一个只读的泛型类型值的字段
        /// </summary>
        /// <param name="propertyName">字段属性名</param>
        /// <returns></returns>
        public static INotifyChangedField<T> CreateReadOnly<T>(string propertyName,T defaultValue=default(T))
        {
            return new InternalNotifyChangedField<T>(propertyName, defaultValue, true);
        }
    }

    internal class InternalNotifyChangedField:INotifyChangedField
    {
        private string _name = string.Empty;
        private object _value = null;
        private bool _isReadOnly = false;

        public InternalNotifyChangedField(string name,object defaultValue,bool isReadOnly)
        {
            _name = name;
            _isReadOnly = isReadOnly;
            _value = defaultValue;
        }

        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
            set
            {
                if (!value.Equals(_value))
                {
                    if (OnPropertyChanging())
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool ReadOnly { get { return _isReadOnly; } }

        #region Implementation of INotifyPropertyChanged

        protected void OnPropertyChanged()
        {
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of INotifyPropertyChanging

        protected bool OnPropertyChanging()
        {
            if (PropertyChanging != null)
            {

                var e = new PropertyChangingEventArgs(Name);

                try
                {
                    PropertyChanging(this, e);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        #endregion
    }

    internal class InternalNotifyChangedField<T> : INotifyChangedField<T>
    {
        private string _name = string.Empty;
        private T _value = default(T);
        private bool _isReadOnly = false;

        public InternalNotifyChangedField(string name, T defaultValue, bool isReadOnly)
        {
            _name = name;
            _isReadOnly = isReadOnly;
            _value = defaultValue;
        }

        public string Name
        {
            get { return _name; }
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (!value.Equals(_value))
                {
                    if (OnPropertyChanging())
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool ReadOnly { get { return _isReadOnly; } }

        #region Implementation of INotifyPropertyChanged

        protected void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of INotifyPropertyChanging

        protected bool OnPropertyChanging()
        {
            if (PropertyChanging != null)
            {

                var e = new PropertyChangingEventArgs(Name);

                try
                {
                    PropertyChanging(this, e);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        #endregion
    }
}
