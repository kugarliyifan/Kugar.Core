using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kugar.Core.BaseStruct;
using System.Linq;
using System.Collections.Specialized;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     一个扩展了功能的线程安全的双向链表，比原有LinkedList增加了 CutTo，CutFrom与CutBetween 函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedListEx<T> : IEnumerable<LinkNodeEx<T>>,INotifyCollectionChanged<LinkNodeEx<T>>,INotifyCollectionChanged
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
        private LinkNodeEx<T> _first = null;
        private LinkNodeEx<T> _rear = null;
        private int count = 0;
        private Func<T, T, bool> compare = null;
        private LinkNodeExPool<T> _pool = null;

        public LinkedListEx()
        {
            var tType = typeof(T);

            if (tType.IsAssignableFrom(typeof(IComparable<T>)))
            {
                compare = (value1, value2) => ((IComparable<T>)value1).CompareTo(value2) == 0;
            }
            else if (tType.IsAssignableFrom(typeof(IComparable)))
            {
                compare = (value1, value2) => ((IComparable)value1).CompareTo(value2) == 0;
            }
            else
            {
                compare = (value1, value2) => value1.Equals(value2);
            }
        }

        public LinkedListEx(IEnumerable<T> lst):this(null,lst)
        {
        }

        public LinkedListEx(LinkNodeExPool<T> pool)
            : this(pool, null)
        {
        }

        public LinkedListEx(LinkNodeExPool<T> pool,IEnumerable<T> lst)
        {
            _pool = pool;

            if (lst != null)
            {
                foreach (var v in lst)
                {
                    AddLast(v);
                }
            }
        }

        /// <summary>
        ///     返回链表的头节点
        /// </summary>
        public LinkNodeEx<T> First
        {
            get
            {
                LinkNodeEx<T> ret = null;
                lockSlim.EnterReadLock();

                ret = _first;

                lockSlim.ExitReadLock();

                return ret;
            }
        }

        /// <summary>
        ///     返回链表的尾节点
        /// </summary>
        public LinkNodeEx<T> Last
        {
            get
            {
                LinkNodeEx<T> ret = null;
                lockSlim.EnterReadLock();

                ret = _rear;

                lockSlim.ExitReadLock();

                return ret;
            }
        }

        /// <summary>
        ///     将从头节点开始，至node节点之间的所有节点截取出来，并返回截取部分的起始节点
        /// </summary>
        /// <param name="node">截取的结束节点</param>
        /// <returns>返回截取部分的起始节点</returns>
        public T[] CutTo(LinkNodeEx<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != this)
            {
                throw new InvalidOperationException("node 不在当前 LinkedListEx<T>中");
            }

            lockSlim.EnterWriteLock();

            var ret = _first;

            var tempCurrentNode = _first;
            var cutCount = 0;
            while (tempCurrentNode!=null)
            {
                cutCount += 1;

                tempCurrentNode = tempCurrentNode.Next;
            }

            //_first.Previous.Next = null;

            _first = node.Next;

            if (_first!=null)
            {
                _first.Previous = null;

                if (_first.Next==null)
                {
                    count = 1;
                }
                else
                {
                    count -= cutCount;
                }
            }
            else
            {
                _first = null;
                _rear = null;
                count = 0;
            }

            lockSlim.ExitWriteLock();

            var tlist = new List<T>();

            var tempNode = ret;
            LinkNodeEx<T> t = null;
            while (tempNode != null)
            {
                t = tempNode.Next;

                tlist.Add(tempNode.Value);

                tempNode.Release();

                tempNode = t;
            }

            return tlist.ToArray();
        }

        /// <summary>
        ///     将从node节点开始，至末端节点之间的所有节点截取出来，并返回截取部分的起始节点
        /// </summary>
        /// <param name="node">截取的起始节点</param>
        /// <returns>返回截取部分的起始节点</returns>
        public T[] CutFrom(LinkNodeEx<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != this)
            {
                throw new InvalidOperationException("node 不在当前 LinkedListEx<T>中");
            }

            lockSlim.EnterReadLock();

            //var oldRear = _rear;
            var ret = node;

            node.Next = null;

            _rear = node.Previous;
            _rear.Next = null;
            node.Previous = null;

            if (_rear!=null)
            {
                if (_rear.Previous==null)
                {
                    _first = _rear;
                    count = 1;
                }
                else
                {
                    count-=node.Count();
                }
            }
            else
            {
                _first = null;
                _rear = null;
                count = 0;
            }


            lockSlim.ExitWriteLock();

            ret.Previous = null;


            var tlist = new List<T>();

            var tempNode = ret;
            LinkNodeEx<T> t = null;
            while (tempNode != null)
            {
                t = tempNode.Next;

                tlist.Add(tempNode.Value);

                tempNode.Release();

                tempNode = t;
            }

            return tlist.ToArray();


            //var tempNode = ret;

            //while (tempNode != null)
            //{
            //    ret.Link = null;
            //    tempNode = tempNode.Next;
            //}

            //return ret;
        }

        /// <summary>
        ///     将从startNode节点开始，至endNode节点之间的所有节点截取出来，并返回截取部分的起始节点
        /// </summary>
        /// <param name="startNode">截取的起始节点</param>
        /// <param name="endNode">截取的结束节点</param>
        /// <returns>返回截取部分的起始节点</returns>
        public T[] CutBetween(LinkNodeEx<T> startNode, LinkNodeEx<T> endNode)
        {
            if (startNode == null)
            {
                throw new ArgumentNullException("startNode");
            }

            if (startNode.Link != this)
            {
                throw new InvalidOperationException("startNode 不在当前 LinkedListEx<T>中");
            }

            if (endNode == null)
            {
                throw new ArgumentNullException("endNode");
            }

            if (endNode.Link != this)
            {
                throw new InvalidOperationException("endNode 属于另一个 LinkedListEx<T>");
            }

            T[] retList = null;

            lockSlim.EnterWriteLock();

            if (startNode == endNode)  //如果起始与结束一致，则移除一个节点
            {
                retList=new T[]{startNode.Value};

                removeNode(startNode,false);
            }
            else
            {
                var tempNode = startNode;
                var isSuccess = false;
                while (tempNode != null)
                {
                    if (tempNode == endNode)
                    {
                        isSuccess = true;
                        break;
                    }
                    tempNode = tempNode.Next;
                }

                if (!isSuccess) //如果endNode指定的节点在startNode之后，则需要调换两个节点
                {
                    var t = startNode;

                    startNode = endNode;

                    endNode = t;
                }

                startNode.Previous.Next = endNode.Next;
                endNode.Next.Previous = startNode.Previous;

                startNode.Previous = null;
                endNode.Next = null;

                var tlist = new List<T>();

                var tempNode1 = startNode;
                LinkNodeEx<T> t1 = null;
                while (tempNode1 != null)
                {
                    t1 = tempNode1.Next;

                    tlist.Add(tempNode1.Value);

                    tempNode1.Release();

                    tempNode1 = t1;
                }

                retList = tlist.ToArray();

            }

            lockSlim.ExitWriteLock();



            return retList;


            //var tempNode1 = startNode;

            //while (tempNode1 != null)
            //{
            //    tempNode1.Link = null;
            //    tempNode1 = tempNode1.Next;
            //}

            //return startNode;
        }

        public void AddLast(LinkNodeEx<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != null)
            {
                if (node.Link == this)
                {
                    throw new InvalidOperationException("node已存在于当前的链表中，请不要重复添加");
                }
                else
                {
                    throw new InvalidOperationException("node 属于另一个 LinkedListEx<T>");
                }
            }

            lockSlim.EnterWriteLock();

            insertNode(_rear, node);

            //if (_first == null)
            //{
            //    _first = node;
            //    _rear = node;

            //    Interlocked.Increment(ref count);
            //}
            //else
            //{
            //    addNewNode(node, _rear, null);

            //    //node.Previous = _rear;

            //    _rear.Next = node;

            //}

            //node.Link = this;

            lockSlim.ExitWriteLock();

        }

        public LinkNodeEx<T> AddLast(T value)
        {
            //var t = new LinkNodeEx<T>(value);

            var t = getNode(value);

            AddLast(t);

            return t;
        }

        public void AddFirst(LinkNodeEx<T> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != null)
            {
                if (node.Link == this)
                {
                    throw new InvalidOperationException("node已存在于当前的链表中，请不要重复添加");
                }
                else
                {
                    throw new InvalidOperationException("node 属于另一个 LinkedListEx<T>");
                }
            }

            lockSlim.EnterWriteLock();

            insertNode(null, node);

            //if (_first == null)
            //{

            //    _first = node;
            //    _rear = node;

            //    Interlocked.Increment(ref count);
            //}
            //else
            //{


            //    //addNewNode(node, null, _first);

            //    //node.Next = _first;
            //    //_first.Previous = node;

            //}

            //node.Link = this;

            lockSlim.ExitWriteLock();
        }

        public LinkNodeEx<T> AddFirst(T value)
        {
            //var t = new LinkNodeEx<T>(value);

            var t = getNode(value);

            AddFirst(t);

            return t;
        }

        public void AddAfter(LinkNodeEx<T> node, LinkNodeEx<T> newNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != this)
            {
                throw new InvalidOperationException("node 不在当前 LinkedListEx<T>中");
            }

            if (newNode == null)
            {
                throw new ArgumentNullException("newNode");
            }

            if (newNode.Link != null)
            {
                throw new InvalidOperationException("newNode 属于另一个 LinkedListEx<T>");
            }

            lockSlim.EnterWriteLock();

            insertNode(node, newNode);

            //addNewNode(newNode, node, node.Next);

            //newNode.Previous = node;
            //newNode.Next = node.Next;


            //node.Next = newNode;

            //newNode.Link = this;

            lockSlim.ExitWriteLock();
        }

        public LinkNodeEx<T> AddAfter(LinkNodeEx<T> node, T value)
        {
            //var t = new LinkNodeEx<T>(value);

            var t = getNode(value);

            AddAfter(node,t);

            return t;
        }

        public void AddBefore(LinkNodeEx<T> node, LinkNodeEx<T> newNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != this)
            {
                throw new InvalidOperationException("node 不在当前 LinkedListEx<T>中");
            }

            if (newNode == null)
            {
                throw new ArgumentNullException("newNode");
            }

            if (newNode.Link != null)
            {
                throw new InvalidOperationException("newNode 属于另一个 LinkedListEx<T>");
            }

            lockSlim.EnterWriteLock();

            insertNode(node.Previous, newNode);

            //addNewNode(newNode, node.Previous, node);

            //newNode.Previous = node.Previous;
            //newNode.Next = node;

            //node.Previous = newNode;

            //newNode.Link = this;

            lockSlim.ExitWriteLock();
        }

        public LinkNodeEx<T> AddBefore(LinkNodeEx<T> node, T value)
        {
            //var t = new LinkNodeEx<T>(value);

            var t = getNode(value);

            AddBefore(node, t);

            return t;
        }

        public void Remove(LinkNodeEx<T> node, bool isRelease = true)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (node.Link != this)
            {
                throw new InvalidOperationException("node 属于另一个 LinkedListEx<T>");
            }

            lockSlim.EnterWriteLock();

            //var tempNode = node;

            //node.Previous.Next = node.Next;

            //node.Next.Previous = node.Previous;

            removeNode(node,isRelease);

            lockSlim.ExitWriteLock();

            //deleteNode(tempNode);
        }

        public bool Remove(T value, bool isRelease = true)
        {
            var node = this.Find(value);

            if (node != null)
            {
                lockSlim.EnterWriteLock();

                //node.Previous.Next = node.Next;

                //node.Next.Previous = node.Previous;

                removeNode(node,isRelease);

                lockSlim.ExitWriteLock();

                //deleteNode(node);

                return true;
            }
            else
            {
                return false;
            }

        }

        public LinkNodeEx<T> RemoveFirst(bool isRelease=true)
        {

            LinkNodeEx<T> old = null;

            lockSlim.EnterWriteLock();

            if (count>0)
            {
                Interlocked.Exchange(ref old, _first);

                //old = _first;

                removeNode(_first, isRelease);                
            }

            //if (_first != null)
            //{
            //    if (_first == _rear)
            //    {
            //        _first = null;
            //        _rear = null;
            //    }
            //    else
            //    {
            //        var tempNode = _first;

            //        _first = _first.Next;

            //        deleteNode(tempNode);
            //    }
            //}

            lockSlim.ExitWriteLock();

            return old;
        }

        public LinkNodeEx<T> RemoveLast(bool isRelease = true)
        {
            LinkNodeEx<T> old = null;

            lockSlim.EnterWriteLock();

            if (count>0)
            {
                old = _rear;
                removeNode(_rear,isRelease);
            }

            //if (_first != null)
            //{
            //    if (_first == _rear)
            //    {
            //        _first = null;
            //        _rear = null;
            //    }
            //    else
            //    {
            //        var tempNode = _rear;

            //        _rear = _rear.Previous;

            //        deleteNode(tempNode);
            //    }
            //}

            lockSlim.ExitWriteLock();

            return old;
        }

        public LinkNodeEx<T> Find(T value)
        {
            lockSlim.EnterReadLock();

            LinkNodeEx<T> retNode = null;

            foreach (var node in this)
            {
                if (compare(node.Value, value))
                {
                    retNode = node;
                    break;
                }
            }

            lockSlim.ExitReadLock();

            return retNode;
        }

        public LinkNodeEx<T> FindLast(T value)
        {
            lockSlim.EnterReadLock();

            LinkNodeEx<T> retValue = null;

            var g = GetEnumeratorBack();

            foreach (var node in g)
            {
                if (compare(node.Value, value))
                {
                    retValue = node;
                    break;
                }
            }



            //if (_first != null)
            //{
            //    var tempNode = _rear;

            //    while (tempNode != null)
            //    {
            //        if (compare(tempNode.Value,value))
            //        {
            //            retValue = tempNode;
            //            break;
            //        }

            //        tempNode = tempNode.Previous;
            //    }
            //}

            lockSlim.ExitReadLock();

            return retValue;
        }

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        public bool Contains(LinkNodeEx<T> node)
        {
            if (node.Link==this)
            {
                foreach (var n in this)
                {
                    if (n==node)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            lockSlim.EnterWriteLock();

            var oldFirst = _first;
            //var oldRear = _rear;

            _first = null;
            _rear = null;

            count = 0;

            lockSlim.ExitWriteLock();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                var tempNode = (LinkNodeEx<T>)o;
                LinkNodeEx<T> t1 = null;
                while (tempNode != null)
                {
                    t1 = tempNode;

                    tempNode = tempNode.Next;

                    t1.Release();
                }
            }, oldFirst);

            notifyChanged(NotifyCollectionChangedAction.Reset,null);
        }

        public int Count
        {
            get
            {
                
                return count;
            }
        }

        private void removeNode(LinkNodeEx<T> node, bool isRelease = true)
        {
            if (_first == null || node.Link!=this)
            {
                return;
            }

            if (node.Previous==null && node.Next==null)
            {
                _first = null;
                _rear = null;
            }
            else if (node.Previous == null) //如果需要移除的是首项值
            {
                _first.Next.Previous = null;

                _first = node.Next;

            }
            else if (node.Next == null)
            {
                _rear.Previous.Next = null;

                _rear = node.Previous;
            }
            else
            {
                node.Previous.Next = node.Next;
                node.Next.Previous = node.Previous;
            }

            if (isRelease)
            {
                node.Release();
            }

            Interlocked.Decrement(ref count);

            notifyChanged(NotifyCollectionChangedAction.Remove,node);

            if (node is IRecyclable)
            {
                node.Dispose();
            }
        }

        private LinkNodeEx<T> getNode(T value)
        {
            LinkNodeEx<T> retValue = null;

            if (_pool!=null)
            {
                retValue = _pool.Take();
                retValue.Value = value;
            }
            else

            {
                retValue=new LinkNodeEx<T>(value);
            }

            return retValue;
        }

        //private void addNewNode(LinkNodeEx<T> node, LinkNodeEx<T> pre, LinkNodeEx<T> next)
        //{
        //    node.Link = this;

        //    node.Previous = pre;
        //    node.Next = next;

        //    Interlocked.Increment(ref count);
        //}

        private void insertNode(LinkNodeEx<T> prevNode, LinkNodeEx<T> newNode)
        {
            if (_first == null ||  _rear==null)
            {
                _first = newNode;
                _rear = newNode;
            }
            else
            {
                if (prevNode == null) //如果该值为空，则表示插入在_frist之前
                {
                    newNode.Next = _first;
                    _first.Previous = newNode;

                    _first = newNode;
                }
                else //否则，为插入到node之后
                {
                    newNode.Previous = prevNode;
                    newNode.Next = prevNode.Next;

                    prevNode.Next = newNode;

                    if (prevNode == _rear) //如果插入为末尾，则修改_rear值
                    {
                        _rear = newNode;
                    }
                }
            }

            newNode.Link = this;

            Interlocked.Increment(ref count);

            notifyChanged(NotifyCollectionChangedAction.Add, newNode);
        }

        private void notifyChanged(NotifyCollectionChangedAction action,LinkNodeEx<T> node)
        {
            if (CollectionChanged!=null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs<LinkNodeEx<T>>(action, node, -1));
            }

            if (_collectionChanged!=null)
            {
                _collectionChanged(this,new NotifyCollectionChangedEventArgs(action,node,-1));
            }
        }

        #region Implementation of IEnumerable<LinkNodeEx<T>>

        public IEnumerable<LinkNodeEx<T>> GetEnumeratorBack()
        {
            if (_first != null)
            {
                var tempNode = _rear;

                while (tempNode != null)
                {
                    yield return tempNode;

                    tempNode = tempNode.Previous;
                }
            }
            else
            {
                yield break;

            }
        }

        public IEnumerator<LinkNodeEx<T>> GetEnumerator()
        {
            

            if (_first != null)
            {
                var tempNode = _first;

                while (tempNode != null)
                {
                    yield return tempNode;

                    tempNode = tempNode.Next;
                }
            }



            //lockSlim.ExitReadLock();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Implementation of INotifyCollectionChanged<LinkNodeEx<T>>

        public event NotifyCollectionChangedEventHandler<LinkNodeEx<T>> CollectionChanged;

        #endregion

        private NotifyCollectionChangedEventHandler _collectionChanged;
         event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
         {
             add { _collectionChanged += value; }
             remove { _collectionChanged -= value; }
         }
    }

    public class LinkNodeEx<T> : IEnumerable<LinkNodeEx<T>>,IDisposable
    {
        public LinkNodeEx(T value)
        {
            Value = value;
            Next = null;
            Previous = null;
        }

        internal LinkedListEx<T> Link { set; get; }
        public T Value { set; get; }
        public LinkNodeEx<T> Next { get; internal protected set; }
        public LinkNodeEx<T> Previous { get; internal protected set; }

        public void Release()
        {
            Link = null;
            Next = null;
            Previous = null;
            Value = default(T);
        }

        public LinkNodeEx<T> Copy()
        {
            var temp = new LinkNodeEx<T>(Value);
            return temp;
        }

        #region Implementation of IEnumerable

        public IEnumerator<LinkNodeEx<T>> GetEnumerator()
        {
            var t = this;

            while (t != null)
            {
                yield return t;

                t = t.Next;
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public virtual void Dispose()
        {
            Release();

            GC.SuppressFinalize(this);
        }
    }

    public class LinkNodeExRecyclable<T> : LinkNodeEx<T>, IRecyclable
    {
        private LinkNodeExPool<T> _pool;

        internal LinkNodeExRecyclable(LinkNodeExPool<T> pool) : base(default(T))
        {
            _pool = pool;
        }

        public IRecyclablePool<IRecyclable> Pool
        {
            get { return (IRecyclablePool<IRecyclable>) _pool; }
            set {}
        }

        public void DisposeObject()
        {
           base.Dispose();
        }

        public override void Dispose()
        {
            base.Release();

            _pool.RecycleObject(this);
        }
    }

    public class LinkNodeExPool<T>:RecyclablePool<LinkNodeExRecyclable<T>>
    {

        public LinkNodeExPool(int maxLength,int minLength):base(maxLength,minLength)
        {
            base.Init();
        }

        protected override LinkNodeExRecyclable<T> CreateRecyclableObject()
        {
            return new LinkNodeExRecyclable<T>(this);
        }
    }
}
