using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Network.Web
{
    public class RequestThreadLocal<T>
    {
        private string _id;
        private Func<T> _valueFactory = null;
        //private ConcurrentDictionary<Type,bool> _typeIsDispose=new ConcurrentDictionary<Type, bool>();
        //private ReaderWriterLockSlim _locker=new ReaderWriterLockSlim();
        
        public RequestThreadLocal(Func<T> valueFactory)
            : this("RequestLocal" + Guid.NewGuid(), valueFactory)
        {
            
        }

        public RequestThreadLocal(string name):this(name,null)
        {
        }

        public RequestThreadLocal()
            : this("", null)
        {
        }


        public RequestThreadLocal(string name,Func<T> valueFactory)
        {
            _id = name;

            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = "RequestLocal" + Guid.NewGuid();
            }

            _valueFactory = valueFactory;
        }

        public T Value
        {
            get
            {
                T v;

                if (isHttp())
                {
                    v = getHttpValue();
                }
                else
                {
                    v = getThreadValue();
                }

                return v;
            }
            set
            {
                if (isHttp())
                {
                    setHttpValue(value);
                }
                else
                {
                    setThreadValue(value);
                }
            }
        }

        public bool IsValueCreated
        {
            get
            {
                if (isHttp())
                {
                    return HttpContext.Current.Items.Contains(_id);
                }
                else
                {
                    try
                    {
                        return CallContext.GetData(_id) != null;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    
                }

            }
        }
        
        public void Reset()
        {
            if (IsValueCreated)
            {
                return;
            }

            if (isHttp())
            {
                HttpContext.Current.Items.Remove(_id);
            }
            else
            {
                CallContext.FreeNamedDataSlot(_id);
            }
        }



        private void OnValueChange()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        

        public event EventHandler ValueChanged;

        private void setHttpValue(T value)
        {
            

            var context = HttpContext.Current;

            try
            {
                if (IsValueCreated)
                {
                    context.Items[_id] = value;
                }
                else
                {
                    context.Items[_id] = value;
                }

                

                OnValueChange();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
            }
        }

        private void setThreadValue(T value)
        {
            try
            {
                if (IsValueCreated)
                {
                    CallContext.SetData(_id,value);
                }
                else
                {
                    CallContext.SetData(_id, value);
                }

                OnValueChange();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
            }
        }

        private T getHttpValue()
        {
            var context = HttpContext.Current;

            T v;

            if (IsValueCreated)
            {
                v = (T)context.Items[_id];
            }
            else
            {
                try
                {
                    if (_valueFactory != null)
                    {
                        v = _valueFactory();

                        context.Items.Add(_id, v);

                        OnValueChange();
                    }
                    else
                    {
                        v = default(T);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {

                }
            }

            return v;
        }

        private T getThreadValue()
        {
            T v;

            if (IsValueCreated)
            {
                v = (T)CallContext.GetData(_id);
            }
            else
            {
                try
                {
                    if (_valueFactory != null)
                    {
                        v = _valueFactory();

                        CallContext.SetData(_id,v);

                        OnValueChange();
                    }
                    else
                    {
                        v = default(T);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {

                }
            }

            return v;
        }

        private bool isHttp()
        {
            try
            {
                if (HttpContext.Current==null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }


        }
    }
}
