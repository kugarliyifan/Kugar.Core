using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kugar.Core.ExtMethod;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     动态列分组
    /// </summary>
    public class DynamicColumn : IEnumerable<KeyValuePair<object, object>>
    {
        //private SortedDictionary<object, object> _cacheColumn = new SortedDictionary<object, object>();
		
        //private List<KeyValuePairEx<object,object>> _cacheColumn = new List<KeyValuePairEx<object, object>>();
        
        private Dictionary<object,object> _cacheColumn=new Dictionary<object,object>();
        
        public DynamicColumn()
        {

        }

        /// <summary>
        ///     动态列分组列头文本
        /// </summary>
        public string HeaderText { set; get; }
        
        /// <summary>
        ///     动态列分组唯一标识Name
        /// </summary>
        public string GroupName { set; get; }

        /// <summary>
        ///     该动态列的数据类型
        /// </summary>
        public Type DataType { set; get; }

        /// <summary>
        ///     移除一个动态列
        /// </summary>
        /// <param name="headerText"></param>
        public void RemoveDynamicValue(object headerText)
        {
            if (headerText==null)
            {
                throw new ArgumentNullException("headerText");
            }
            
            if (_cacheColumn.ContainsKey(headerText)) {
            	_cacheColumn.Remove(headerText);
            	OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText));
            }
			
//            var index=_cacheColumn.IndexOf<KeyValuePairEx<object,object>>(x=>x.Key.SafeEquals(headerText));
//            
//            if (index<0) {
//            	return;
//            }
            	
            //_cacheColumn.RemoveAt(index);
            
           // OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText));
        }

        /// <summary>
        ///     索引器访问动态列
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[object headerText]
        {
            get
            {
                object item;
				
//                var v=_cacheColumn.FirstOrDefault(x=>x.Key.SafeEquals(headerText));
//                
//                if (v!=null) {
//                	return v.Value;
//                }
//                else{
//                	return null;
//                }
                
                
                if (_cacheColumn.TryGetValue(headerText,out item))
                {
                    return item;
                }
                else
                {
                    return null;
                }

                //return _cacheColumn[headerText];
            }
            set
            {
                if (headerText==null)
                {
                    throw new ArgumentNullException("headerText");
                }

//                var item=_cacheColumn.FirstOrDefault(x=>x.Key.SafeEquals(headerText));
//                
//                if (item!=null) {
//                	item.Value=value;
//                	OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText,item,value));
//                }
//                else{
//                	var pair=new KeyValuePairEx<object,object>(headerText,value);
//                	_cacheColumn.Add(pair);
//                	OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText,value));
//                }
                
                object item;

                if (_cacheColumn.TryGetValue(headerText,out item))
                {
                    _cacheColumn[headerText] = value;
                    OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText,item,value));
                }
                else
                {
                    _cacheColumn.Add(headerText,value);
                    OnDynamicColumnValueChanged(new DynamicColumnValueChangedEventArgs(this,headerText,value));
                }
            }
        }

        public IEnumerable<object> Headers
        {
        	get 
        	{
        		return _cacheColumn.Keys;// _cacheColumn.Select(x=>x.Key).ToArray();
        	}
        }

        public bool ContainsKey(object headerKey)
        {
        	//return _cacheColumn.Any(x=>x.Key.SafeEquals(headerKey));
        	return _cacheColumn.ContainsKey(headerKey);
        }

        public IDetailWithDynamicColumnsCollection Collection { internal set; get; }

        protected void OnDynamicColumnValueChanged(DynamicColumnValueChangedEventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }

        public event DynamicColumnValueChanged ValueChanged;

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<object,object>> GetEnumerator()
        {
            return _cacheColumn.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is DBNull)
            {
                return false;
            }

            var des = (DynamicColumn)obj;

            return this.GroupName == des.GroupName;

            //return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.GroupName.GetHashCode();

            //return base.GetHashCode();
        }
    }


    public delegate void DynamicColumnValueChanged(object sender, DynamicColumnValueChangedEventArgs e);

    public delegate void DynamicColumnChanged<T>(object sender, DynamicColumnChangedEventArgs<T> e) where T : DynamicColumn;


}