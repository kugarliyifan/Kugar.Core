using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.Remoting
{
    /// <summary>
    ///     remoting的远程回调模块,支持lambda表达式
    /// </summary>
    /// <typeparam name="T">EventHandler的第二个参数类型 </typeparam>
    public class CallBackBlock<T> : MarshalByRefObject where T : EventArgs
    {
        public CallBackBlock(EventHandler<T> e1)
        {
            var att = typeof(T).GetCustomAttributes(true);

            var isok = false;

            foreach (var a in att)
            {
                if (a is SerializableAttribute)
                {
                    isok = true;
                }
            }

            if (!isok)
            {
                throw new TypeLoadException("委托所定义的类型不包含Serializable标记");
            }

            callback += e1;

        }

        private event EventHandler<T> callback;

        public void OnCallBack(object sender, T e)
        {
            if (callback != null)
            {
                callback(sender, e);
            }
        }

        public bool IsSameDelegate(EventHandler<T> tt)
        {
            if (callback == tt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public EventHandler<T> CallBack
        {
            get
            {
                return callback;
            }
        }

    }
}
