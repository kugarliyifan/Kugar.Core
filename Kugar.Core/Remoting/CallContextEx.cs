using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Remoting
{
    public static class CallContextEx
    {
        /// <summary>
        ///     获取本次连接中的客户端IP地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetClientIP()
        {
            return (IPAddress) CallContext.GetData("ClientIP");
        }

        /// <summary>
        ///     获取本次连接中的RequestUri值
        /// </summary>
        /// <returns></returns>
        public static string GetRequestUri()
        {
            return CallContext.GetData("RequestUri").ToStringEx();
        }

        /// <summary>
        ///     获取本次连接中的ConnectionID值
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionID()
        {
            return CallContext.GetData("ConnectionId").ToStringEx();
        }

        public static void SetData(string name,object value)
        {
            CallContext.SetData(name,value);
        }

        public static object GetData(string name)
        {
            return GetData(name, null);
        }

        public static object GetData(string name,object defaultValue)
        {
            var value = CallContext.GetData(name);

            if (value==null)
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        public static T GetData<T>(string name)
        {
            return GetData<T>(name, default(T));
        }

        public static T GetData<T>(string name, T defaultValue)
        {

            var value = CallContext.GetData(name);

            if (value == null)
            {
                return defaultValue;
            }
            else
            {
                return (T)value;
            }
        }
    }
}
