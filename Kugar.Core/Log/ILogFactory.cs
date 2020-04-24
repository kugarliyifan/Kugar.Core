using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.Log
{
    public interface ILoggerFactory
    {

        /// <summary>
        ///     获取指定名称的日志记录器
        /// </summary>
        /// <param name="loggerName">日志记录器的名称</param>
        /// <returns></returns>
        ILogger GetLogger(string loggerName);

        /// <summary>
        /// 获取默认的日志器
        /// </summary>
        ILogger Default { get; }
    }
}
