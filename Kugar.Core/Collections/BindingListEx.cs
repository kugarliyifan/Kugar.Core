using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kugar.Core.Reflection;
using System.Collections;

namespace Kugar.Core.Collections
{
    public class BindingListEx<T> : BindingList<T>, ITypedList
    {
        public BindingListEx() : base() { }

        public BindingListEx(IList<T> srcList) : base(srcList) { }

        public bool IsSubsetProperty { set; get; }

        #region "ITypedList接口处理"

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "BindingListEx";
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (IsSubsetProperty)
            {
                var provider = new SubsetTypeDescriptionProvider<T>(TypeDescriptor.GetProvider(typeof (T)));

                var cols = provider.GetTypeDescriptor(typeof (T)).GetProperties();

                return cols;

            }
            else
            {
                return TypeDescriptor.GetProperties(typeof (T));
            }
        }
        #endregion

    }
}
