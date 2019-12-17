using System;
using System.Collections;
using System.Collections.Generic;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     字典类内容被修改时,引发的事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDictionaryChanged<T>
    {
        event DictionaryChangedEventHandler<T> DictionaryChanged;
    }

    /// <summary>
    ///     事件处理委托
    /// </summary>
    /// <typeparam name="T">字典类Key的类型</typeparam>
    /// <param name="sender">发送者</param>
    /// <param name="e">参数</param>
    public delegate void DictionaryChangedEventHandler<T>(object sender, DictionaryChangedEventArgs<T> e);

    /// <summary>
    ///     当字典类出现被变更时,用于传递的参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DictionaryChangedEventArgs<T>:EventArgs
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="effectKey">发生变更的Key</param>
        /// <param name="changedType">变更类型</param>
        public DictionaryChangedEventArgs(T effectKey, object effecValue, DictionaryChangedType changedType)
        {
            DictionaryChangedType = changedType;
            EffectKey = effectKey;
            EffectValue = effecValue;
        }

        /// <summary>
        ///     变更类型
        /// </summary>
        public DictionaryChangedType DictionaryChangedType { protected set; get; }

        /// <summary>
        ///     发生变更的key
        /// </summary>
        public T EffectKey { private set; get; }

        public object EffectValue { private set; get; }
    }

    /// <summary>
    ///     变更类型
    /// </summary>
    public enum DictionaryChangedType
    {
        /// <summary>
        ///     添加项目操作
        /// </summary>
        ItemAdded,

        /// <summary>
        ///     删除项目操作
        /// </summary>
        ItemDeleted,

        /// <summary>
        ///     修改Key对应的Value操作
        /// </summary>
        ItemChanged,

        /// <summary>
        ///     清空整个字典类操作
        /// </summary>
        Reset
    }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IListItemChangedEventArgs<TKey> : EventArgs
    {
        public IListItemChangedEventArgs(TKey _key)
        {
            Key = _key;

        }

        public TKey Key { set; get; }
    }


    ///// <summary>
    /////     一个扩展的Dictionary 类,支持数据访问时的自动添加,以及添加,删除,修改的事件通知
    ///// </summary>
    ///// <typeparam name="TKey">key类型</typeparam>
    ///// <typeparam name="TValue">value类型</typeparam>
    public partial class DictionaryEx<TKey,TValue>:DictionaryBase,IDictionaryChanged<TKey>,IEnumerable<KeyValuePair<TKey,TValue>>,IDictionary<TKey,TValue>
    {
        private bool isUpdating = false;

        public DictionaryEx()
            : base()
        {
            IsAutoAddKeyPair = true;
        }


        /// <summary>
        ///     获取指定key关联的value
        /// </summary>
        /// <param name="key">指定的key值</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!base.Dictionary.Contains(key))
                {
                    if (!IsAutoAddKeyPair)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    else
                    {
                        return default(TValue);
                    }
                }
                else
                {
                    return (TValue)base.Dictionary[key];
                }


            }
            set
            {
                if (!base.Dictionary.Contains(key))
                {
                    if (!IsAutoAddKeyPair)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    else
                    {
                        this.Add(key, (TValue)value);
                    }
                }
                else
                {
                    base.Dictionary[key]=value;
                }
            }
        }


        #region "公用函数"
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

            try
            {
                if (!this.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    value = (TValue) base.Dictionary[key];
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }


        }

        public TValue TryGetValue(TKey key)
        {
            return TryGetValue(key, default(TValue));
        }

        public TValue TryGetValue(TKey key,TValue defaultValue)
        {
            TValue ret;

            if (this.TryGetValue(key,out ret))
            {
                return ret;
            }
            else
            {
                return defaultValue;
            }



        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array==null || arrayIndex<0 || array.Length<arrayIndex)
            {
                return;
            }

            lock (base.Dictionary)
            {
                int i = arrayIndex;
                foreach (DictionaryEntry v in base.Dictionary)
                {
                    var temp = new KeyValuePair<TKey, TValue>((TKey) v.Key, (TValue) v.Value);
                    array[i] = temp;
                    i++;
                }
            }
        }


        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (base.Dictionary.Count<=0)
            {
                yield break;
            }

            lock (base.Dictionary)
            {
                foreach (DictionaryEntry v in base.Dictionary)
                {
                    yield return (new KeyValuePair<TKey,TValue>((TKey)v.Key,(TValue) v.Value));
                }
            }
        }

        public void BeginUpdate()
        {
            if(isUpdating)
            {
                throw new Exception("当前处于更新状态,请先调用EndUpdate函数");
            }

            isUpdating = true;

            
        }

        public void EndUpdate()
        {
            isUpdating = false;
            OnDictionaryChanged(default(TKey), default(TValue), DictionaryChangedType.Reset);
        }



        //添加

        /// <summary>
        ///     添加一个键值对
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">关联的值</param>
        public void Add(TKey key,TValue value)
        {
            base.Dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        //删除

        /// <summary>
        ///     移除一个键值对
        /// </summary>
        /// <param name="key"></param>
        public bool Remove(TKey key)
        {
            try
            {
                lock (base.Dictionary)
                {
                    if (!this.ContainsKey(key))
                    {
                        return false;
                    }

                    base.Dictionary.Remove(key);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
            
        }

        /// <summary>
        ///     删除一个项目
        /// </summary>
        /// <param name="item">指定要删除的键值对</param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }


        //判断是否包含值或者键

        /// <summary>
        ///     判断是否包含指定的key
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>如果含有指定的key返回true,不包含返回false</returns>
        public bool ContainsKey(TKey key)
        {
            return base.Dictionary.Contains(key);
        }

        /// <summary>
        ///     判断是否包含指定的value
        /// </summary>
        /// <param name="value">指定的值</param>
        /// <returns>如果含有指定的key返回true,不包含返回false</returns>
        public bool ContainsValue(TValue value)
        {
            var t = this.Values;

            if (t==null)
            {
                return false;
            }

            var ret=Array.Find(t, e => e.Equals(value));

            return !ret.Equals(default(TValue));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.ContainsKey(item.Key);
        }

        #endregion


        #region "公用属性"

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     设置是否当使用索引器设置一个值时,当不存在指定关键字的项目时,是否自动添加
        /// </summary>
        public bool IsAutoAddKeyPair { set; get; }

        /// <summary>
        ///     返回值key列表
        /// </summary>
        public TKey[] Keys
        {
            get
            {
                var temp = base.Dictionary.Keys;

                if (temp.Count <= 0)
                {
                    return null;
                }

                var lst = new List<TKey>(temp.Count);

                lock (base.Dictionary)
                {
                    foreach (var v in temp)
                    {
                        lst.Add((TKey)v);
                    }
                }

                return lst.ToArray();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return Keys; }
        }



        /// <summary>
        ///     返回value列表
        /// </summary>
        public TValue[] Values
        {
            get
            {
                var temp = base.Dictionary.Values;

                if (temp.Count <= 0)
                {
                    return null;
                }

                var lst = new List<TValue>(base.Dictionary.Count);

                lock (base.Dictionary)
                {
                    foreach (var v in temp)
                    {
                        lst.Add((TValue)v);
                    }

                }

                return lst.ToArray();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return Values; }
        }


        #endregion


        #region "公用事件"

        /// <summary>
        ///     列表发生改动时,引发的事件
        /// </summary>
        public event DictionaryChangedEventHandler<TKey> DictionaryChanged;

        #endregion






        #region "重载原有类的函数,用于输入检查以及事件引发"

        protected override void OnInsert(object key, object value)
        {
            TKeyTValueCheck(key, value);

            base.OnInsert(key, value);
        }

        protected override void OnRemove(object key, object value)
        {
            TKeyTValueCheck(key, value);

            base.OnRemove(key, value);
        }

        protected override object OnGet(object key, object currentValue)
        {
            TKeyTValueCheck(key, currentValue);

            return base.OnGet(key, currentValue);
        }

        protected override void OnSet(object key, object oldValue, object newValue)
        {

            TKeyTValueCheck(key, newValue);

            base.OnSet(key, oldValue, newValue);
        }

        protected override void OnValidate(object key, object value)
        {
            TKeyTValueCheck(key, value);

            base.OnValidate(key, value);
        }

        protected override void OnInsertComplete(object key, object value)
        {
            TKeyTValueCheck(key, value);

            OnDictionaryChanged((TKey)key,value, DictionaryChangedType.ItemAdded);

            base.OnInsertComplete(key, value);
        }

        protected override void OnRemoveComplete(object key, object value)
        {
            TKeyTValueCheck(key, value);

            OnDictionaryChanged((TKey)key, value, DictionaryChangedType.ItemDeleted);

            base.OnRemoveComplete(key, value);
        }

        protected override void OnSetComplete(object key, object oldValue, object newValue)
        {
            OnDictionaryChanged((TKey)key,newValue, DictionaryChangedType.ItemChanged);

            base.OnSetComplete(key, oldValue, newValue);
        }

        protected override void OnClearComplete()
        {
            OnDictionaryChanged(default(TKey),null, DictionaryChangedType.Reset);

            base.OnClearComplete();
        }

        #endregion

        private Type keyType = typeof (TKey);
        private Type valueType = typeof (TValue);

        protected bool TKeyTValueCheck(object key, object value)
        {
            if (key.GetType() != keyType && !(value is TValue))
                throw new ArgumentException("key must be of type TKey.", "Key");

            if (value.GetType() != valueType && !(value is TValue))
                throw new ArgumentException("value must be of type TValue.", "Value");

            return true;
        }


        protected virtual void OnDictionaryChanged(TKey key,object value, DictionaryChangedType changedType)
        {
            if (DictionaryChanged!=null)
            {
                DictionaryChanged(this, new DictionaryChangedEventArgs<TKey>(key, value,changedType));
            }
        }




    }
}
