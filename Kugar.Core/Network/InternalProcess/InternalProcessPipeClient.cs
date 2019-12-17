using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.Network.InternalProcess
{
    public class InternalProcessPipeClient:MarshalByRefObject
    {
        private string _serverName = "";
        
        public InternalProcessPipeClient(string serverName)
        {
            _serverName = serverName;
        }

        public object Call(string methodName,params object[] args)
        {
            return InternalProcessPipeBrocker.Call(_serverName, methodName, args);
        }

        public object CallGeneric(string methodName, Type[] genericTypes, params object[] args)
        {
            return InternalProcessPipeBrocker.CallGeneric(_serverName,methodName, genericTypes, args);
        }
    }
}
