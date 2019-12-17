using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class BindingListExt
    {
        public static BindingList<T> AddRange<T>(this BindingList<T> bd, IEnumerable<T> lst)
        {
            if (lst == null)
            {
                return bd;
            }

            foreach (var l in lst)
            {
                bd.Add(l);
            }

            return bd;
        }
    }
}
