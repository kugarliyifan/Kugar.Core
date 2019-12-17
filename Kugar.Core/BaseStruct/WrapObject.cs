using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Kugar.Core.Exceptions;
using Kugar.Core.ExtMethod;
using NPOI.SS.Formula.Functions;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    /// 用于将一个完整的类,根据属性名切分出不同分部类
    /// </summary>
    public class WrapObject_1 : DynamicObject, ITypedList, ICustomTypeDescriptor
    {
        protected object _sourceObj = null;
        protected HashSet<string> _partialPropertySet = null;
        protected Type _sourceType = null;

        protected static Dictionary<Type, PropertyDescriptor[]> _cachePropertyDescriptorCollections = new Dictionary<Type, PropertyDescriptor[]>();
        protected static Dictionary<Type, EventDescriptorCollection> _cacheEventDescriptorCollections = new Dictionary<Type, EventDescriptorCollection>();

        public WrapObject_1(object sourceObj, string[] partialPropertyNames)
        {
            _sourceType = sourceObj.GetType();

            if (partialPropertyNames==null)
            {
                _partialPropertySet=new HashSet<string>(getPropertyDescriptors(_sourceType).Select(x => x.Name));
            }
            else
            {
                _partialPropertySet = new HashSet<string>(partialPropertyNames);
            }

            _sourceObj = sourceObj;
            //_partialPropertySet = new HashSet<string>(partialPropertyNames);
            
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_partialPropertySet.Contains(binder.Name))
            {
                //result = _sourceObj.FastGetValue(binder.Name);

                result = getPropertyValue(binder.Name);

                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_partialPropertySet.Contains(binder.Name))
            {
                //_sourceObj.FastSetValue(binder.Name, value);

                setPropertyValue(binder.Name,value);

                return true;
            }
            else
            {
                return false;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _partialPropertySet;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == "FastGetValue" ||
                binder.Name == "FastInvoke" ||
                binder.Name == "InvokeIgnoreError" ||
                binder.Name == "FastSetValue"

                )
            {
                result = null;
                var name = args[0].ToStringEx();
                switch (binder.Name)
                {
                    case "FastGetValue":
                        {
                            if (_partialPropertySet.Contains(name))
                            {
                                //result = ObjectReflectionExt.FastGetValue(_sourceObj, name);

                                result = getPropertyValue(name);
                            }
                            else
                            {
                                return false;
                            }

                            break;
                        }
                    case "FastSetValue":
                        {
                            if (_partialPropertySet.Contains(name))
                            {
                                //ObjectReflectionExt.FastSetValue(_sourceObj, name, args[1]);

                                setPropertyValue(name,args[1]);
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        }
                    case "FastInvoke":
                        {
                            result = ObjectReflectionExt.FastInvoke(_sourceObj, name, args.Skip(1).ToArray());
                            break;
                        }
                    case "InvokeIgnoreError":
                        {
                            result = ObjectReflectionExt.FastInvoke(_sourceObj, name, args.Skip(1).ToArray());
                            break;
                        }
                }
                return true;
            }

            result = _sourceObj.FastInvoke(binder.Name, args);

            return true;
        }

        #region ITypedList接口

        public virtual string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "PartialObject";
        }

        public virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            var collection = getPropertyDescriptors(_sourceType);

            return new PropertyDescriptorCollection(collection.Where(x => _partialPropertySet.Contains(x.Name)).ToArray());
        }

        #endregion


        #region ICustomTypeDescriptor 成员

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(_sourceType);
        }

        public string GetClassName()
        {
            return typeof(WrapObject_1).Name;
        }

        public string GetComponentName()
        {
            return typeof(WrapObject_1).Name;
        }

        public TypeConverter GetConverter()
        {
            return new TypeConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(_sourceType);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(_sourceType);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(_sourceType, editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            var col = _cacheEventDescriptorCollections.TryGetValue(_sourceType);

            if (col == null)
            {
                col = TypeDescriptor.GetEvents(_sourceType);

                _cacheEventDescriptorCollections.Add(_sourceType, col);
            }

            return col;

        }

        public EventDescriptorCollection GetEvents()
        {
            return GetEvents(null);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetItemProperties(null);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _sourceObj;
        }

        #endregion

        protected virtual object getPropertyValue(string name)
        {
            return _sourceObj.FastGetValue(name);
        }

        protected virtual void setPropertyValue(string name, object value)
        {
            _sourceObj.FastSetValue(name,value);
        }

        protected static PropertyDescriptor[] getPropertyDescriptors(Type sourceType)
        {
            var collection = _cachePropertyDescriptorCollections.TryGetValue(sourceType);

            if (collection == null)
            {
                collection = TypeDescriptor.GetProperties(sourceType).AsEnumerable<PropertyDescriptor>().ToArray();
                _cachePropertyDescriptorCollections.Add(sourceType, collection);
            }

            return collection;
        }
    }

    /// <summary>
    /// 对WrapObject对象进行扩展,允许对指定属性的值进行重定向
    /// </summary>
    internal class WrapPropertyCastObject : WrapObject_1
    {
        //private WrapObjectPropertyCast[] _propertyCasts = null;

        private Dictionary<string, WrapObjectPropertyCast> _dic = new Dictionary<string, WrapObjectPropertyCast>();

        private Lazy<PropertyDescriptorCollection> _lazy = null;

        public WrapPropertyCastObject(object sourceObj, string[] partialPropertyNames,WrapObjectPropertyCast[] propertyCasts) : base(sourceObj, partialPropertyNames)
        {
            foreach (var cast in propertyCasts)
            {
                _dic.Add(cast.Name,cast);
            }

            _lazy=new Lazy<PropertyDescriptorCollection>(getItemPropertiesLazy);
        }

        public override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return _lazy.Value;
        }

        private PropertyDescriptorCollection getItemPropertiesLazy()
        {
            var plist = base.GetItemProperties(null).AsEnumerable<PropertyDescriptor>().ToArray();

            for (int i = 0; i < plist.Length; i++)
            {
                if (_dic.ContainsKey(plist[i].Name))
                {
                    plist[i] = _dic[plist[i].Name].Descriptor;
                }
            }

            return new PropertyDescriptorCollection(plist);
        }

        protected override object getPropertyValue(string name)
        {
            var result = base.getPropertyValue(name);

            if (_dic.ContainsKey(name))
            {
                result = _dic[name].CastOutFunc(result);
            }

            return result;

            //return base.getPropertyValue(name);
        }

        protected override void setPropertyValue(string name, object value)
        {
            if (_dic.ContainsKey(name))
            {
                value = _dic[name].CastInFunc(value);
            }

            base.setPropertyValue(name, value);
        }
    }

    public class WrapObjectPropertyCast
    {
        private Lazy<WrappPropertyDesctiptor> _lazy = null;
        private Func<object, object> _castOutFunc = null;
        private Func<object, object> _castInFunc = null;
        private string _name = "";

        public WrapObjectPropertyCast(string propertyName,
                                      Func<object, object> castOutFunc,
                                      Func<object, object> castInFunc
                                     )
        {
            _name = propertyName;

            _castOutFunc = castOutFunc;
            _castInFunc = castInFunc;

            _lazy = new Lazy<WrappPropertyDesctiptor>(createDesctiptor);

            //var s = new WrappPropertyDesctiptor<TOutResult, TInResult>("", castOutFunc, castInFunc);
        }

        public string Name {get { return _name; }}

        public PropertyDescriptor Descriptor
        {
            get { return _lazy.Value; }
        }

        public Func<object, object> CastOutFunc { get { return _castOutFunc; } }

        public Func<object, object> CastInFunc { get { return _castInFunc; } } 

        private WrappPropertyDesctiptor createDesctiptor()
        {
            var desc = new WrappPropertyDesctiptor(_name, _castOutFunc, _castInFunc);

            return desc;
        }
    }

    internal class WrappPropertyDesctiptor : PropertyDescriptor
    {
        private Func<object, object> _castOutFunc = null;
        private Func<object, object> _castInFunc = null;
        private string _name = "";

        public WrappPropertyDesctiptor(string name, Func<object, object> castOutFunc, Func<object, object> castInFunc)
            : base(name, null)
        {
            _castOutFunc = castOutFunc;
            _castInFunc = castInFunc;
            _name = name;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return _castOutFunc(component.FastGetValue(_name));
        }

        public override void ResetValue(object component)
        {
            return;
        }

        public override void SetValue(object component, object value)
        {
            component.FastSetValue(_name, _castInFunc(value));
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(WrappPropertyDesctiptor); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(object); }
        }
    }

    public static class PartialObjectExtMethod
    {
        /// <summary>
        /// 将指定的对象包装只包含部分属性的子类,对新类属性的修改会影响源对象对应属性值
        /// </summary>
        /// <param name="sourceObj"></param>
        /// <param name="partialPropertyNames">抽取出来的属性名列表</param>
        /// <returns></returns>
        public static WrapObject_1 ToPartialObject(this object sourceObj, string[] partialPropertyNames)
        {
            return new WrapObject_1(sourceObj, partialPropertyNames);
        }

        /// <summary>
        /// 将指定的对象包装只包含部分属性的子类,对新类属性的修改会影响源对象对应属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObj">源对象</param>
        /// <param name="partialPropertyNames">抽取出来的属性名表达式列表</param>
        /// <returns></returns>
        public static WrapObject_1 ToPartialObject<T>(this T sourceObj,params Expression<Func<T, object>>[] partialPropertyNames)
        {
            //var names =
            //    partialPropertyNames.Where(x => x.Body is MemberExpression)
            //        .Select(x => ((MemberExpression) x.Body).Member.Name)
            //        .ToArray();

            var names = new List<string>(partialPropertyNames.Length);



            foreach (var nameExpression in partialPropertyNames)
            {
                string propertyName = nameExpression.GetPropertyName();    //返回的属性名

                names.Add(propertyName);
            }

            return ToPartialObject(sourceObj, names.ToArray());
        }

        /// <summary>
        /// 将指定的对象包装对指定名称的属性设置和返回值的类型进行重定向,对新类属性的修改,将影响源对象对应属性的值
        /// </summary>
        /// <param name="sourceObj">源对象</param>
        /// <param name="castLst">属性转换列表</param>
        /// <returns></returns>
        public static WrapObject_1 ToWrapObject(this object sourceObj, WrapObjectPropertyCast[] castLst)
        {
            return new WrapPropertyCastObject(sourceObj, null, castLst);
        }

        /// <summary>
        /// 将指定的对象的包装,抽取指定名称列表的属性,并对指定名称的属性设置和返回值的类型进行重定向,对新类属性的修改,将影响源对象对应属性的值
        /// </summary>
        /// <param name="sourceObj">源对象</param>
        /// <param name="partialPropertyNames">抽取的属性名列表</param>
        /// <param name="castLst">属性转换列表</param>
        /// <returns></returns>
        public static WrapObject_1 ToWrapObject(this object sourceObj, string[] partialPropertyNames,
            WrapObjectPropertyCast[] castLst)
        {
            return new WrapPropertyCastObject(sourceObj, partialPropertyNames, castLst);
        }
    }
}
