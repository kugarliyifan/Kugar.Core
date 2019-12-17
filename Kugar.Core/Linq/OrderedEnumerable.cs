using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Kugar.Core.Linq
{


    internal abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>, IEnumerable<TElement>, IEnumerable
    {
        // Fields
        internal IEnumerable<TElement> source;

        // Methods
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected OrderedEnumerable()
        {
        }

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next);
        public IEnumerator<TElement> GetEnumerator()
        {
            SerializableEnumerable.Buffer<TElement> iteratorVariable0 = new SerializableEnumerable.Buffer<TElement>(this.source);
            if (iteratorVariable0.count <= 0)
            {
                goto Label_00EA;
            }
            int[] iteratorVariable2 = this.GetEnumerableSorter(null).Sort(iteratorVariable0.items, iteratorVariable0.count);
            int index = 0;
        Label_PostSwitchInIterator: ;
            if (index < iteratorVariable0.count)
            {
                yield return iteratorVariable0.items[iteratorVariable2[index]];
                index++;
                goto Label_PostSwitchInIterator;
            }
        Label_00EA: ;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedEnumerable<TElement, TKey>(this.source, keySelector, comparer, descending) { parent = (OrderedEnumerable<TElement>)this };
        }
    }

    internal class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement>
    {
        // Fields
        internal IComparer<TKey> comparer;
        internal bool descending;
        internal Func<TElement, TKey> keySelector;
        internal OrderedEnumerable<TElement> parent;

        // Methods
        internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            base.source = source;
            this.parent = null;
            this.keySelector = keySelector;
            this.comparer = (comparer != null) ? comparer : ((IComparer<TKey>)Comparer<TKey>.Default);
            this.descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next)
        {
            EnumerableSorter<TElement> enumerableSorter = new EnumerableSorter<TElement, TKey>(this.keySelector, this.comparer, this.descending, next);
            if (this.parent != null)
            {
                enumerableSorter = this.parent.GetEnumerableSorter(enumerableSorter);
            }
            return enumerableSorter;
        }
    }





}
