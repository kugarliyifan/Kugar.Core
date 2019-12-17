using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class DelegateHelper
    {
        public static void Clear(this Delegate del)
        {
            var lst = del.GetInvocationList();

            foreach (var tempdel in lst)
            {
                Delegate.RemoveAll(del, tempdel);
            }
        }

        public static void InvokeIgoreError(this Delegate method,params object[] paramList)
        {
            var lst = method.GetInvocationList();

            foreach (var tempdel in lst)
            {
                try
                {
                    tempdel.DynamicInvoke(paramList);
                }
                catch (Exception)
                {
                }
            }
        }



    //    public static Delegate Combine(this Delegate src, Delegate other)
    //    {
    //        if (other==null || src==null)
    //        {
    //            return src;
    //        }

    //        var otherDelList = other.GetInvocationList();

    //        if (otherDelList==null || otherDelList.Length<=0)
    //        {
    //            return src;
    //        }

    //        return Delegate.Combine(src, other);
    //    }


    }

}
