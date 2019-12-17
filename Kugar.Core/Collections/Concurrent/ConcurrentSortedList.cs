using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kugar.Core.Collections.Concurrent
{
    public class ConcurrentSortedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable
    {
        //private ConcurrentDictionary<TKey, TValue> _cacheDic = new ConcurrentDictionary<TKey, TValue>();
        private SortedList<TKey, TValue> _sortedList = new SortedList<TKey, TValue>();
        private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ConcurrentSortedList(int capacity = 16, IComparer<TKey> comparer = null)
        {
            if (comparer != null)
            {
                //_cacheDic = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity, new IComparerToIEquality<TKey>(comparer));
                _sortedList = new SortedList<TKey, TValue>(capacity, comparer);
            }
            else
            {
                _sortedList = new SortedList<TKey, TValue>(capacity);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in _sortedList)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _sortedList.Count; }
        }

        public IList<TValue> Values
        {
            get { return _sortedList.Values; }
        }

        public IList<TKey> Keys
        {
            get { return _sortedList.Keys; }
        }

        public TValue GetByIndex(int index)
        {
            _readerWriterLock.EnterReadLock();

            if (index > _sortedList.Count-1)
            {
                _readerWriterLock.ExitReadLock();
                throw new ArgumentOutOfRangeException("index");
            }

            var item = _sortedList.Values[index];

            _readerWriterLock.ExitReadLock();

            return item;
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
            {
                throw new ArgumentException("已存在具有相同键的元素");
            }
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
            {
                _readerWriterLock.EnterWriteLock();

                try
                {
                    if (_sortedList.ContainsKey(key))
                    {
                        _sortedList[key] = value;
                    }
                    //_cacheDic[key] = value;

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
            }
        }

        public TValue Remove(TKey key)
        {
            TValue value;

            TryRemove(key, out value);

            return value;
        }

        public TValue RemoveAt(int index)
        {
            TValue value;

            TryRemoveAt(index, out value);

            return value;
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();

            _sortedList.Clear();
            //_cacheDic.Clear();

            _readerWriterLock.ExitWriteLock();
        }


        public bool TryAdd(TKey item, TValue value)
        {
            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(item))
            {
                //_readerWriterLock.ExitUpgradeableReadLock();
                retValue = false;
            }
            else
            {

                _readerWriterLock.EnterWriteLock();

                _sortedList.Add(item, value);

                _readerWriterLock.ExitWriteLock();

                retValue = true;
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;
            //if (_cacheDic.TryAdd(item, value))
            //{
            //    _readerWriterLock.EnterWriteLock();
            //    _sortedList.Add(item, value);
            //    _readerWriterLock.ExitWriteLock();
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

        }

        public bool TryAddForFactory(TKey key,Func<TValue> valueFactory)
        {
            if (valueFactory==null)
            {
                throw new ArgumentNullException("valueFactory");
            }

            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(key))
            {
                //_readerWriterLock.ExitUpgradeableReadLock();
                retValue = false;
            }
            else
            {

                _readerWriterLock.EnterWriteLock();

                try
                {
                    _sortedList.Add(key,valueFactory.Invoke() );
                    retValue = true;
                }
                catch (Exception)
                {
                    retValue = false;
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;
        }

        public bool TryRemove(TKey item, out TValue oldValue)
        {
            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(item))
            {
                oldValue = _sortedList[item];

                _readerWriterLock.EnterWriteLock();

                retValue = _sortedList.Remove(item);

                _readerWriterLock.ExitWriteLock();

            }
            else
            {
                oldValue = default(TValue);
                retValue = false;
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;

        }

        public bool TryRemoveAt(int index, out TValue oldValue)
        {
            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (index > _sortedList.Count - 1)
            {
                oldValue = default(TValue);
                retValue = false;
            }
            else
            {

                oldValue = _sortedList.Values[index];

                _readerWriterLock.EnterWriteLock();

                try
                {
                    _sortedList.RemoveAt(index);
                    retValue = true;
                }
                catch (Exception)
                {
                    retValue = false;
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }

            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;

            //var t = _sortedList.Keys[index];

            //_cacheDic.TryRemove(t, out oldValue);

            //_readerWriterLock.ExitWriteLock();

            //return retValue;
        }

        public bool TryGetValue(TKey item, out TValue value)
        {
            return _sortedList.TryGetValue(item, out value);

            // return _cacheDic.TryGetValue(item, out value);
        }

        public bool TryUpdate(TKey item, TValue value, TValue comparsionValue)
        {
            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(item))
            {
                var oldValue = _sortedList[item];

                var isequal = false;

                if (value is ValueType)
                {
                    isequal = value.Equals(comparsionValue);
                }
                else
                {
                    var newobj = (object)oldValue;
                    var newcom = (object)comparsionValue;

                    if (newobj == null && newcom == null)
                    {
                        isequal = true;
                    }
                    else if (newobj != null)
                    {
                        isequal = newobj.Equals(newcom);
                    }
                    else if (newcom != null)
                    {
                        isequal = newcom.Equals(newobj);
                    }

                }

                if (isequal)
                {
                    _readerWriterLock.EnterWriteLock();

                    _sortedList[item] = value;

                    _readerWriterLock.ExitWriteLock();

                    retValue = true;
                }
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;

            //if (_cacheDic.TryUpdate(item, value, comparsionValue))
            //{
            //    _readerWriterLock.EnterWriteLock();

            //    _sortedList[item] = value;

            //    _readerWriterLock.ExitWriteLock();

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        public bool TryUpdateForFactory(TKey key, Action<TValue> updateMethod, Func<TValue, bool> compareMethod)
        {
            if (updateMethod == null)
            {
                throw new ArgumentNullException("valueFactory");
            }

            if (compareMethod == null)
            {
                throw new ArgumentNullException("compareMethod");
            }

            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(key))
            {
                var oldValue = _sortedList[key];

                var isequal = false;

                try
                {
                    isequal = compareMethod.Invoke(oldValue);
                }
                catch (Exception)
                {
                    isequal = false;
                }

                if (isequal)
                {
                    _readerWriterLock.EnterWriteLock();

                    try
                    {
                        updateMethod.Invoke(oldValue);
                        retValue = true;
                    }
                    catch (Exception)
                    {
                        retValue = false;
                    }
                    finally
                    {
                        _readerWriterLock.ExitWriteLock();
                    }

                }
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;
        }

        public bool TryUpdateForFactory(TKey key, Func<TValue> valueFactory, Func<TValue, bool> compareMethod)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }

            if (compareMethod == null)
            {
                throw new ArgumentNullException("compareMethod");
            }

            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(key))
            {
                var oldValue = _sortedList[key];

                var isequal = false;

                try
                {
                    isequal = compareMethod.Invoke(oldValue);
                }
                catch (Exception)
                {
                    isequal = false;
                }

                if (isequal)
                {
                    _readerWriterLock.EnterWriteLock();

                    try
                    {
                        _sortedList[key]= valueFactory.Invoke();
                        retValue = true;
                    }
                    catch (Exception)
                    {
                        retValue = false;
                    }
                    finally
                    {
                        _readerWriterLock.ExitWriteLock();
                    }

                }
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;
        }

        public bool TryUpdateNoCompare(TKey item, TValue value)
        {
            var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(item))
            {
                _readerWriterLock.EnterWriteLock();

                _sortedList[item] = value;

                _readerWriterLock.ExitWriteLock();

                retValue = true;
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;
        }

        public bool TryUpdateNoCompareForFactory(TKey key, Func<TValue> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }

             var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(key))
            {
                //var item = _sortedList[key];

                _readerWriterLock.EnterWriteLock();

                try
                {
                    var item=valueFactory.Invoke();

                    _sortedList[key] = item;

                    retValue = true;
                }
                catch (Exception)
                {
                    retValue = false;
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
                
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;


        }

        public bool TryUpdateNoCompareForFactory(TKey key, Action<TValue> updateMethod)
        {
            if (updateMethod == null)
            {
                throw new ArgumentNullException("updateMethod");
            }

             var retValue = false;

            _readerWriterLock.EnterUpgradeableReadLock();

            if (_sortedList.ContainsKey(key))
            {
                var item = _sortedList[key];

                _readerWriterLock.EnterWriteLock();

                try
                {
                    updateMethod.Invoke(item);

                    //_sortedList[key] = item;

                    retValue = true;
                }
                catch (Exception)
                {
                    retValue = false;
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
                
            }

            _readerWriterLock.ExitUpgradeableReadLock();

            return retValue;


        }

        public void CopyValueTo(TValue[] array, int index)
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                _sortedList.Values.CopyTo(array, index);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {


            _readerWriterLock.EnterReadLock();

            var items = _sortedList.ToArray();


            _readerWriterLock.ExitReadLock();

            return items;
        }
    }

    public class IComparerToIEquality<T> : IEqualityComparer<T>
    {
        private IComparer<T> _comparer = null;

        public IComparerToIEquality(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _comparer.Compare(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
