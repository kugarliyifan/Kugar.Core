using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kugar.Core.Linq
{

    [Serializable]
    public  class SerializableEnumerable<T>
    {
        

        public  IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            if (source is Iterator<TSource>)
                return ((Iterator<TSource>)source).Where(predicate);
            if (source is TSource[])
                return (IEnumerable<TSource>)new WhereArrayIterator<TSource>((TSource[])source, predicate);
            if (source is List<TSource>)
                return (IEnumerable<TSource>)new WhereListIterator<TSource>((List<TSource>)source, predicate);
            else
                return (IEnumerable<TSource>)new WhereEnumerableIterator<TSource>(source, predicate);
        }

        public  IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
                throw Error.ArgumentNull("source");
            if (selector == null)
                throw Error.ArgumentNull("selector");
            if (source is Iterator<TSource>)
                return ((Iterator<TSource>)source).Select<TResult>(selector);
            if (source is TSource[])
                return (IEnumerable<TResult>)new WhereSelectArrayIterator<TSource, TResult>((TSource[])source, (Func<TSource, bool>)null, selector);
            if (source is List<TSource>)
                return (IEnumerable<TResult>)new WhereSelectListIterator<TSource, TResult>((List<TSource>)source, (Func<TSource, bool>)null, selector);
            else
                return (IEnumerable<TResult>)new WhereSelectEnumerableIterator<TSource, TResult>(source, (Func<TSource, bool>)null, selector);
        }

        private static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2)
        {
            return (Func<TSource, bool>)(x =>
            {
                if (predicate1(x))
                    return predicate2(x);
                else
                    return false;
            });
        }

        private static Func<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, TMiddle> selector1, Func<TMiddle, TResult> selector2)
        {
            return (Func<TSource, TResult>)(x => selector2(selector1(x)));
        }

        public  IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            else
                return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            else
                return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        private  IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter outer1 in outer)
            {
                Lookup<TKey, TInner>.Grouping g = lookup.GetGrouping(outerKeySelector(outer1), false);
                if (g != null)
                {
                    for (int i = 0; i < g.count; ++i)
                        yield return resultSelector(outer1, g.elements[i]);
                }
            }
        }

        public  IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            else
                return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw Error.ArgumentNull("outer");
            if (inner == null)
                throw Error.ArgumentNull("inner");
            if (outerKeySelector == null)
                throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null)
                throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null)
                throw Error.ArgumentNull("resultSelector");
            else
                return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        private  IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter outer1 in outer)
                yield return resultSelector(outer1, lookup[outerKeySelector(outer1)]);
        }

        public  IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (IOrderedEnumerable<TSource>)new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>)null, false);
        }

        public  IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return (IOrderedEnumerable<TSource>)new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public  IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (IOrderedEnumerable<TSource>)new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>)null, true);
        }

        public  IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return (IOrderedEnumerable<TSource>)new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public  IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (IEnumerable<IGrouping<TKey, TSource>>)new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return (IEnumerable<IGrouping<TKey, TSource>>)new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public  IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return (IEnumerable<IGrouping<TKey, TElement>>)new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return (IEnumerable<IGrouping<TKey, TElement>>)new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        }

        public  IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return (IEnumerable<TResult>)new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return (IEnumerable<TResult>)new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, (IEqualityComparer<TKey>)null);
        }

        public  IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            return (IEnumerable<TResult>)new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        public  IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            return (IEnumerable<TResult>)new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
        }

        public  ILookup<TKey, TSource> ToLookup<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (ILookup<TKey, TSource>)Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, (IEqualityComparer<TKey>)null);
        }

        public  ILookup<TKey, TSource> ToLookup<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return (ILookup<TKey, TSource>)Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public  ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return (ILookup<TKey, TElement>)Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, (IEqualityComparer<TKey>)null);
        }

        public  ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return (ILookup<TKey, TElement>)Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
        }

        public  IEnumerable<TResult> Empty<TResult>()
        {
            return EmptyEnumerable<TResult>.Instance;
        }

        [Serializable]
        internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private int threadId; //TODO:需要设置的
            internal int state;
            internal TSource current;

            public TSource Current
            {
                [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            public Iterator()
            {
                this.threadId = Thread.CurrentThread.ManagedThreadId;
            }

            public abstract Iterator<TSource> Clone();

            public virtual void Dispose()
            {
                this.current = default(TSource);
                this.state = -1;
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                if (this.threadId == Thread.CurrentThread.ManagedThreadId && this.state == 0)
                {
                    this.state = 1;
                    return (IEnumerator<TSource>)this;
                }
                else
                {
                    Iterator<TSource> iterator = this.Clone();
                    iterator.state = 1;
                    return (IEnumerator<TSource>)iterator;
                }
            }

            public abstract bool MoveNext();

            public abstract IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector);

            public abstract IEnumerable<TSource> Where(Func<TSource, bool> predicate);

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }

            void IEnumerator.Reset()
            {
                throw new NotImplementedException();
            }
        }

        [Serializable]
        internal class WhereEnumerableIterator<TSource> : Iterator<TSource>
        {
            [NonSerialized]
            internal IEnumerable<TSource> source;
            private Func<TSource, bool> predicate;
            private IEnumerator<TSource> enumerator;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone()
            {
                return (Iterator<TSource>)new WhereEnumerableIterator<TSource>(this.source, this.predicate);
            }

            public override void Dispose()
            {
                if (this.enumerator != null)
                    this.enumerator.Dispose();
                this.enumerator = (IEnumerator<TSource>)null;
                base.Dispose();
            }

            public override bool MoveNext()
            {
                switch (this.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        this.state = 2;
                        goto case 2;
                    case 2:
                        while (this.enumerator.MoveNext())
                        {
                            TSource current = this.enumerator.Current;
                            if (this.predicate(current))
                            {
                                this.current = current;
                                return true;
                            }
                        }
                        this.Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return (IEnumerable<TResult>)new WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return (IEnumerable<TSource>)new WhereEnumerableIterator<TSource>(this.source, CombinePredicates<TSource>(this.predicate, predicate));
            }
        }



        [Serializable]
        internal class WhereSelectEnumerableIterator<TSource, TResult> : Iterator<TResult>
        {
            [NonSerialized]
            internal IEnumerable<TSource> source;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private IEnumerator<TSource> enumerator;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereSelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone()
            {
                return (Iterator<TResult>)new WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override void Dispose()
            {
                if (this.enumerator != null)
                    this.enumerator.Dispose();
                this.enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext()
            {
                switch (this.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        this.state = 2;
                        goto case 2;
                    case 2:
                        while (this.enumerator.MoveNext())
                        {
                            TSource current = this.enumerator.Current;
                            if (this.predicate == null || this.predicate(current))
                            {
                                this.current = this.selector(current);
                                return true;
                            }
                        }
                        this.Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return (IEnumerable<TResult2>)new WhereSelectEnumerableIterator<TSource, TResult2>(this.source, this.predicate, CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>)new WhereEnumerableIterator<TResult>((IEnumerable<TResult>)this, predicate);
            }
        }


        [Serializable]
        internal class WhereArrayIterator<TSource> : Iterator<TSource>
        {
            private TSource[] source;
            private Func<TSource, bool> predicate;
            private int index;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereArrayIterator(TSource[] source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone()
            {
                return (Iterator<TSource>)new WhereArrayIterator<TSource>(this.source, this.predicate);
            }

            public override bool MoveNext()
            {
                if (this.state == 1)
                {
                    while (this.index < this.source.Length)
                    {
                        TSource source = this.source[this.index];
                        ++this.index;
                        if (this.predicate(source))
                        {
                            this.current = source;
                            return true;
                        }
                    }
                    this.Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return (IEnumerable<TResult>)new WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return (IEnumerable<TSource>)new WhereArrayIterator<TSource>(this.source, CombinePredicates<TSource>(this.predicate, predicate));
            }
        }

        [Serializable]
        internal class WhereListIterator<TSource> : Iterator<TSource>
        {
            private List<TSource> source;
            private Func<TSource, bool> predicate;
            private List<TSource>.Enumerator enumerator;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone()
            {
                return (Iterator<TSource>)new WhereListIterator<TSource>(this.source, this.predicate);
            }

            public override bool MoveNext()
            {
                switch (this.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        this.state = 2;
                        goto case 2;
                    case 2:
                        while (this.enumerator.MoveNext())
                        {
                            TSource current = this.enumerator.Current;
                            if (this.predicate(current))
                            {
                                this.current = current;
                                return true;
                            }
                        }
                        this.Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return (IEnumerable<TResult>)new WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return (IEnumerable<TSource>)new WhereListIterator<TSource>(this.source, CombinePredicates<TSource>(this.predicate, predicate));
            }
        }


        [Serializable]
        internal class WhereSelectArrayIterator<TSource, TResult> : Iterator<TResult>
        {
            private TSource[] source;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private int index;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereSelectArrayIterator(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone()
            {
                return (Iterator<TResult>)new WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override bool MoveNext()
            {
                if (this.state == 1)
                {
                    while (this.index < this.source.Length)
                    {
                        TSource source = this.source[this.index];
                        ++this.index;
                        if (this.predicate == null || this.predicate(source))
                        {
                            this.current = this.selector(source);
                            return true;
                        }
                    }
                    this.Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return (IEnumerable<TResult2>)new WhereSelectArrayIterator<TSource, TResult2>(this.source, this.predicate, CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>)new WhereEnumerableIterator<TResult>((IEnumerable<TResult>)this, predicate);
            }
        }

        [Serializable]
        internal class WhereSelectListIterator<TSource, TResult> : Iterator<TResult>
        {
            internal List<TSource> source;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private List<TSource>.Enumerator enumerator;

            [TargetedPatchingOptOut("Performance critical to inline type of method across NGen image boundaries")]
            public WhereSelectListIterator(List<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone()
            {
                return (Iterator<TResult>)new WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override bool MoveNext()
            {
                switch (this.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        this.state = 2;
                        goto case 2;
                    case 2:
                        while (this.enumerator.MoveNext())
                        {
                            TSource current = this.enumerator.Current;
                            if (this.predicate == null || this.predicate(current))
                            {
                                this.current = this.selector(current);
                                return true;
                            }
                        }
                        this.Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return (IEnumerable<TResult2>)new WhereSelectListIterator<TSource, TResult2>(this.source, this.predicate, CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>)new WhereEnumerableIterator<TResult>((IEnumerable<TResult>)this, predicate);
            }
        }



        [StructLayout(LayoutKind.Sequential)]
        internal struct Buffer<TElement>
        {
            internal TElement[] items;
            internal int count;
            internal Buffer(IEnumerable<TElement> source)
            {
                TElement[] array = null;
                int length = 0;
                ICollection<TElement> is2 = source as ICollection<TElement>;
                if (is2 != null)
                {
                    length = is2.Count;
                    if (length > 0)
                    {
                        array = new TElement[length];
                        is2.CopyTo(array, 0);
                    }
                }
                else
                {
                    foreach (TElement local in source)
                    {
                        if (array == null)
                        {
                            array = new TElement[4];
                        }
                        else if (array.Length == length)
                        {
                            TElement[] destinationArray = new TElement[length * 2];
                            Array.Copy(array, 0, destinationArray, 0, length);
                            array = destinationArray;
                        }
                        array[length] = local;
                        length++;
                    }
                }
                this.items = array;
                this.count = length;
            }

            internal TElement[] ToArray()
            {
                if (this.count == 0)
                {
                    return new TElement[0];
                }
                if (this.items.Length == this.count)
                {
                    return this.items;
                }
                TElement[] destinationArray = new TElement[this.count];
                Array.Copy(this.items, 0, destinationArray, 0, this.count);
                return destinationArray;
            }
        }

        internal class EmptyEnumerable<TElement>
        {
            // Fields
            private static TElement[] instance;

            // Properties
            public static IEnumerable<TElement> Instance
            {
                get
                {
                    if (EmptyEnumerable<TElement>.instance == null)
                    {
                        EmptyEnumerable<TElement>.instance = new TElement[0];
                    }
                    return EmptyEnumerable<TElement>.instance;
                }
            }
        }


        internal class IdentityFunction<TElement>
        {
            // Properties
            public static Func<TElement, TElement> Instance
            {
                get
                {
                    return x => x;
                }
            }
        }

    }
}


