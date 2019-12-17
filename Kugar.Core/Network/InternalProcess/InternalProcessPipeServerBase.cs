using System;

namespace Kugar.Core.Network.InternalProcess
{
    public abstract class InternalProcessPipeServerBase:MarshalByRefObject
    {
        protected InternalProcessPipeServerBase()
        {
            InternalProcessPipeBrocker.RegisterServer(this);
        }

        public abstract string ServerName { get; }
        
    }
}