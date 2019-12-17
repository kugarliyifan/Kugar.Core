using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.Exceptions
{
    public class ArgumentTypeNotMatchException:System.ArgumentException
    {
        public ArgumentTypeNotMatchException(string paramName, string typeName)
            : base("输入类型错误，类型必须为:" + typeName, paramName)
        {
        }
    }
}
