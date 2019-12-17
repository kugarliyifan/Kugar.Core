using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Kugar.Core.Collections;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{
    public class MultiTypeKeyDictionary<TValue>
    {
        private ReaderWriterLockSlim _locker=new ReaderWriterLockSlim();

        private Dictionary<TValue, HashSet<object>> _valueHashSet = new Dictionary<TValue, HashSet<object>>();

        private MutileKeysDictionary<Type,object,TValue> _dictionaryData=new MutileKeysDictionary<Type, object, TValue>();

        public void Add<TKey>(TKey key, TValue value)
        {
            var type = typeof(TKey);

            _locker.EnterWriteLock();

            var valueList = _valueHashSet.TryGetValue(value);

            if (valueList==null)
            {
                valueList=new HashSet<object>();
                _valueHashSet.Add(value, valueList);
            }

            if (!valueList.Add(key))
            {
                throw new ArgumentOutOfRangeException("key","已存在相同的键");
            }

            try
            {
                _dictionaryData.Add(typeof(TKey), key, value);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void Remove<TKey>(TKey key)
        {
            var type = typeof (TKey);

            removeInternal(type, key);
        }

        public void Remove(object key)
        {
            var type = key.GetType();

            removeInternal(type, key);
        }

        public TValue this[object key]
        {
            get
            {
                _locker.EnterReadLock();

                if (_dictionaryData.ContainsKey(key.GetType(), key))
                {
                    _locker.ExitReadLock();
                    throw new ArgumentOutOfRangeException("key", "不存在指定的Key");
                }

                var value = _dictionaryData.TryGetValue(key.GetType(), key);

                _locker.ExitReadLock();

                return value;
            }
            set
            {
                var keylist = getKeyValueDic(key.GetType());

                if (keylist == null)
                {
                    throw new ArgumentOutOfRangeException("key", "不存在指定的Key");
                }

                _locker.EnterWriteLock();

                try
                {
                    if (keylist.ContainsKey(key))
                    {
                        throw new ArgumentOutOfRangeException("key", "不存在指定的Key");
                    }

                    var oldValue = keylist[key];

                    var valueKeyList = _valueHashSet.TryGetValue(oldValue);

                    _valueHashSet.Remove(oldValue);

                    _valueHashSet.Add(value,valueKeyList);

                    foreach (var keyItem in valueKeyList)
                    {
                        var keyDic = getKeyValueDic(keyItem.GetType());

                        if (keyDic.ContainsKey(keyItem))
                        {
                            keyDic[keyItem] = value;
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        public TValue TryGetValue<TKey>(TKey key, TValue defaultValue = default(TValue))
        {
            _locker.EnterReadLock();

            TValue value;

            if (_dictionaryData.ContainsKey(typeof(TKey), key))
            {
               
                value= _dictionaryData.TryGetValue(typeof (TKey), key);
            }
            else
            {
                value= defaultValue;
            }

            _locker.ExitReadLock();

            return value;
        }

        public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator<TKey>()
        {
            var lst = _dictionaryData.TryGetValue(typeof (TKey));

            foreach (var pair in lst)
            {
                yield return new KeyValuePair<TKey, TValue>((TKey)pair.Key,pair.Value);
            }
        }

        public IEnumerator<KeyValuePair<object, TValue>> GetEnumerator(Type type)
        {
            var lst = _dictionaryData.TryGetValue(type);

            foreach (var pair in lst)
            {
                yield return pair;
            }
        }

        public bool ContainValue(TValue value)
        {
            return _valueHashSet.ContainsKey(value);
        }

        public bool ContainKey(object key)
        {
            _locker.EnterReadLock();

            var b = _dictionaryData.ContainsKey(key.GetType(),key);

            _locker.ExitReadLock();

            return b;
        }

        public bool ContainKey<TKey>(TKey key)
        {
            _locker.EnterReadLock();

            var b= _dictionaryData.ContainsKey(typeof (TKey), key);

            _locker.ExitReadLock();

            return b;
            
        }


        private Dictionary<object, TValue> getKeyValueDic(Type type)
        {
            var lst = _dictionaryData.TryGetValue(type);

            return lst;
        }

        private void removeInternal(Type keyType, object key)
        {

            _locker.EnterWriteLock();

            try
            {
                var value = _dictionaryData.TryGetValue(keyType, key);

                var keyList = _valueHashSet.TryGetValue(value);

                foreach (var keyV in keyList)
                {
                    var l = _dictionaryData.TryGetValue(keyV.GetType());

                    l.Remove(keyV);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

    }
}
