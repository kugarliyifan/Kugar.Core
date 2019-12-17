using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Kugar.Core.Linq
{
    internal class GroupedEnumerable<TSource, TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        // Fields
        private IEqualityComparer<TKey> comparer;
        private Func<TSource, TElement> elementSelector;
        private Func<TSource, TKey> keySelector;
        private IEnumerable<TSource> source;

        // Methods
        public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            if (elementSelector == null)
            {
                throw Error.ArgumentNull("elementSelector");
            }
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return Lookup<TKey, TElement>.Create<TSource>(this.source, this.keySelector, this.elementSelector, this.comparer).GetEnumerator();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    internal class GroupedEnumerable<TSource, TKey, TElement, TResult> : IEnumerable<TResult>, IEnumerable
    {
        // Fields
        private IEqualityComparer<TKey> comparer;
        private Func<TSource, TElement> elementSelector;
        private Func<TSource, TKey> keySelector;
        private Func<TKey, IEnumerable<TElement>, TResult> resultSelector;
        private IEnumerable<TSource> source;

        // Methods
        public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            if (elementSelector == null)
            {
                throw Error.ArgumentNull("elementSelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
            this.resultSelector = resultSelector;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Lookup<TKey, TElement>.Create<TSource>(this.source, this.keySelector, this.elementSelector, this.comparer).ApplyResultSelector<TResult>(this.resultSelector).GetEnumerator();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

 



}
