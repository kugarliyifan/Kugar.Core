using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kugar.Core.BaseStruct
{
    public class SequenceDataHandlerList:IEnumerable
    {
        private Action<object> _handler = null;
        private Queue _queue=new Queue(); 
        private ReaderWriterLockSlim _locker=new ReaderWriterLockSlim();

        public SequenceDataHandlerList()
        {
            
        }

        public SequenceDataHandlerList(Action<object> handler):this()
        {
            _handler = handler;
        }

        public void EnQueue(object item)
        {
            _locker.EnterWriteLock();

            try
            {
                _queue.Enqueue(item);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _locker.ExitWriteLock();
            }

            SequenceDataHandlerListManager.Active(this);

        }

        public object DeQueue()
        {
            _locker.EnterWriteLock();

            object item=null;

            if (_queue.Count>0)
            {
                item = _queue.Dequeue();
            }

            _locker.ExitWriteLock();

            return item;
        }

        internal void Active()
        {
            
        }

        public bool IsAutoHandle { set; get; }


        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal static class SequenceDataHandlerListManager
    {
        

        internal static void Active(SequenceDataHandlerList list)
        {
            
        }

        internal static void Managed(SequenceDataHandlerList list)
        {
            
        }
    }
}
