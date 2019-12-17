using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.Exceptions
{
    /// <summary>
    /// 异常处理程序
    /// </summary>
    public static class ExceptionHelper
    {

        public static string ToStringEx(this Exception error)
        {
            StringBuilder fs = new StringBuilder(256);
            fs.AppendLine("异常信息：" + error.Message);
            fs.AppendLine("异常对象：" + error.Source);
            fs.AppendLine("调用堆栈：\n" + error.StackTrace);
            fs.AppendLine("触发方法：" + error.TargetSite);
            fs.AppendLine();
            
            return fs.ToString();
        }

        public static void ThrowIfNull(object args,string argName)
        {
            if (args==null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public static void ThrowArgumentOutOfRange(string name)
        {
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
