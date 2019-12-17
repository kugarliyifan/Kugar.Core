using System;

namespace Kugar.Core.Exceptions
{
    public class ObjectNotNullEnableException:System.Exception
    {
        private const string errorMsg = "{0}参数值不能为空";
        public ObjectNotNullEnableException(string paramName) : base(string.Format(errorMsg,paramName)){}
    }
}