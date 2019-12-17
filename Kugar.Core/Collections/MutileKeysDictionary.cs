using System;
using System.Collections;
using System.Collections.Generic;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     一个2个Key指定1个Value的字典类
    /// </summary>
    /// <typeparam name="TKey1">Key1类型</typeparam>
    /// <typeparam name="TKey2">Key2类型</typeparam>
    /// <typeparam name="TValue">Value类型</typeparam>
    public class MutileKeysDictionary<TKey1, TKey2, TValue> : IEnumerable<KeysValuePair<TKey1, TKey2, TValue>>
    {
        private Dictionary<TKey1, Dictionary<TKey2, TValue>> cacheData = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();

        private IEqualityComparer<TKey2> _key2Comparer = null; 

        public MutileKeysDictionary()
        {

        }

        public MutileKeysDictionary(IEnumerable<KeysValuePair<TKey1, TKey2, TValue>> lst)
            : this()
        {
            if (lst != null)
            {
                foreach (var pair in lst)
                {
                    Add(pair);
                }
            }
        }

        public MutileKeysDictionary(IEqualityComparer<TKey1> key1Comparer, IEqualityComparer<TKey2> key2compare, IEnumerable<KeysValuePair<TKey1, TKey2, TValue>> lst)
        {
            cacheData=new Dictionary<TKey1, Dictionary<TKey2, TValue>>(key1Comparer);

            _key2Comparer = key2compare;

            if (lst != null)
            {
                foreach (var pair in lst)
                {
                    Add(pair);
                }
            }
        }

        private readonly object lockerObject = new object();

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get
            {
                if (ContainsKey(key1, key2))
                {
                    var dic = cacheData[key1];

                    return dic[key2];

                }
                else
                {
                    throw new KeyNotFoundException(@"不存在指定的key值");
                }
            }
            set
            {
                if (ContainsKey(key1, key2))
                {
                    cacheData[key1][key2] = value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public Dictionary<TKey2, TValue> this[TKey1 key1]
        {
            get
            {
                if (cacheData.ContainsKey(key1))
                {
                    return cacheData[key1];
                }
                else
                {
                    throw new KeyNotFoundException(@"不存在指定的key值");
                }
            }
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> Add(TKey1 key1)
        {
            cacheData.Add(key1,new Dictionary<TKey2, TValue>());

            return this;
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> Add(TKey1 key1, TKey2 key2, TValue value)
        {
            return Add(key1, key2, value, true);
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> Add(KeysValuePair<TKey1, TKey2, TValue> keysValuePair)
        {
            return Add(keysValuePair.Key1, keysValuePair.Key2, keysValuePair.Value);
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> AddSafe(TKey1 key1, TKey2 key2, TValue value)
        {
            return Add(key1, key2, value, false);
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> AddOrUpdate(TKey1 key1, TKey2 key2, TValue value)
        {
            Dictionary<TKey2, TValue> dic = null;

            lock (lockerObject)
            {
                if (cacheData.ContainsKey(key1))
                {
                    dic = cacheData[key1];
                }
                else
                {
                    if (_key2Comparer!=null)
                    {
                        dic = new Dictionary<TKey2, TValue>(_key2Comparer);
                    }
                    else
                    {
                        dic = new Dictionary<TKey2, TValue>();
                    }
                    
                    cacheData.Add(key1, dic);
                }

                if (dic.ContainsKey(key2))
                {
                    dic[key2] = value;
                }
                else
                {
                    dic.Add(key2, value);
                }

            }

            return this;
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> Remove(TKey1 key1, TKey2 key2)
        {
            lock (lockerObject)
            {
                if (ContainsKey(key1, key2))
                {
                    var dic = cacheData[key1];

                    dic.Remove(key2);

                    if (dic.Count == 0)
                    {
                        cacheData.Remove(key1);
                    }
                }
            }


            return this;
        }

        public MutileKeysDictionary<TKey1, TKey2, TValue> Remove(TKey1 key1)
        {
            lock (lockerObject)
            {
                if (cacheData.ContainsKey(key1))
                {


                    var dic = cacheData[key1];

                    dic.Clear();

                    cacheData.Remove(key1);

                    //dic.Remove(key2);

                    //if (dic.Count == 0)
                    //{
                    //    cacheData.Remove(key1);
                    //}
                }
            }

            return this;
        }

        public void Clear()
        {
            lock (lockerObject)
            {
                foreach (var kp in cacheData)
                {
                    kp.Value.Clear();
                }

                cacheData.Clear();
            }

        }

        /// <summary>
        ///     是否包含指定的主key
        /// </summary>
        /// <param name="key1"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey1 key1)
        {
            return cacheData.ContainsKey(key1);
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            Dictionary<TKey2, TValue> key2Dic = null;

            if (cacheData.ContainsKey(key1))
            {
                key2Dic = cacheData[key1];
            }
            else
            {
                return false;
            }

            if (key2Dic.ContainsKey(key2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            if (this.ContainsKey(key1, key2))
            {
                value = this[key1, key2];
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public Dictionary<TKey2,TValue> TryGetValue(TKey1 key1)
        {
            lock (lockerObject)
            {
                Dictionary<TKey2, TValue> result = null;

                if (cacheData.TryGetValue(key1,out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public TValue TryGetValue(TKey1 key1, TKey2 key2)
        {
            return TryGetValue(key1, key2, default(TValue));
        }

        public TValue TryGetValue(TKey1 key1, TKey2 key2, TValue defaultValue)
        {
            TValue value;

            if (TryGetValue(key1, key2, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public IEnumerable<KeysPair> Keys
        {
            get
            {
                List<KeysPair> tempList = new List<KeysPair>(cacheData.Count * 2);

                foreach (var key1 in cacheData)
                {
                    var dic = key1.Value;

                    if (dic.Count > 0)
                    {
                        foreach (var value in dic)
                        {
                            tempList.Add(new KeysPair(key1.Key, value.Key));
                        }
                    }
                }

                return tempList;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                List<TValue> tempList = new List<TValue>(cacheData.Count * 2);

                foreach (var key1 in cacheData)
                {
                    var dic = key1.Value;

                    if (dic.Count > 0)
                    {
                        foreach (var value in dic)
                        {
                            tempList.Add(value.Value);
                        }
                    }
                }

                return tempList;
            }
        }

        public int Count
        {
            get { return cacheData.Count; }
        }

        protected MutileKeysDictionary<TKey1, TKey2, TValue> Add(TKey1 key1, TKey2 key2, TValue value, bool isThrowException)
        {
            Dictionary<TKey2, TValue> key2Dic = null;

            lock (lockerObject)
            {
                if (ContainsKey(key1, key2))
                {
                    if (isThrowException)
                    {
                        throw new ArgumentException("不能添加相同的键值");
                    }
                    else
                    {
                        return this;
                    }
                }

                if (cacheData.ContainsKey(key1))
                {
                    key2Dic = cacheData[key1];
                }
                else
                {
                    key2Dic = new Dictionary<TKey2, TValue>();
                    cacheData.Add(key1, key2Dic);
                }

                key2Dic.Add(key2, value);
            }



            return this;
        }

        #region "IEnumerator"

        public IEnumerator<KeysValuePair<TKey1, TKey2, TValue>> GetEnumerator()
        {
            foreach (var keyValue in cacheData)
            {
                var subDic = keyValue.Value;

                if (subDic.Count > 0)
                {
                    foreach (var valuePair in subDic)
                    {
                        yield return new KeysValuePair<TKey1, TKey2, TValue>(keyValue.Key, valuePair.Key, valuePair.Value);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public class KeysPair
        {
            public KeysPair(TKey1 key1, TKey2 key2)
            {
                Key1 = key1;
                Key2 = key2;
            }

            public TKey1 Key1 { set; get; }

            public TKey2 Key2 { set; get; }

            public bool Equals(KeysPair other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Key1, Key1) && Equals(other.Key2, Key2);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(KeysPair)) return false;
                return Equals((KeysPair)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Key1.GetHashCode() * 397) ^ Key2.GetHashCode();
                }
            }
        }


    }

    public class KeysValuePair<TKey1, TKey2, TValue>
    {
        public KeysValuePair(TKey1 key1, TKey2 key2, TValue value)
        {
            Key1 = key1;
            Key2 = key2;
            Value = value;
        }

        public TKey1 Key1 { set; get; }

        public TKey2 Key2 { set; get; }

        public TValue Value { set; get; }

        public bool Equals(KeysValuePair<TKey1, TKey2, TValue> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Key1, Key1) && Equals(other.Key2, Key2) && Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(KeysValuePair<TKey1, TKey2, TValue>)) return false;
            return Equals((KeysValuePair<TKey1, TKey2, TValue>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Key1.GetHashCode();
                result = (result * 397) ^ Key2.GetHashCode();
                result = (result * 397) ^ Value.GetHashCode();
                return result;
            }
        }
    }

    public class ReadOnlyMutileKeysDictionary<TKey1, TKey2, TValue> : IEnumerable<KeysValuePair<TKey1, TKey2, TValue>>
    {
        private MutileKeysDictionary<TKey1, TKey2, TValue> collection = null;

        public ReadOnlyMutileKeysDictionary(MutileKeysDictionary<TKey1, TKey2, TValue> srcCollection)
        {
            collection = srcCollection;
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return collection.ContainsKey(key1, key2);
        }

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get { return collection[key1, key2]; }
        }

        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            return collection.TryGetValue(key1, key2, out value);
        }

        public TValue TryGetValue(TKey1 key1, TKey2 key2)
        {
            return TryGetValue(key1, key2, default(TValue));
        }

        public TValue TryGetValue(TKey1 key1, TKey2 key2, TValue defaultValue)
        {
            TValue value;

            if (TryGetValue(key1, key2, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public int Count
        {
            get { return collection.Count; }
        }


        public IEnumerator<KeysValuePair<TKey1, TKey2, TValue>> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    //public class MutileKeysDictionary<TKey1, TKey2, TValue> : IEnumerable<KeysValuePair<TKey1, TKey2, TValue>>
    //{
    //    private readonly object lockerObject = new object();
    //    protected Dictionary<long, TValue> cacheData = new Dictionary<long, TValue>();

    //    protected List<KeysPair> lstKeyPair=new List<KeysPair>();


    //    public MutileKeysDictionary<TKey1,TKey2,TValue> Add(TKey1 key1,TKey2 key2,TValue value)
    //    {
    //        var keyCode = GetKeysHashCode(key1, key2);

    //        lock (lockerObject)
    //        {
    //            if (cacheData.ContainsKey(keyCode))
    //            {
    //                throw new ArgumentOutOfRangeException(@"已存在相同的Key");
    //            }

    //            cacheData.Add(keyCode,value);
    //            lstKeyPair.Add(new KeysPair(key1,key2));                
    //        }



    //        return this;
    //    }

    //    public MutileKeysDictionary<TKey1,TKey2,TValue> AddOrUpdate(TKey1 key1,TKey2 key2,TValue value)
    //    {
    //        var keyCode = GetKeysHashCode(key1, key2);

    //        lock (lockerObject)
    //        {
    //            if (cacheData.ContainsKey(keyCode))
    //            {
    //                cacheData[keyCode] = value;
    //            }
    //            else
    //            {
    //                cacheData.Add(keyCode,value);
    //                lstKeyPair.Add(new KeysPair(key1, key2));
    //            }
    //        }

    //        return this;
    //    }

    //    public TValue this[TKey1 key1,TKey2 key2]
    //    {
    //        get 
    //        {
    //            var keyCode = GetKeysHashCode(key1, key2);
    //            lock (lockerObject)
    //            {
    //                if (cacheData.ContainsKey(keyCode))
    //                {
    //                    return cacheData[keyCode];
    //                }
    //                else
    //                {
    //                    throw new KeyNotFoundException();
    //                }                    
    //            }
    //        }
    //        set
    //        {
    //            var keyCode = GetKeysHashCode(key1, key2);

    //            lock (lockerObject)
    //            {
    //                if (cacheData.ContainsKey(keyCode))
    //                {
    //                    cacheData[keyCode] = value;
    //                }
    //                else
    //                {
    //                    throw new KeyNotFoundException();
    //                }                    
    //            }
    //        }
    //    }

    //    public MutileKeysDictionary<TKey1,TKey2,TValue> Remove(TKey1 key1,TKey2 key2)
    //    {
    //        var keyCode = GetKeysHashCode(key1, key2);

    //        lock (lockerObject)
    //        {
    //            if (cacheData.ContainsKey(keyCode))
    //            {
    //                cacheData.Remove(keyCode);

    //                KeysPair kp = null;

    //                for (int i = 0; i < lstKeyPair.Count; i++)
    //                {
    //                    if(Equals(lstKeyPair[i].Key1,key1) && Equals(lstKeyPair[i].Key2,key2))
    //                    {
    //                        kp = lstKeyPair[i];
    //                    }
    //                }

    //                lstKeyPair.Remove(kp);
    //            }                    
    //        }



    //        return this;
    //    }

    //    public void Clear()
    //    {
    //        lock (lockerObject)
    //        {
    //            cacheData.Clear();
    //            lstKeyPair.Clear();                
    //        }

    //    }

    //    public bool ContainsKey(TKey1 key1,TKey2 key2)
    //    {
    //        var keyCode = GetKeysHashCode(key1, key2);

    //        lock (lockerObject)
    //        {
    //            return cacheData.ContainsKey(keyCode);
    //        }

    //    }

    //    public bool TryGetValue(TKey1 key1,TKey2 key2,out TValue value)
    //    {
    //        if (this.ContainsKey(key1,key2))
    //        {
    //            value = this[key1, key2];
    //            return true;
    //        }
    //        else
    //        {
    //            value = default(TValue);
    //            return false;
    //        }
    //    }

    //    public TValue TryGetValue(TKey1 key1,TKey2 key2)
    //    {
    //        return TryGetValue(key1, key2, default(TValue));
    //    }

    //    public TValue TryGetValue(TKey1 key1,TKey2 key2,TValue defaultValue)
    //    {
    //        TValue value;

    //        if (TryGetValue(key1,key2,out value))
    //        {
    //            return value;
    //        }
    //        else
    //        {
    //            return defaultValue;
    //        }
    //    }

    //    public KeysPair[] Keys{get { return lstKeyPair.ToArray(); }}

    //    public TValue[] Values
    //    {
    //        get { return cacheData.Values.ToArray(); }
    //    }

    //    #region "IEnumerable接口"

    //    public IEnumerator<KeysValuePair<TKey1, TKey2,TValue>> GetEnumerator()
    //    {
    //        foreach (var pair in lstKeyPair)
    //        {
    //            yield return new KeysValuePair<TKey1, TKey2, TValue>(pair.Key1,pair.Key2,this[pair.Key1,pair.Key2]);
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }

    //    #endregion

    //    public class KeysPair
    //    {
    //        public KeysPair(TKey1 key1, TKey2 key2)
    //        {
    //            Key1 = key1;
    //            Key2 = key2;
    //        }

    //        public TKey1 Key1 { set; get; }

    //        public TKey2 Key2 { set; get; }

    //        public bool Equals(KeysPair other)
    //        {
    //            if (ReferenceEquals(null, other)) return false;
    //            if (ReferenceEquals(this, other)) return true;
    //            return Equals(other.Key1, Key1) && Equals(other.Key2, Key2);
    //        }

    //        public override bool Equals(object obj)
    //        {
    //            if (ReferenceEquals(null, obj)) return false;
    //            if (ReferenceEquals(this, obj)) return true;
    //            if (obj.GetType() != typeof (KeysPair)) return false;
    //            return Equals((KeysPair) obj);
    //        }

    //        public override int GetHashCode()
    //        {
    //            unchecked
    //            {
    //                return (Key1.GetHashCode()*397) ^ Key2.GetHashCode();
    //            }
    //        }
    //    }

    //    protected long GetKeysHashCode(TKey1 key1, TKey2 key2)
    //    {
    //        unchecked
    //        {
    //            //var tp = new Tuple<string, string, int>();

    //            long key1Code = key1.GetHashCode();
    //            long key2Code = key2.GetHashCode();

    //            return  (((key1Code << 5) + key1Code) ^ key2Code);

    //            //return (key1.GetHashCode() * 397) ^ key2.GetHashCode();
    //        }
    //    }
    //}
}
