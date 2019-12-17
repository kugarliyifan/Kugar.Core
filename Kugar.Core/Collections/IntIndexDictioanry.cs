using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Collections
{
    public class IntIndexDictioanry<TValue>:IEnumerable<KeyValuePair<int,TValue>>
    {
        private ConcurrentDictionary<int, TValue> _cacheValue = null;
        private ReaderWriterLockSlim lockSlim=new ReaderWriterLockSlim();
        private int _index;

        public IntIndexDictioanry(int startIndex=0)
        {
            _cacheValue = new ConcurrentDictionary<int, TValue>();
            _index = startIndex;
        }

        public TValue this[int key]
        {
            get
            {
                TValue v;
                lockSlim.EnterReadLock();

                v = _cacheValue[key];

                lockSlim.ExitReadLock();

                return v;
            }
            set
            {
                lockSlim.EnterWriteLock();

                _cacheValue[key] = value;

                lockSlim.ExitWriteLock();
            }
        }

        public int TaskItem(TValue value)
        {
            var testCount = 0;

            lockSlim.EnterWriteLock();

            var tempIndex = _index+1;
            var isSuccess = false;
            var testMaxCount = int.MaxValue - _cacheValue.Count;


            while (testCount < testMaxCount)
            {
                if (!_cacheValue.ContainsKey(tempIndex))
                {
                    isSuccess = true;
                    break;
                }
                else
                {
                    testCount += 1;

                    unchecked
                    {
                        tempIndex += 1;
                    }
                }
            }

            if (isSuccess)
            {
                Interlocked.Exchange(ref _index, tempIndex);

                _cacheValue.TryAdd(tempIndex, value);

                //_cacheValue.Add(tempIndex,value);
            }

            lockSlim.ExitWriteLock();

            return tempIndex;
        }

        public bool ContainsID(int id)
        {
            //lockSlim.EnterReadLock();

            return _cacheValue.ContainsKey(id);

            //lockSlim.ExitReadLock();

            //return s;
        }

        public void ReturnValue(int key)
        {
            lockSlim.EnterWriteLock();

            _cacheValue.TryRemove(key);

            lockSlim.ExitWriteLock();
        }

        public bool AddValue(int key, TValue value)
        {
            bool s = false;

            lockSlim.EnterWriteLock();

            s = _cacheValue.TryAdd(key, value);

            lockSlim.ExitWriteLock();

            return s;
        }

        public TValue AddOrUpdate(int key, TValue value)
        {
            lockSlim.EnterWriteLock();

            TValue oldValue = default(TValue);

            if (_cacheValue.ContainsKey(key))
            {
                oldValue=_cacheValue[key] ;
                _cacheValue[key] = value;
            }
            else
            {
                _cacheValue.TryAdd(key,value);
            }

            lockSlim.ExitWriteLock();

            return oldValue;
        }

        public void Clear()
        {
            lockSlim.EnterWriteLock();

            _cacheValue.Clear();

            _index = 0;

            lockSlim.ExitWriteLock();
        }

        public int Count
        {
            get { return _cacheValue.Count; }
        }

        public bool TryAdd(int key,TValue value)
        {
            //var retValue = true;

            //lockSlim.EnterWriteLock()

            return _cacheValue.TryAdd(key, value);


            //lockSlim.ExitWriteLock();
        }

        public bool TryUpdate(int key,TValue value,TValue comparisonValue)
        {
            return _cacheValue.TryUpdate(key, value, comparisonValue);
        }

        public bool TryGetValue(int key,out TValue value)
        {
            lockSlim.EnterReadLock();

            var v = _cacheValue.TryGetValue(key, out value);

            lockSlim.ExitReadLock();

            return v;
        }

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            //lockSlim.EnterReadLock();

            if (_cacheValue.Count>0)
            {
                foreach (var value in _cacheValue)
                {
                    yield return value;
                }
            }
            else
            {
                yield break;
            }
            

            //lockSlim.ExitReadLock();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
