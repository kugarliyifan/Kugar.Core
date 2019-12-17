using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Kugar.Core.Remoting
{
    // implementation SAOCAOClassFactory.cs
    public class RemotingCAOInstance<T> : MarshalByRefObject, ICAOInterfaceFactory where T : MarshalByRefObject, new()
    {
        #region Implementation of ICAOInterfaceFactory

        public object CreateInstance()
        {
            CallContext.FreeNamedDataSlot("ObservedIP");
            return new T(); // class factory create
        }

        public object CreateInstance(string serverIPAddress)
        {
            CallContext.SetData("ObservedIP", serverIPAddress);
            return new T(); // class factory create
        }

        #endregion
    }
}
