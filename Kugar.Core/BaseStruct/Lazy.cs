using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System
{
    #if NET2 && !NET4

    public class Lazy<T>
    {
        private Func<T>  _valueFactory;
        private T _value;
        private bool isValueCreated ;
        private object lockerObj=new object();

        public Lazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
        }

        public T Value
        {
           get
            {
                if (!isValueCreated)
                {
                    lock (lockerObj)
                    {
                        if (!isValueCreated)
                        {
                            _value = _valueFactory();
                            isValueCreated = true;
                        }
                    }
                }

                return _value;
            }
        }

        public void Reset()
        {
            if (!isValueCreated)
            {
                return;
            }

            lock (lockerObj)
            {
                isValueCreated = false;
                _value = default(T);
            }
        }


        public bool IsValueCreated { get { return isValueCreated; } }
    }

#endif
}
