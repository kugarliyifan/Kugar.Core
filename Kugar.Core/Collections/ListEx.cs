using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Kugar.Core.BaseStruct;
using System.Linq;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     支持通知事件的List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListEx<T> : Collection<T>, INotifyCollectionChanged<T>, INotifyCollectionChanging<T>,INotifyCollectionChanged
    {
        public ListEx()
        {
        }

        public ListEx(IList<T> list) : base(list)
        {
        }

        public void AddRange(IEnumerable<T> srcLst)
        {
            if (srcLst == null)
            {
                throw new ArgumentNullException("srcLst");
            }

            foreach (var item in srcLst)
            {
                this.Add(item);
            }

            //var e = new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset,srcLst.ToArray(),null,);
            //OnCollectionChanged(e);
        }

        public T[] ToArray()
        {
            var t = new T[this.Count];

            for (int i = 0; i < this.Count; i++)
            {
                t[i] = this[i];
            }

            return t;
        }

        protected override void InsertItem(int index, T item)
        {

            var e1 = new NotifyCollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Add, item, index);

            OnCollectionChanging(e1);

            if (!e1.IsEnable)
            {
                return;
            }

            base.InsertItem(index, item);

            var e = new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, item, index);

            OnCollectionChanged(e);
        }

        protected override void RemoveItem(int index)
        {
            var tempItem = base[index];

            var e1 = new NotifyCollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Remove, tempItem, index);

            OnCollectionChanging(e1);

            if (!e1.IsEnable)
            {
                return;
            }


            base.RemoveItem(index);

            var e = new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, tempItem, index);

            OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            if (base.Count<=0)
            {
                return;
            }

            var tempArrary = new T[base.Count];

            base.CopyTo(tempArrary, 0);

            var e1 = new NotifyCollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Reset,null,tempArrary,0);

            OnCollectionChanging(e1);

            if (!e1.IsEnable)
            {
                return;
            }

            base.ClearItems();

            var e = new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset);

            OnCollectionChanged(e);
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = base[index];

            var e1 = new NotifyCollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Replace, item, oldItem, index);

            OnCollectionChanging(e1);

            if (!e1.IsEnable)
            {
                return;
            }

            base.SetItem(index, item);

            var e = new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Replace,item,oldItem,index);

            OnCollectionChanged(e);
        }

        protected  virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs<T> e)
        {
            if (CollectionChanged!=null)
            {
                CollectionChanged(this, e);
            }

            if (_collectionChanged!=null)
            {
                var e1 = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, e.NewStartingIndex);

                _collectionChanged(this, e1);
            }
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs<T> e)
        {
            if (CollectionChanging != null)
            {
                CollectionChanging(this, e);
            }
        }

        public event NotifyCollectionChangedEventHandler<T> CollectionChanged;
        public event NotifyCollectionChangingEventHandler<T> CollectionChanging;

        private NotifyCollectionChangedEventHandler _collectionChanged;
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }
    }


}
