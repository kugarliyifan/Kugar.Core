using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.ExtMethod
{
    public static class IListExtMethodPart2
    {
        public static TValue GetByIndex<TKey,TValue>(this SortedList<TKey,TValue> lst,int index )
        {
            if (lst==null)
            {
                throw new ArgumentNullException("lst");
            }

            if ((index < 0) || (index >= lst.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return lst.Values[index];

            //return lst.IndexOfValue(index);
        }

        //public static CollectionBase Remove(this CollectionBase src, Predicate<object> checkFunc)
        //{
        //    if (checkFunc == null || src == null || src.Count <= 0)
        //    {
        //        return src;
        //    }

        //    var tempList = new List<object>(src.Count);

        //    for (int i = 0; i < src.Count; i++)
        //    {
                
        //    }

        //    foreach (var obj in src)
        //    {
        //        if (checkFunc(obj))
        //        {
        //            tempList.Add(src);
        //        }
        //    }

        //    if (tempList.Count > 0)
        //    {
        //        for (int i = tempList.Count-1; i < 0; i--)
        //        {
        //            src.RemoveAt(tempList[i]);
        //        }

        //        //foreach (var o in tempList)
        //        //{
        //        //    for (int i = 0; i < src.Count; i++)
        //        //    {
        //        //        if (src.)
        //        //        {

        //        //        }
        //        //    }
        //        //}
        //    }

        //    //for (int i = 0; i < tempList.Count; i++)
        //    //{
        //    //    src.RemoveAt(tempList[i]);
        //    //}

        //    return src;

        //}
    }
}
