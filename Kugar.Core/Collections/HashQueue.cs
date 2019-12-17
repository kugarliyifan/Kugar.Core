using System;
using System.Collections.Generic;

namespace Kugar.Core.Collections
{
	/// <summary>
	/// 带哈希表的队列，保证入队的值是唯一的
	/// </summary>
	[Serializable]
	public class HashQueue<T>:IDisposable, IEnumerable<T>
	{
		private HashSet<T> keyLst=null;
		private LinkedList<T> valueLst=null;
		[NonSerializedAttribute]
		private System.Threading.ReaderWriterLockSlim locker=new System.Threading.ReaderWriterLockSlim();
		
		public HashQueue():this(null){}
		
		public HashQueue(IEqualityComparer<T> comparer)
		{
			if (comparer==null) {
				keyLst=new HashSet<T>();
			}
			else{
				keyLst=new HashSet<T>(comparer);
			}
			
			valueLst=new LinkedList<T>();
		}
		
		public T this[int index]
		{
			get{
				locker.EnterReadLock();
				
				if (index>=this.Count) {
					throw new ArgumentOutOfRangeException("index");
				}
				
				var node=valueLst.First;
				
				for (int i = 0; i < index; i++) {
					node=node.Next;	
				}
				
				locker.ExitReadLock();
				
				return node.Value;
			}
		}
		
		public bool EnQueue(T item)
		{
			locker.EnterWriteLock();

		    var isSuccess = false;

			if (keyLst.Add(item)) {
				valueLst.AddLast(item);
			    isSuccess = true;
			}

			locker.ExitWriteLock();

		    return isSuccess;
		}
		
		public T DeQueue()
		{
			locker.EnterWriteLock();
			
			if (valueLst.Count<=0) {
				locker.ExitWriteLock();
				throw new ArgumentOutOfRangeException();
			}
			
			var ret=valueLst.First;
			
			valueLst.RemoveFirst();
			
			keyLst.Remove(ret.Value);
			
			locker.ExitWriteLock();
			
			return ret.Value;
				
		}
		
        public void Remove(T key)
        {
            locker.EnterWriteLock();

            try
            {
                if(keyLst.Remove(key))
                {
                    valueLst.Remove(key);
                }
            }
            catch (Exception)
            {
                
            }


            locker.ExitWriteLock();
        }

		public bool Contain(T item)
		{
			locker.EnterReadLock();
			
			var ret= keyLst.Contains(item);
			
			locker.ExitReadLock();
			
			return ret;
		}		
		
		public int Count
		{
			get{
				//locker.EnterReadLock();
				
				var c=keyLst.Count;
				
				//locker.ExitReadLock();
				
				return c;
			}
		}
		
		public void Clear()
		{
			
		}
		
		
		public IEnumerator<T> GetEnumerator()
		{
			locker.EnterReadLock();
			
			if (keyLst.Count<=0) {
				yield break;
			}
			
			var node=valueLst.First;
			
			while(node!=null){
				yield return node.Value;
				
				node=node.Next;
			}
			
			locker.ExitReadLock();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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
						keyLst=null;
						valueLst=null;
				
						_disposed=true;
					}
				}
			}
			
		}
	}
}
