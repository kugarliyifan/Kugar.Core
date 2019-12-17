using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Kugar.Core.Remoting
{
    public class MethodReturnWrapper : IMethodReturnMessage
    {
        private IMethodReturnMessage imr = null;
        private object _returnValue = null;

        public MethodReturnWrapper(IMethodReturnMessage msg)
        {
            imr = msg;
            _returnValue = imr.ReturnValue;
        }

        public void SetReturnValue(object value)
        {
            _returnValue = value;
        }


        public IDictionary Properties
        {
            get { return imr.Properties; }
        }

        public string GetArgName(int index)
        {
            return imr.GetArgName(index);
        }

        public object GetArg(int argNum)
        {
            return imr.GetArg((argNum));
        }

        public string Uri
        {
            get { return imr.Uri; }
        }

        public string MethodName
        {
            get { return imr.MethodName; }
        }

        public string TypeName
        {
            get { return imr.TypeName; }
        }

        public object MethodSignature
        {
            get { return imr.MethodSignature; }
        }

        public int ArgCount
        {
            get { return imr.ArgCount; }
        }

        public object[] Args
        {
            get { return imr.Args; }
        }

        public bool HasVarArgs
        {
            get { return imr.HasVarArgs; }
        }

        public LogicalCallContext LogicalCallContext
        {
            get { return imr.LogicalCallContext; }
        }

        public MethodBase MethodBase
        {
            get { return imr.MethodBase; }
        }

        public string GetOutArgName(int index)
        {
            return imr.GetOutArgName(index);
        }

        public object GetOutArg(int argNum)
        {
            return imr.GetOutArg(argNum);
        }

        public int OutArgCount
        {
            get { return imr.OutArgCount; }
        }

        public object[] OutArgs
        {
            get { return imr.OutArgs; }
        }

        public Exception Exception
        {
            get { return imr.Exception; }
        }

        public object ReturnValue
        {
            get { return _returnValue; }
        }
    }
}
