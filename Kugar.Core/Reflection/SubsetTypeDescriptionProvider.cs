using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Reflection
{
    /// <summary>
    ///     多级属性绑定
    /// </summary>
    /// <typeparam name="T">绑定的对象类型</typeparam>
    public class SubsetTypeDescriptionProvider<T> : TypeDescriptionProvider
    {
        private ICustomTypeDescriptor td;

        public SubsetTypeDescriptionProvider()
            : this(TypeDescriptor.GetProvider(typeof(T)))
        {
        }

        public SubsetTypeDescriptionProvider(TypeDescriptionProvider parent)
            : base(parent)
        {

        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (td == null)
            {
                td = base.GetTypeDescriptor(objectType, instance);
                td = new SubsetCustomTypeDescriptor(td);
            }
            return td;
        }
    }

    internal class SubsetCustomTypeDescriptor : CustomTypeDescriptor
    {
        //递归遍历的最大层数
        private const int MaxGetPropertyDepth = 9;

        public SubsetCustomTypeDescriptor(ICustomTypeDescriptor parent)
            : base(parent)
        {

        }


        public override PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection cols = base.GetProperties();

            //var lst = new List<PropertyDescriptor>(cols.Count*2);

            var dic = new Dictionary<string, PropertyDescriptor>(cols.Count * 2);

            int count = 1;

            foreach (PropertyDescriptor col in cols)
            {
                if (dic.ContainsKey(col.Name))
                {
                    continue;
                }

                dic.Add(col.Name, col);

                if (!col.PropertyType.IsValueType && !col.PropertyType.IsAssignableFrom(typeof(IEnumerable)) && !col.PropertyType.IsAssignableFrom(typeof(IEnumerable<>)))
                {
                    getNextLevelProperty(col, dic, ref count);
                }
            }

            PropertyDescriptorCollection newcols = new PropertyDescriptorCollection(dic.Values.ToArray());
            return newcols;
        }

        private void getNextLevelProperty(PropertyDescriptor parent, Dictionary<string, PropertyDescriptor> lst, ref int currentDepth)
        {
            foreach (PropertyDescriptor propertyDescriptor in parent.GetChildProperties())
            {
                var name = string.Format("{0}.{1}", parent.Name, propertyDescriptor.Name);

                if (lst.ContainsKey(name))
                {
                    continue;
                }

                var destor = new SubsetPropertyDescriptor(parent, propertyDescriptor, name);

                lst.Add(name, destor);

                currentDepth += 1;
                if (!propertyDescriptor.PropertyType.IsValueType &&
                    propertyDescriptor.PropertyType.IsClass && 
                    !propertyDescriptor.PropertyType.IsAssignableFrom(typeof(IEnumerable)) &&
                    !propertyDescriptor.PropertyType.IsAssignableFrom(typeof(IEnumerable<>)) && 
                    currentDepth <= MaxGetPropertyDepth)
                {

                    getNextLevelProperty(destor, lst, ref currentDepth);

                }
                currentDepth -= 1;
            }
        }
    }

    //public class pro:PropertyDescriptor
    //{
    //    private PropertyInfo _property = null;

    //    public pro(PropertyInfo property, string pdname)
    //        : base(pdname, null)
    //    {
    //        _property = property;
    //    }


    //    #region Overrides of PropertyDescriptor

    //    public override bool IsReadOnly { get { return !_property.CanWrite; } }
    //    public override void ResetValue(object component) { }
    //    public override bool CanResetValue(object component) { return false; }
    //    public override bool ShouldSerializeValue(object component)
    //    {
    //        return true;
    //    }

    //    public override object GetValue(object component)
    //    {
    //        return component.FastGetValue(base.Name);
    //    }

    //    public override void SetValue(object component, object value)
    //    {
    //        component.FastSetValue(base.Name,value);
    //    }

    //    public override Type ComponentType
    //    {
    //        get { return _property.DeclaringType; }
    //    }

    //    public override Type PropertyType
    //    {
    //        get { return _property.PropertyType; }
    //    }

    //    #endregion
    //}


    internal class SubsetPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor _subPD;
        private PropertyDescriptor _parentPD;
        private string _name = "";

        public SubsetPropertyDescriptor(PropertyDescriptor parentPD, PropertyDescriptor subPD, string pdname)
            : base(pdname, null)
        {
            _subPD = subPD;
            _parentPD = parentPD;
            _name = pdname;
        }

        public override string Name
        {
            get
            {
                return _name;
                return base.Name;
            }
        }

        public override bool IsReadOnly { get { return _subPD.IsReadOnly; } }
        public override void ResetValue(object component) { }
        public override bool CanResetValue(object component) { return false; }
        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return _parentPD.ComponentType; }
        }
        public override Type PropertyType { get { return _subPD.PropertyType; } }

        public override object GetValue(object component)
        {
            try
            {
                return component.FastGetValue(base.Name);
            }
            catch (NullReferenceException)
            {
                return null;
            }
            
            return _subPD.GetValue(_parentPD.GetValue(component));
        }

        public override void SetValue(object component, object value)
        {
            try
            {
                component.FastSetValue(base.Name, value);
                OnValueChanged(component, EventArgs.Empty);
            }
            catch (NullReferenceException)
            {
                
            }
            
            //_subPD.SetValue(_parentPD.GetValue(component), value);
            
        }

        //public override string ToString()
        //{
        //    return base.Name;
        //    return base.ToString();
        //}
    }

}
