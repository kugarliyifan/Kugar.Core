using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace System.Collections.Specialized
{

     public interface INotifyCollectionChanged<T>
    {
        event NotifyCollectionChangedEventHandler<T> CollectionChanged;
    }

     [Serializable]
     public sealed class NotifyCollectionChangedEventArgs<T> : EventArgs
     {
         IList<T> new_items, old_items;

         public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
         {
             if (action != NotifyCollectionChangedAction.Reset)
                 throw new NotSupportedException();

             Action = action;
             NewStartingIndex = -1;
             OldStartingIndex = -1;
         }

         public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, T changedItem, int index)
         {
             switch (action)
             {
                 case NotifyCollectionChangedAction.Add:
                     new_items = new List<T>();
                     new_items.Add(changedItem);
                     NewStartingIndex = index;
                     OldStartingIndex = -1;
                     break;
                 case NotifyCollectionChangedAction.Remove:
                     old_items = new List<T>();
                     old_items.Add(changedItem);
                     OldStartingIndex = index;
                     NewStartingIndex = -1;
                     break;
                 default:
                     throw new NotSupportedException();
             }

             Action = action;
         }

         public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem, int index)
         {
             if (action != NotifyCollectionChangedAction.Replace)
                 throw new NotSupportedException();

             Action = action;

             new_items = new List<T>();
             new_items.Add(newItem);

             old_items = new List<T>();
             old_items.Add(oldItem);

             NewStartingIndex = index;
             OldStartingIndex = -1;
         }

         public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<T> newItems, IList<T> oldItems,int newStartIndex, int oldStartIndex)
         {
             new_items = newItems;
             old_items = oldItems;
             NewStartingIndex = newStartIndex;
             OldStartingIndex = OldStartingIndex;
             Action = action;
         }

         public NotifyCollectionChangedAction Action { get; private set; }

         public IList<T> NewItems
         {
             get { return new_items; }
         }

         public IList<T> OldItems
         {
             get { return old_items; }
         }

         public int NewStartingIndex { get; private set; }
         public int OldStartingIndex { get; private set; }
     }

    public delegate void NotifyCollectionChangedEventHandler<T>(object sender, NotifyCollectionChangedEventArgs<T> e);


    public interface INotifyCollectionChanging<T>
    {
        event NotifyCollectionChangingEventHandler<T> CollectionChanging;
    }

    

    public delegate void NotifyCollectionChangingEventHandler<T>(object sender, NotifyCollectionChangingEventArgs<T> e);


    [Serializable]
    public sealed class NotifyCollectionChangingEventArgs<T> : EventArgs
    {
        IList<T> new_items, old_items;

        private NotifyCollectionChangingEventArgs()
        {
            IsEnable = true;
        }

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action)
            : this()
        {
            if (action != NotifyCollectionChangedAction.Reset)
                throw new NotSupportedException();

            Action = action;
            NewStartingIndex = -1;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, T changedItem, int index)
            : this()
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    new_items = new List<T>();
                    new_items.Add(changedItem);
                    NewStartingIndex = index;
                    OldStartingIndex = -1;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    old_items = new List<T>();
                    old_items.Add(changedItem);
                    OldStartingIndex = index;
                    NewStartingIndex = -1;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Action = action;
        }

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem, int index)
            : this()
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new NotSupportedException();

            Action = action;

            new_items = new List<T>();
            new_items.Add(newItem);

            old_items = new List<T>();
            old_items.Add(oldItem);

            NewStartingIndex = index;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, T[] newItem, T[] oldItem, int index)
            : this()
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new NotSupportedException();

            Action = action;

            new_items = newItem;
            //new_items.Add(newItem);

            old_items = oldItem;
            //old_items.Add(oldItem);

            NewStartingIndex = index;
            OldStartingIndex = -1;
        }

        public NotifyCollectionChangedAction Action { get; private set; }

        public IList<T> NewItems
        {
            get { return new_items; }
        }

        public IList<T> OldItems
        {
            get { return old_items; }
        }

        public int NewStartingIndex { get; private set; }
        public int OldStartingIndex { get; private set; }

        /// <summary>
        ///     是否允许该操作
        /// </summary>
        public bool IsEnable { set; get; }
    }

#if NET2
    public enum NotifyCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Reset = 4
    }
#endif






}
