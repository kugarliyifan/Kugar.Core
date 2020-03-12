using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;

#if NET45
  using Kugar.Core.Linq;    
#endif


namespace Kugar.Core.ExtMethod
{
    public class ForEachArgs<T> : EventArgs
    {
        public ForEachArgs(T item)
        {
            Cancel = false;
            Item = item;
        }

        public bool Cancel { set; get; }

        public object Data { set; get; }

        public T Item { private set; get; }
    }

    /// <summary>
    /// HashSet对象的扩展函数
    /// </summary>
    public static class HashSetExt
    {
        /// <summary>
        /// 判断Src参数指向的hashSet是否包含desc中的全部元素,如果有任意一个不存在,则返回false,否则返回true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static bool IsContainsAll<T>(this HashSet<T> src, HashSet<T> desc)
        {
            if (desc.Count < src.Count)
            {
                foreach (var item in desc)
                {
                    if (!src.Contains(item))
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (var item in src)
                {
                    if (!desc.Contains(item))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 判断Src参数指向的hashSet是否包含desc中的任意元素,如果有任意一个,则返回true,否则返回false<br/>
        /// 类似于集合的交集操作,只不过使用了HashSet的一些特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static bool IsContainsAny<T>(this HashSet<T> src, HashSet<T> desc)
        {
            if (desc.Count < src.Count)
            {
                foreach (var item in desc)
                {
                    if (src.Contains(item))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in src)
                {
                    if (desc.Contains(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static class IEnumerableExt
    {
        /// <summary>
        /// 乱序输出一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var elements = source.ToArray();
            for (int i = elements.Length - 1; i > 0; i--)
            {
                int swapIndex = RandomEx.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }

            yield return elements[0];
        }

        //   /// <summary>
        //   ///     将列表转换为Dictionary
        //   /// </summary>
        //   /// <typeparam name="T">元数据类型</typeparam>
        //   /// <param name="srcList">源数据</param>
        //   /// <param name="keyFunc">获取元数据中用来作为字典类的Key</param>
        //   /// <param name="valueFunc">获取元数据中用来作为字典类的value</param>
        //   /// <returns></returns>
        //public static Dictionary<TKey,TValue> ToDictionary<T,TKey,TValue>(this IEnumerable<T> src,Func<T,TKey> keyFunc,Func<T,TValue> valueFunc)
        //{
        //	if (src==null) {
        //		throw new ArgumentNullException("src");
        //	}

        //	if (keyFunc==null) {
        //		throw new ArgumentNullException("keyFunc");
        //	}

        //	if (valueFunc==null) {
        //		throw new ArgumentNullException("valueFunc");
        //	}

        //	var tempDic=new Dictionary<TKey,TValue>();

        //	foreach (var item in src) {
        //		tempDic.Add(keyFunc(item),valueFunc(item));
        //	}

        //	return tempDic;
        //}

        /// <summary>
        ///     将列表转换为Dictionary,默认将整个对象作为value
        /// </summary>
        /// <typeparam name="T">元数据类型</typeparam>
        /// <param name="srcList">源数据</param>
        /// <param name="keyFunc">获取元数据中用来作为字典类的Key</param>
        /// <returns></returns>
        public static Dictionary<TKey, T> ToDictionary<T, TKey>(this IEnumerable<T> src, Func<T, TKey> keyFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            var tempDic = new Dictionary<TKey, T>();

            foreach (var item in src)
            {
                tempDic.Add(keyFunc(item), item);
            }

            return tempDic;
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyPairs<T, TKey, TValue>(this IEnumerable<T> src,
            Func<T, TKey> keyFunc, Func<T, TValue> valueFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException("valueFunc");
            }

            foreach (var item in src)
            {
                yield return new KeyValuePair<TKey, TValue>(keyFunc(item), valueFunc(item));
            }
        }


        /// <summary>
        ///     返回一个不包含空对象的可枚举对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcList">源数据</param>
        /// <returns></returns>
        public static IEnumerable<T> IgnoreNulls<T>(this IEnumerable<T> srcList)
        {
            if (ReferenceEquals(srcList, null))
                yield break;

            foreach (var item in srcList)
            {
                if (ReferenceEquals(item, null))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        ///     返回一个指定对象的可枚举对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcList">源列表</param>
        /// <param name="checker">用于判断是否符合要排除的对象的函数</param>
        /// <returns></returns>
        public static IEnumerable<T> IgnoreSpecial<T>(this IEnumerable<T> srcList, Predicate<T> checker)
        {
            foreach (var item in srcList)
            {
                if (!checker(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        ///     筛选出指定数据源中所有不重复的项目,注意,该函数不能在Linq to EF或者Linq to Sql中调用,会导致所有数据加载如本地再判断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">数据源</param>
        /// <param name="checkFunc">判断是否重复的函数</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> src, Func<T, T, bool> checkFunc)
        {
            var tempList = new List<T>();

            if (src == null)
            {
                return tempList;
            }

            var isSame = true;
            foreach (var item in src)
            {
                isSame = false;
                foreach (var nItem in tempList)
                {
                    if (item.SafeEquals(nItem) || checkFunc(item, nItem))
                    {
                        isSame = true;
                        break;
                    }
                }

                if (!isSame)
                {
                    tempList.Add(item);
                }
            }

            return tempList;

            //return source == null ? Enumerable.Empty<T>() : source.GroupBy(expression).Select(i => i.First());
        }

        /// <summary>
        /// 获取两个集合的交集，即两个集合的变化，比Linq的Except多了desc与src的比较
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExceptAll<T>(this IEnumerable<T> src, IEnumerable<T> desc)
        {
            if (!src.HasData())
            {
                return desc;
            }

            if (!desc.HasData())
            {
                return src;
            }

            return Enumerable.Union(src.Except(desc), desc.Except(src));
        }

        public static IEnumerable<TSource> Update<TSource>(this IEnumerable<TSource> source, Action<TSource> updateFunc)
        {
            if (source != null && source.Count() > 0)
            {
                foreach (var s in source)
                {
                    updateFunc(s);
                }
            }

            return source;
        }

        public static IEnumerable<TSource> UpdateIf<TSource>(this IEnumerable<TSource> source,
            Predicate<TSource> checkFunc, Action<TSource> updateFunc)
        {
            if (source != null && source.Count() > 0)
            {
                foreach (var s in source)
                {
                    if (!checkFunc(s))
                    {
                        continue;
                    }

                    updateFunc(s);
                }
            }

            return source;
        }

        ///// <summary>
        /////     合并两个集合
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="src"></param>
        ///// <param name="targetList"></param>
        ///// <returns></returns>
        //public static IEnumerable<T> Union<T>(this IEnumerable<T> src, IEnumerable<T> targetList)
        //{
        //    if (targetList == null && src == null)
        //    {
        //        return null;
        //    }

        //    if (src == null)
        //    {
        //        return targetList;
        //    }

        //    if (targetList == null)
        //    {
        //        return src;
        //    }

        //    var t = new List<T>(src.Count() + targetList.Count());

        //    //if (src!=null)
        //    //{
        //        t.AddRange(src);
        //    //}

        //    //if (targetList != null)
        //    //{
        //        t.AddRange(targetList);
        //    //}

        //    return t;
        //}


        public static string JoinToString<T>(this IEnumerable<T> n)
        {
            return JoinToString(n, ",", "", "");
        }

        public static string JoinToString<T>(this IEnumerable<T> n, char split)
        {
            return JoinToString(n, split, "", "");
        }


        public static string JoinToString<T>(this IEnumerable<T> n, char splite, string beforeChar = "", string endChar = "")
        {
            if (n == null)
            {
                return String.Empty;
            }

            var s = new StringBuilder(200);

            foreach (var n1 in n)
            {
                if (!string.IsNullOrEmpty(beforeChar))
                {
                    s.Append(beforeChar);
                }

                s.Append(n1.ToStringEx());

                if (!string.IsNullOrEmpty(endChar))
                {
                    s.Append(endChar);
                }

                //if (!string.IsNullOrEmpty(splite))
                {
                    s.Append(splite);
                }

                //s.Append(String.Format("{0}{1}{2}{3}", beforeChar, n1.ToStringEx(), endChar, splite));
            }

            if (s.Length >= 1 && s[s.Length - 1] == splite)
            {
                s.Remove(s.Length - 1, 1);
            }

            return s.ToStringEx();
        }


        public static string JoinToString<T>(this IEnumerable<T> n, string splite, string beforeChar = "", string endChar = "")
        {
            if (n == null)
            {
                return String.Empty;
            }

            var s = new StringBuilder(200);

            foreach (var n1 in n)
            {
                if (!string.IsNullOrEmpty(beforeChar))
                {
                    s.Append(beforeChar);
                }

                s.Append(n1.ToStringEx());

                if (!string.IsNullOrEmpty(endChar))
                {
                    s.Append(endChar);
                }

                if (!string.IsNullOrEmpty(splite))
                {
                    s.Append(splite);
                }

                //s.Append(String.Format("{0}{1}{2}{3}", beforeChar, n1.ToStringEx(), endChar, splite));
            }

            if (!string.IsNullOrEmpty(splite) && s.Length >= splite.Length)
            {
                s.Remove(s.Length - splite.Length, splite.Length);

            }

            return s.ToStringEx();
        }

        public static string JoinToString<T>(this IEnumerable<T> n, Func<T, string> newStrFactory, string splite = ",",
            string beforeChar = "", string endChar = "")
        {
            if (n == null)
            {
                return String.Empty;
            }

            var s = new StringBuilder(200);

            foreach (var n1 in n)
            {
                if (!string.IsNullOrEmpty(beforeChar))
                {
                    s.Append(beforeChar);
                }

                s.Append(newStrFactory(n1).ToStringEx());

                if (!string.IsNullOrEmpty(endChar))
                {
                    s.Append(endChar);
                }

                if (!string.IsNullOrEmpty(splite))
                {
                    s.Append(splite);
                }

                //s.Append(String.Format("{0}{1}{2}{3}", beforeChar, n1.ToStringEx(), endChar, splite));
            }

            if (!string.IsNullOrEmpty(splite) && s.Length >= splite.Length)
            {
                s.Remove(s.Length - splite.Length, splite.Length);

            }

            return s.ToStringEx();
        }

        public static T[] Random<T>(this IEnumerable<T> src)
        {
            return Random(src, 0);
        }

        public static T[] Random<T>(this IEnumerable<T> src, int randomCount)
        {
            var tList = src.ToList();

            if (src == null || tList.Count <= 0)
            {
                throw new ArgumentNullException("src");
            }

            if (randomCount == 0)
            {
                randomCount = RandomEx.Next(1, tList.Count);
            }

            var newlist = new T[randomCount];

            for (int i = 0; i < randomCount; i++)
            {
                var index = RandomEx.Next(0, tList.Count);

                newlist[i] = tList[index];
            }

            return newlist;
        }


        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, int, bool> predicate,
            bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }


        [Obsolete]
        public static TResult[] ChangeType<TSource, TResult>(this TSource[] src, ChangeTypeFunc<TSource, TResult> func)
        {
            if (func != null)
            {
                var lst = src.Select(source => func(source)).ToList();

                if (lst != null && lst.Count > 0)
                {
                    return lst.ToArray();
                }

                return new TResult[0];
                ;
            }

            return new TResult[0];
            ;
        }

        [Obsolete]
        public static IEnumerable<TResult> ChangeType<TSource, TResult>(this IEnumerable<TSource> src,
            ChangeTypeFunc<TSource, TResult> func)
        {
            if (func != null)
            {
                var lst = src.Select(source => func(source)).ToList();

                if (lst != null && lst.Count > 0)
                {
                    return lst;
                }

                return new TResult[0];
                ;
            }

            return new TResult[0];
            ;
        }

        /// <summary>
        ///     转换指定类型的枚举列表为新的类型
        /// </summary>
        /// <typeparam name="TSource">源枚举Item类型</typeparam>
        /// <typeparam name="TResult">目标枚举Item类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static TResult[] Cast<TSource, TResult>(this TSource[] src, ChangeTypeFunc<TSource, TResult> func)
        {
            if (func != null)
            {
                var lst = new List<TResult>();

                foreach (var source in src)
                {
                    lst.Add(func(source));
                }

                //var lst = src.Select(source => func(source)).ToList();

                if (lst.Count > 0)
                {
                    return lst.ToArray();
                }

                return new TResult[0];
                ;
            }

            return new TResult[0];
            ;
        }

        /// <summary>
        ///     转换指定类型的枚举列表为新的类型
        /// </summary>
        /// <typeparam name="TSource">源枚举Item类型</typeparam>
        /// <typeparam name="TResult">目标枚举Item类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static TResult[] Cast<TSource, TResult>(this IList<TSource> src, ChangeTypeFunc<TSource, TResult> func)
        {
            if (func != null)
            {
                var lst = new List<TResult>();

                foreach (var source in src)
                {
                    lst.Add(func(source));
                }

                //var lst = src.Select(source => func(source)).ToList();

                if (lst.Count > 0)
                {
                    return lst.ToArray();
                }

                return new TResult[0];
                ;
            }

            return new TResult[0];
            ;
        }


        /// <summary>
        ///     转换指定类型的枚举列表为新的类型
        /// </summary>
        /// <param name="src">源列表</param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static IEnumerable Cast(this IEnumerable src, ChangeTypeFunc func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            var lst = new List<object>();

            foreach (var source in src)
            {
                lst.Add(func(source));
            }

            return lst.AsQueryable();
        }

        /// <summary>
        ///     转换指定类型的枚举列表为新的类型
        /// </summary>
        /// <typeparam name="TSource">源枚举Item类型</typeparam>
        /// <typeparam name="TResult">目标枚举Item类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Cast<TSource, TResult>(this IEnumerable<TSource> src,
            ChangeTypeFunc<TSource, TResult> func)
        {
            if (func != null)
            {
                var lst = new List<TResult>();

                foreach (var source in src)
                {
                    lst.Add(func(source));
                }

                //var lst = src.Select(source => func(source)).ToList();

                if (lst.Count > 0)
                {
                    return lst.ToArray();
                }

                return new TResult[0];
            }

            return new TResult[0];
        }


        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable list)
        {
            return new ObjectEnumerable<T>(list);
        }

        public static bool AllEqual<T>(this IEnumerable<T> src, IEnumerable<T> des)
        {
            if (src == null && des != null)
            {
                return false;
            }

            if (src != null && des == null)
            {
                return false;
            }

            if (src == null && des == null)
            {
                return true;
            }

            foreach (var item in src)
            {
                if (!des.Contains(item))
                {
                    return false;
                }
            }

            foreach (var item in des)
            {
                if (!src.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        private class ObjectEnumerable<T> : IEnumerable<T>
        {
            IEnumerable _list;

            public ObjectEnumerable(IEnumerable list)
            {
                _list = list;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                foreach (object node in _list)
                {
                    if (node is T)
                    {
                        yield return (T) node;
                    }
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (object node in _list)
                {
                    if (node is T)
                    {
                        yield return (T) node;
                    }
                }
            }
        }

        public static VM_PagedList<T> GetPagedList<T>(this IEnumerable<T> src, int pageIndex, int pageSize)
        {
            if (src != null)
            {
                var count = src.Count();

                if (pageIndex == 1)
                {
                    return new VM_PagedList<T>(src.Take(pageSize).ToArray(), pageIndex, pageSize, count);
                }
                else
                {
                    return new VM_PagedList<T>(src.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray(), pageIndex,
                        pageSize, count);
                }
            }
            else
            {
                return null;
            }
        }

        public static T[] GetPagedList<T>(this IEnumerable<T> src, int pageIndex, int pageSize, out int totalCount)
        {
            if (src != null)
            {
                totalCount = src.Count();

                if (pageIndex == 1)
                {
                    return src.Take(pageSize).ToArray();
                }
                else
                {
                    return src.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray();
                }
            }
            else
            {
                totalCount = 0;
                return new T[0];
                ;
            }
        }

        public static bool HasData<T>(this IEnumerable<T> src)
        {
            return src != null && src.Any();
        }

        public static bool HasData(this IEnumerable src)
        {
            return src != null;
        }


        public static T[] ToArrayEx<T>(this IEnumerable<T> src)
        {
            if (src == null)
            {
                return new T[0];
            }
            else
            {
                return src.ToArray();
            }
        }
    }

    public static class IQueryableExtMethod
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate,
            bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, int, bool>> predicate,
            bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        ///     转换指定类型的枚举列表为新的类型
        /// </summary>
        /// <typeparam name="TSource">源枚举Item类型</typeparam>
        /// <typeparam name="TResult">目标枚举Item类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static IQueryable<TResult> Cast<TSource, TResult>(this IQueryable<TSource> src,
            ChangeTypeFunc<TSource, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            var lst = new List<TResult>();

            foreach (var source in src)
            {
                lst.Add(func(source));
            }

            return lst.AsQueryable();
        }

        public static VM_PagedList<T> GetPagedList<T>(this IQueryable<T> src, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页码参数不能小于1");
            }

            if (src != null)
            {
                var count = src.Count();

                if (pageIndex <= 0 || pageIndex == 1)
                {
                    return new VM_PagedList<T>(src.Take(pageSize).ToArray(), pageIndex, pageSize, count);
                }
                else
                {
                    return new VM_PagedList<T>(src.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray(), pageIndex,
                        pageSize, count);
                }
            }
            else
            {
                return null;
            }
        }

        public static T[] GetPagedList<T>(this IQueryable<T> src, int pageIndex, int pageSize, out int totalCount)
        {
            if (pageIndex <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页码参数不能小于1");
            }

            if (src != null)
            {
                totalCount = src.Count();

                if (pageIndex == 1)
                {
                    return src.Take(pageSize).ToArray();
                }
                else
                {
                    return src.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray();
                }
            }
            else
            {
                totalCount = 0;
                return null;
            }
        }

        public static IQueryable<T> Where<T>(IQueryable<T> query, String restriction, params Object[] values)
        {
            //Assembly asm = typeof(System.Web.ApplicationServices.CreatingCookieEventArgs).Assembly;
            //Type dynamicExpressionType = asm.GetType("System.Web.Query.Dynamic.DynamicExpression");

            //var t =
            //    dynamicExpressionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            //        .Where(m => (m.Name == "ParseLambda") && (m.GetParameters().Length == 2))
            //        .Single();

            //MethodInfo parseLambdaMethod = dynamicExpressionType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => (m.Name == "ParseLambda") && (m.GetParameters().Length == 2)).Single().MakeGenericMethod(typeof(T), typeof(Boolean));
            //Expression<Func<T, Boolean>> expression = parseLambdaMethod.Invoke(null, new Object[] { restriction, values }) as Expression<Func<T, Boolean>>;

            //return (query.Where(expression));

            return null;
            ;
        }

        public static T[] ToArrayWithNoLock<T>(this IQueryable<T> src)
        {
            var transactionOptions = new System.Transactions.TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
            using (var transactionScope =
                new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required,
                    transactionOptions))
            {
                try
                {
                    return src.ToArray();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    transactionScope.Complete();
                }
            }
        }

        public static List<T> ToListWithNoLock<T>(this IQueryable<T> src)
        {
            var transactionOptions = new System.Transactions.TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
            using (var transactionScope =
                new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required,
                    transactionOptions))
            {
                try
                {
                    return src.ToList();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    transactionScope.Complete();
                }
            }
        }

        public static T FirstOrDefaultWithNoLock<T>(this IQueryable<T> src)
        {
            var transactionOptions = new System.Transactions.TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
            using (var transactionScope =
                new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required,
                    transactionOptions))
            {
                try
                {
                    return src.FirstOrDefault();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    transactionScope.Complete();
                }
            }
        }

#if NET45
/// <summary>
/// 查询动态查询表达式
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="src"></param>
/// <param name="query">动态的查询表达式列表</param>
/// <returns></returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> src,params DynamicLinqQuery[] query) where T:class
        {
            return DynamicLinqQueryBuilder.Build(src, query);
        }
#endif
    }

    public static class IListExtMethod
    {
        public static void AddRangeEx<T>(this List<T> src, IEnumerable<T> datas)
        {
            if (datas.HasData())
            {
                src.AddRange(datas);
            }
        }

        /// <summary>
        ///     使用ForEach循环
        /// </summary>
        /// <typeparam name="T">循环类型</typeparam>
        /// <param name="src"></param>
        /// <param name="excuteFunction"></param>
        public static void ForEachLoop<T>(this IList<T> src, EventHandler<ForEachArgs<T>> excuteFunction)
        {
            if (src == null || src.Count <= 0)
            {
                return;
            }

            foreach (T item in src)
            {
                if (excuteFunction != null)
                {
                    var e = new ForEachArgs<T>(item);

                    excuteFunction(src, e);

                    if (e.Cancel)
                    {
                        break;
                    }
                }
            }
        }

        public static void ForLoop<T>(this IList<T> src, EventHandler<ForEachArgs<int>> excuteFunction)
        {
            if (src == null || src.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < src.Count; i++)
            {
                if (excuteFunction != null)
                {
                    var e = new ForEachArgs<int>(i);

                    excuteFunction(src, e);

                    if (e.Cancel)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     在指定的BindingList中查找所有匹配的项目,并返回
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="checker">自定义的检查的委托,如果为所要的项目,则返回true</param>
        /// <param name="defaultValue">当查找不到时,返回的默认值</param>
        /// <returns>返回一个指定的默认值或查找后的结果</returns>
        public static T[] FindItems<T>(this IList<T> src, Predicate<T> checker, T[] defaultValue)
        {
            if (src.Count <= 0 || checker == null)
            {
                return defaultValue;
            }

            var list = new List<T>(src.Count / 2);

            foreach (var t in src)
            {
                if (checker(t))
                {
                    list.Add(t);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        ///     在指定的BindingList中查找第一个匹配的项目,并返回;默认值为:default(T)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="checker">自定义的检查的委托,如果为所要的项目,则返回true</param>
        /// <returns>返回一个指定的默认值或查找后的结果</returns>
        public static T FindItem<T>(this IList<T> src, Predicate<T> checker)
        {
            return FindItem(src, checker, default(T));
        }

        /// <summary>
        ///     在指定的BindingList中查找第一个匹配的项目,并返回
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="src">源列表</param>
        /// <param name="checker">自定义的检查的委托,如果为所要的项目,则返回true</param>
        /// <param name="defaultValue">当查找不到时,返回的默认值</param>
        /// <returns>返回一个指定的默认值或查找后的结果</returns>
        public static T FindItem<T>(this IList<T> src, Predicate<T> checker, T defaultValue)
        {
            if (src.Count <= 0 || checker == null)
            {
                return defaultValue;
            }

            foreach (var t in src)
            {
                if (checker(t))
                {
                    return t;
                }
            }

            return defaultValue;
        }

        /// <summary>
        ///     合并两个集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="targetList"></param>
        /// <returns></returns>
        public static IList<T> Union<T>(this IList<T> src, IList<T> targetList)
        {
            if (targetList == null && src == null)
            {
                return null;
            }

            if (src == null)
            {
                return targetList;
            }

            if (targetList == null)
            {
                return src;
            }

            var t = new List<T>(src.Count + targetList.Count);

            if (!src.HasData())
            {
                t.AddRange(src);
            }

            if (!targetList.HasData())
            {
                t.AddRange(targetList);
            }

            return t;
        }

        public static IList<T> Remove<T>(this IList<T> src, Predicate<T> checkFunc)
        {
            if (checkFunc == null || src == null || src.Count <= 0)
            {
                return src;
            }

            var tempList = new List<T>(src.Count / 2);

            foreach (var s in src)
            {
                if (checkFunc(s))
                {
                    tempList.Add(s);
                    //src.Remove(s);
                }
            }

            for (int i = 0; i < tempList.Count; i++)
            {
                src.Remove(tempList[i]);
            }

            return src;
        }

        public static int IndexOf<T>(this IList<T> src, Predicate<T> checker, int startIndex = 0)
        {
            for (int i = startIndex; i < src.Count; i++)
            {
                if (checker(src[i]))
                {
                    return i;
                }
            }

            return -1;
        }


        //public static IList Remove(this IList src, Predicate checkFunc)
        //{
        //    if (checkFunc == null || src == null || src.Count <= 0)
        //    {
        //        return src;
        //    }

        //    var tempList = new List<object>();

        //    foreach (var s in src)
        //    {
        //        if (checkFunc(s))
        //        {
        //            tempList.Add(s);
        //        }
        //    }

        //    for (int i = 0; i < tempList.Count; i++)
        //    {
        //        src.Remove(tempList[i]);
        //    }

        //    return src;

        //}

        /// <summary>
        ///     移除列表中所有为null值的项目
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IList<T> RemoveAllNull<T>(this IList<T> src) where T : class
        {
            return Remove(src, (s) => s == null);
        }

        [Obsolete]
        public static bool IsEmptyData<T>(this IList<T> src)
        {
            return HasData(src);

            //if (src==null || src.Count<=0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        //public static bool HasData(this IList src)
        //{
        //    if (src == null || src.Count <= 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static bool HasData<T>(this IList<T> src)
        {
            return src != null && src.Any();
        }


        //public static bool HasData<T>(this IList<T> src)
        //{
        //    if (src == null || src.Count <= 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}

        /// <summary>
        ///     判断从指定startIndex开始，处理length个长度的数据，是否会超出数组的范围
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="lst">要检查的IList接口对象</param>
        /// <param name="startIndex">起始索引值</param>
        /// <param name="length">要检查的长度</param>
        /// <returns></returns>
        public static bool IsInEnableRange<T>(this IList<T> lst, int startIndex, int length)
        {
            if (lst == null || startIndex < 0 || length < 0)
            {
                return false;
            }

            if (startIndex > lst.Count - 1 || length > lst.Count)
            {
                return false;
            }

            if (startIndex + length > lst.Count)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     在给定的列表中,挑选出随机个数的随机项<br/>
        ///     比如实现在src中,随机挑出5个并返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">随机数据源</param>
        /// <returns>返回随机挑选后的数据</returns>
        public static T[] Random<T>(this IList<T> src)
        {
            return Random(src, 0);
        }

        /// <summary>
        ///     在给定的列表中,挑选出指定个数的随机项<br/>
        ///     比如实现在src中,随机挑出5个并返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">随机数据源</param>
        /// <param name="randomCount">需要挑选出的个数,如果为0,则连挑选出的数量也是随机</param>
        /// <returns>返回随机挑选后的数据</returns>
        public static T[] Random<T>(this IList<T> src, int randomCount)
        {
            if (src == null || src.Count <= 0)
            {
                throw new ArgumentNullException("src");
            }

            if (randomCount == 0)
            {
                randomCount = RandomEx.Next(1, src.Count);
            }

            //var newlist = new List<T>(randomCount);

            var newlist = new T[randomCount];

            for (int i = 0; i < randomCount; i++)
            {
                var index = RandomEx.Next(0, src.Count);

                newlist[i] = src[index];
            }

            return newlist;
        }

        /// <summary>
        ///     检查两个列表内所有元素是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">用于比较的源列表</param>
        /// <param name="target">用于比较的另一个列表</param>
        /// <returns></returns>
        public static bool ElementsEquals<T>(this IList<T> src, IList<T> target)
        {
            if (src == null && target == null)
            {
                return true;
            }

            if (src != null && target == null)
            {
                return false;
            }

            if (src == null && target != null)
            {
                return false;
            }

            if (src.Count != target.Count)
            {
                return false;
            }

            for (int i = 0; i < src.Count; i++)
            {
                if (!src[i].SafeEquals(target[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public delegate bool Predicate(object obj);
}