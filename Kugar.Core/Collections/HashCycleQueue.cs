using System;
using System.Collections.Generic;
using System.Threading;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Collections
{
	/// <summary>
	/// 带哈希表的环形队列，保证入队的值是唯一的
	/// </summary>
	[Serializable]
	public class HashCircularQueue<T>:IDisposable,IEnumerable<T>
	{
		private HashSet<T> keyList =null;
		private T[] valueList=null;
		[NonSerializedAttribute]
		private System.Threading.ReaderWriterLockSlim locker=new System.Threading.ReaderWriterLockSlim();
		private int _front = 0;
        private int _rear = 0;
        private int _count = 0;
        private int _capacity=0;

        public HashCircularQueue(int capacity):this(capacity,null){}
		
		public HashCircularQueue(int capacity,IEqualityComparer<T> comparer)
		{
			_capacity=capacity;
			
			if (comparer==null) {
				keyList=new HashSet<T>();
			}
			else{
				keyList=new HashSet<T>(comparer);
			}
			
			valueList=new T[_capacity];
			
		}
		
		public bool EnQueue(T item)
		{
			locker.EnterWriteLock();

		    var isSuccess = false;

			if (keyList.Add(item)) {
				
				var indexStart = _rear;

	            _rear = (indexStart + 1) % _capacity;
	
	            if (_rear == _front)
	            {
	                _front = (++_front ) % _capacity;
	            }
	
	            valueList[indexStart] = item;
	
	            UpdateCount(1);

			    isSuccess = true;
			}
			
			locker.ExitWriteLock();

		    return isSuccess;
		}
		
		public T Dequeue()
        {
            if (_count<=0) {
				throw new ArgumentOutOfRangeException();
            }
			
            locker.EnterWriteLock();

            var indexStart = _front;

            _front = (_front + 1) % _capacity;

            UpdateCount(-1);

        	var ret=valueList[indexStart];
        	
        	locker.ExitWriteLock();
        	
        	return ret;

        }		
		
		public bool Contain(T item)
		{
			locker.EnterReadLock();
			
			var ret=keyList.Contains(item);
			
			locker.ExitWriteLock();
			
			return ret;
		}
		
		public void Clear()
		{
			locker.EnterWriteLock();

            if (valueList != null)
            {
                Array.Clear(valueList, 0, valueList.Length);
                keyList.Clear();
                _rear = 0;
                _front = 0;
                _count = 0;
            }

            

            locker.ExitWriteLock();
		}
		
		public int Capacity { get { return _capacity; } }
		
		public int Count
        {
            get
            {
                locker.EnterReadLock();

                var tempCount = _count;
                
                locker.ExitReadLock();

                return tempCount;
            }
        }
		

		
		private void UpdateCount(int updatedCount)
        {
            //readerWriterLock.EnterWriteLock();

            var tempCount = _count;

            tempCount += updatedCount;

            if (tempCount<0)
            {
                tempCount = 0;
            }

            if (tempCount>_capacity)
            {
                tempCount = _capacity;
            }

            Interlocked.Exchange(ref _count, tempCount);
        }
		
        
		[NonSerializedAttribute]
		private bool _disposed=false;
		[NonSerializedAttribute]
		private object _desposingObj=new object();
		public void Dispose()
		{
			if (!_disposed) {
				lock(_desposingObj)
				{
					if (!_disposed) {
		                Clear();            	
		                locker.Dispose();
		
		                valueList = null;
		                keyList=null;
		                
		                _disposed = true;
					}
				}
			}
			
		}
        
		
		public IEnumerator<T> GetEnumerator()
		{
			locker.EnterReadLock();

            var tempFront = ((_front-1) % _capacity).ToMinInt(0);
            var tempRear = _rear;

            if (_count>0)
            {
                do
                {
                    yield return valueList[tempFront];
                    tempFront = (tempFront + 1) % _capacity;
                } while (tempFront != tempRear);
                //while (tempFront != tempRear)
                //{

                //}                
            }



            locker.ExitReadLock();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
