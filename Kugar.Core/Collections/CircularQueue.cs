using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Collections
{
    /// <summary>
    /// 环形队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularQueue<T> : IEnumerable<T>, IDisposable
    {
        private int _capacity = 0;
        //private object _lockerObj = new object();
        private T[] _dataList = null;
        private int _front = 0;
        private int _rear = 0;
        private int _count = 0;
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public CircularQueue(int capacity)
        {
            if (capacity<=0)
            {
                throw new ArgumentOutOfRangeException("cap");
            }

            _dataList=new T[capacity];

            _front = 0;
            _rear = 0;
            _count = 0;

            _capacity = capacity;
        }

        public T Enqueue(T item)
        {
            readerWriterLock.EnterWriteLock();

            var indexStart = _rear;

            _rear = (indexStart + 1) % _capacity;

            if (_rear == _front)
            {
                _front = (++_front ) % _capacity;
            }

            var oldItem = _dataList[indexStart];

            _dataList[indexStart] = item;

            UpdateCount(1);

            readerWriterLock.ExitWriteLock();

            return oldItem;
        }

        public void Enqueue(T[] src, int offset, int count)
        {

            var leftCount = _capacity - count;

            var copyCount = count;
            var newFront = _rear;
            var newRear = _front;

            if (count>leftCount)
            {
                copyCount = ((count%leftCount) + leftCount)%_capacity - 1;
                newFront = count + 1;
                newRear = copyCount;
                var copyStartIndex = newRear - count;
                if (copyCount==0)
                {
                    copyCount =_capacity;
                }
                else
                {
                    Array.Clear(_dataList,0,_dataList.Length);
                }

                Array.Copy(src,offset+count-copyCount,_dataList,copyStartIndex,copyCount);
            }
            else
            {
                newRear = (newRear + copyCount) % _capacity;

                if (newRear == newFront)
                {
                    newFront= (newRear+ 1) % _capacity;
                }
            }


            if (count <= 0 || count > _capacity)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (!src.IsInEnableRange(offset, count))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            readerWriterLock.EnterWriteLock();

            var indexStart = _rear;

            _rear = (indexStart + count) % _capacity;

            if (_rear == _front)
            {
                _front= (_front+ count) % _capacity;
            }


            UpdateCount(count);

            var realEnd = indexStart + count;

            if (realEnd > _capacity)
            {
                var tempCount = realEnd - _capacity;

                Array.Copy(src, offset, _dataList, indexStart, tempCount);

                Array.Copy(src, offset + tempCount, _dataList, indexStart + tempCount, count - tempCount);
            }
            else
            {
                Array.Copy(src, offset, _dataList, indexStart, count);
            }

            readerWriterLock.ExitWriteLock();


            //_rear = _rear%_maxCount;

            ////Node<T> newNode = new Node<T>(item, null);
            //while (true)
            //{
            //    int curTail = _rear;
            //    int residue = _rear+1;

            //    //判断_tail是否被其他process改变
            //    if (curTail == _rear)
            //    {
            //        //A 有其他process执行C成功，_tail应该指向新的节点
            //        if (residue == null)
            //        {
            //            //C 如果其他process改变了tail.next节点，需要重新取新的tail节点
            //            if (Interlocked.CompareExchange<Node<T>>(
            //                ref curTail.Next, newNode, residue) == residue)
            //            {
            //                //D 尝试修改tail
            //                Interlocked.CompareExchange<Node<T>>(ref _tail, newNode, curTail);
            //                return;
            //            }
            //        }
            //        else
            //        {
            //            //B 帮助其他线程完成D操作
            //            Interlocked.CompareExchange<Node<T>>(ref _tail, residue, curTail);
            //        }
            //    }
            //}
        }

        public void Enqueue(IList<T> src, int offset, int count)
        {
            if (count <= 0 || count > _capacity)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (!src.IsInEnableRange(offset, count))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            readerWriterLock.EnterWriteLock();

            var indexStart = _rear;

            _rear = (indexStart + count) % _capacity;

            if (_rear==_front)
            {
                _front = (_front+count) % _capacity;
            }

            UpdateCount(count);

            for (int i = 0; i < count; i++)
            {
                _dataList[indexStart] = src[offset + i];

                indexStart = (indexStart + 1) % _capacity;
            }

            readerWriterLock.ExitWriteLock();
        }

        public void Enqueue(IEnumerable<T> src)
        {
            if (src == null)
            {
                throw new ArgumentOutOfRangeException("src");
            }

            readerWriterLock.EnterWriteLock();

            var indexStart = _rear;

            var tempCount = 0;

            foreach (var t in src)
            {
                _dataList[indexStart] = t;

                indexStart = (indexStart + 1) % _capacity;
                _rear = (indexStart + 1) % _capacity;

                if (_rear == _front)
                {
                    _front = (++_front) % _capacity;
                }

                tempCount++;
            }

            UpdateCount(tempCount);

            readerWriterLock.ExitWriteLock();
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

            //readerWriterLock.ExitWriteLock();
        }

        public T Dequeue()
        {
            return Dequeue(1)[0];
        }

        //public T[] Dequeue(int count)
        //{
        //    if (count <= 0 || count > _capacity)
        //    {
        //        throw new ArgumentOutOfRangeException("count");
        //    }

        //    var tempList = new T[count];

        //    readerWriterLock.EnterWriteLock();

        //    var indexStart = _front;

        //    _front = (_front + count) % _capacity;

        //    UpdateCount(-1*count);

        //    try
        //    {
        //        Array.Copy(_dataList, indexStart, tempList, 0, count);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        readerWriterLock.ExitWriteLock();
        //    }

        //    return tempList;
        //}

        public T[] Dequeue(int count)
        {
            T[] ret = null;

            if (TryDequeue(count,out ret))
            {
                return ret;
            }
            else
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }

        public T[] Dequeue(Predicate<T> checker)
        {
            var readCount = 0;
            
            readerWriterLock.EnterReadLock();

            var flag = false;

            foreach (var item in this)
            {
                readCount++;
                flag = checker(item);
                if (flag)
                {
                    break;
                }
            }

            readerWriterLock.ExitReadLock();

            if (!flag || readCount <= 0)
            {
                //readerWriterLock.ExitReadLock();
                return null;
            }
            else
            {
                //readerWriterLock.EnterUpgradeableReadLock();

                var lst = Dequeue(readCount);

                //readerWriterLock.ExitUpgradeableReadLock();
                
                //readerWriterLock.ExitWriteLock();
                return lst;
            }
        }

        public bool TryDequeue(out T item)
        {
            item = default(T);

            var retValue = false;

            readerWriterLock.EnterWriteLock();

            if (_count>0)
            {
                var indexStart = _front;

                _front = (_front + 1) % _capacity;

                UpdateCount(-1);

                item = _dataList[indexStart];

                retValue = true;
            }

            readerWriterLock.ExitWriteLock();

            return retValue;

            //T[] t = null;

            //if (TryDequeue(1,out t))
            //{
            //    item = t[0];
            //    return true;
            //}
            //else
            //{
            //    item = default(T);
            //    return false;
            //}
        }

        public bool TryDequeue(int count,out T[] items)
        {
            if (count <= 0 || count > _capacity )
            {
                //throw new ArgumentOutOfRangeException("count");
                items = null;
                return false;
            }

            var retValue = false;

            var tempList = new T[count];

            readerWriterLock.EnterWriteLock();

            if (_count>0)
            {
                var indexStart = _front;

                _front = (_front + count) % _capacity;

                UpdateCount(-1 * count);

                try
                {
                    Array.Copy(_dataList, indexStart, tempList, 0, count);

                    items = tempList;

                    retValue = true;     
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    readerWriterLock.ExitWriteLock();
                }

           
            }
            else
            {
                items = null;
                retValue = false;
                readerWriterLock.ExitWriteLock();
            }

            return retValue;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index > _count || index > _capacity)
                {
                    //readerWriterLock.ExitReadLock();
                    throw new ArgumentOutOfRangeException("index");
                }

                readerWriterLock.EnterReadLock();

                var item = _dataList[_front + index];

                readerWriterLock.ExitReadLock();

                return item;
            }
            set
            {
                if (index < 0 || index > _rear - _front || index > _capacity)
                {
                    //readerWriterLock.ExitReadLock();
                    throw new ArgumentOutOfRangeException("index");
                }

                readerWriterLock.EnterWriteLock();

                _dataList[_front + index] = value;

                readerWriterLock.ExitReadLock();

            }
        }

        public int Capacity { get { return _capacity; } }

        public T[] Clear()
        {
            List<T> tempList = null;
            readerWriterLock.EnterWriteLock();

            if (_dataList != null)
            {
                T d;

               tempList = new List<T>(_count); 

                while (this.TryDequeue(out d))
                {
                    tempList.Add(d);
                }

                Array.Clear(_dataList, 0, _dataList.Length);

                _rear = 0;
                _front = 0;
                _count = 0;
            }

            readerWriterLock.ExitWriteLock();


            if (tempList!=null && tempList.Count>0)
            {
                return tempList.ToArray();
            }
            else
            {
                return null;
            }
        }

        public int Count
        {
            get
            {
                readerWriterLock.EnterReadLock();

                var tempCount = _count;

                //if (_rear==_front && _count<=0)
                //{
                //    if (_count<=0)
                //    {
                //        tempCount= 0;
                //    }
                //    else
                //    {
                //        tempCount= _maxCount;
                //    }
                //}
                //else
                //{
                //    tempCount= _rear - _front;
                //}
                
                readerWriterLock.ExitReadLock();

                return tempCount;
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            readerWriterLock.EnterReadLock();

            var tempFront = ((_front-1) % _capacity).ToMinInt(0);
            var tempRear = _rear;

            if (_count>0)
            {
                do
                {
                    yield return _dataList[tempFront];
                    tempFront = (tempFront + 1) % _capacity;
                } while (tempFront != tempRear);
                //while (tempFront != tempRear)
                //{

                //}                
            }



            readerWriterLock.ExitReadLock();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IDisposable

        private bool _isDisposed = false;
        public void Dispose()
        {
            if (!_isDisposed)
            {
                readerWriterLock.Dispose();

                Clear();

                _dataList = null;

                _isDisposed = true;
            }


        }

        #endregion
    }
}
