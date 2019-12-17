using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kugar.Core.BaseStruct
{
	/// <summary>
	/// 	一个可以将数组封装为List的包装器,支持在数组中的Add,Remove等操作,但最大Add的长度不允许超过源数组的长度 <br/>
	/// 	该包装器使用读写锁保证读写为线程安全
	/// </summary>
    public class ArrayToListWrapper<T> : IList<T>
    {
        private T[] _currentList = null;
        private int _currentIndex = 0;
        private int _minIndex = 0;
        private int _maxIndex = 0;
        private ReaderWriterLockSlim locker=new ReaderWriterLockSlim();
        
        /// <summary>
        /// 	构造函数,由包装器自己新建一个指定长度的数据
        /// </summary>
        /// <param name="length">指定新建的数组的长度</param>
        public ArrayToListWrapper(int length)
            : this(new T[length])
        {
        }

        /// <summary>
        /// 	指定数组一个源数组,并从0开始,直至数据长度
        /// </summary>
        /// <param name="data"></param>
        public ArrayToListWrapper(T[] data):this(data,0,data.Length)
        {
//            if (data == null)
//            {
//                throw new ArgumentNullException("data");
//            }
//
//            _currentList = data;
//            _minIndex = -1;
//            _maxIndex = data.Length - 1;
//            _currentIndex = _minIndex;
        }

        /// <summary>
        /// 	指定一个源数组,并指定数组的起始偏移和映射的长度
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public ArrayToListWrapper(T[] data, int startIndex, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (startIndex + count > data.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            _currentList = data;
            _minIndex = startIndex;
            _maxIndex = _minIndex + count-1;
            _currentIndex = _minIndex - 1;
        }

        public T[] Data
        {
            get { return _currentList; }
        }

        public void Add(T item)
        {
        	locker.EnterWriteLock();
        	
            var index = Interlocked.Increment(ref _currentIndex);

            if (index > _maxIndex)
            {
            	locker.ExitWriteLock();
                throw new ArgumentOutOfRangeException("item");
            }

            _currentList[index] = item;
            locker.ExitWriteLock();
        }

	    public void AddRange(IEnumerable<T> items)
	    {
	        foreach (var item in items)
	        {
	            this.Add(item);
	        }
	    }

	    public void AddRange(T[] items)
	    {
            
        	locker.EnterWriteLock();

            var index = Interlocked.Increment(ref _currentIndex);

            if (index > _maxIndex)
            {
                locker.ExitWriteLock();
                throw new ArgumentOutOfRangeException("item");
            }

            Array.Copy(items, 0, _currentList, index, items.Length);

            Interlocked.Add(ref _currentIndex, items.Length-1);

            locker.ExitWriteLock();
	    }


        public void Clear()
        {
        	locker.EnterWriteLock();
            Interlocked.Exchange(ref _currentIndex, _minIndex);
            locker.ExitWriteLock();
            //_currentIndex = _minIndex;
        }

        public bool Contains(T item)
        {
        	locker.EnterReadLock();
        	var ret=_currentList.Any(x => x .Equals(item));
    		locker.ExitReadLock();
            return ret;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        	locker.EnterReadLock();
        	try {
        		Array.Copy(_currentList,_minIndex,array,arrayIndex, _currentIndex+1);
        	} catch (Exception) {
        		
        		throw;
        	}
        	finally{
        		locker.ExitReadLock();
        	}
        	
        	
        }

        public int Count { 
        	get { 
        		
        		locker.EnterReadLock();
        		
        		var c=_maxIndex - _currentIndex +1;
        		
        		locker.ExitReadLock();
        		return c; 
        	}
        }

        public bool IsReadOnly { get { return false; } }

        public bool Remove(T item)
        {
        	if (item==null) {
        		return false;
        	}
        	
            var index = -1;

            locker.EnterWriteLock();
            
            for (int i = _minIndex; i < _currentIndex; i++)
            {
                if (_currentList[i] != null && _currentList[i].Equals(item))
                {
                    index = i;
                }
            }
            
            T[] tempBuff=new T[_maxIndex - index+1];
            
            try {
            	Array.Copy(_currentList,index,tempBuff,0,tempBuff.Length);
            	Array.Copy(tempBuff,_currentList,tempBuff.Length);
            	
            } catch (Exception) {
            	
            	throw;
            }
            finally{
            	locker.ExitWriteLock();
            }
            
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
        	locker.EnterReadLock();
        	
        	
    		for (int i = _minIndex; i <= _currentIndex; i++)
            {
                yield return _currentList[i];
            }
        	
    		locker.ExitReadLock();
        	
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
        	if (item==null) {
        		throw new ArgumentNullException("item");
        	}
        	
        	locker.EnterReadLock();
        	
            int tempIndex = _currentIndex;
			int retIndex=-1;
			
			var isSuccess=false;
            for (int i = _minIndex; i < tempIndex; i++)
            {
            	isSuccess=false;
                if (item != null && item.Equals(_currentList[i]))
                {
                	isSuccess=true;
                    //retIndex= i - _minIndex;
                }
                else if (_currentList[i] != null && _currentList[i].Equals(item))
                {
                	isSuccess=true;
                    //retIndex= i - _minIndex;
                }
                else if (_currentList[i].Equals(item))
                {
                	isSuccess=true;
                    //retIndex= i - _minIndex;
                }
                
                if (isSuccess) {
                	retIndex= i - _minIndex;
                }
            }

            locker.ExitReadLock();
            
            return retIndex;
        }

        public void Insert(int index, T item)
        {
            if (_currentIndex==_maxIndex)
            {
                throw new ArgumentOutOfRangeException("index","目标已满,无法插入");
            }

            locker.EnterWriteLock();
            
            Array.ConstrainedCopy(_currentList,index,_currentList,index+1,_currentIndex-index);

            _currentList[index] = item;
            
            locker.ExitWriteLock();
        }

        public void RemoveAt(int index)
        {
        	locker.EnterWriteLock();
        	
            var newIndex = index + _minIndex;

            var tempList = new T[_maxIndex-index-_minIndex];

            Array.Copy(_currentList,newIndex+1,tempList,0,_maxIndex-newIndex-1);

            Array.ConstrainedCopy(tempList,0,_currentList,newIndex,tempList.Length);
            
            locker.ExitWriteLock();
        }

        public T this[int index]
        {
            get {
        		locker.EnterReadLock();
        		
        		if (index+_minIndex>_maxIndex) {
        			throw new ArgumentOutOfRangeException("index");
        		}
        		
        		var item=_currentList[index + _minIndex];
        		
        		locker.ExitReadLock();
        		
        		return item; 
        	}
            set {
        		if (index+_minIndex>_maxIndex) {
        			throw new ArgumentOutOfRangeException("index");
        		}
        		
        		locker.EnterWriteLock();
        		
        		_currentList[index + _minIndex] = value; 
        		
        		locker.ExitWriteLock();
        	}
        }
    }
}
