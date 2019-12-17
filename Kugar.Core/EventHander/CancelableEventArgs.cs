using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core
{
    public class CancelableEventArgs:EventArgs
    {
        protected bool _cancel = false;

        public bool Cancel
        {
            set { _cancel = value; }
            get { return _cancel; }
        }

        public Exception Error { set; get; }
    }

    public class CancelableEventArgs<T> : CancelableEventArgs
    {
        public CancelableEventArgs(T data)
        {
            Data = data;
        }

        public T Data { get; private set; }

    }

    public class ValueEventArgs : EventArgs
    {
        public ValueEventArgs(object value)
        {
            Value = value;
        }

        public object Value { set; get; }
    }
}
