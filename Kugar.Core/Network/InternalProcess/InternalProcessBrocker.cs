using System;
using System.Collections.Concurrent;
using Fasterflect;

namespace Kugar.Core.Network.InternalProcess
{
    internal static class InternalProcessPipeBrocker
    {
        private static ConcurrentDictionary<string, InternalProcessPipeServerBase> _serverCache=new ConcurrentDictionary<string, InternalProcessPipeServerBase>(StringComparer.CurrentCultureIgnoreCase);
        
        public static void RegisterServer(InternalProcessPipeServerBase server)
        {
            if (!_serverCache.TryAdd(server.ServerName, server))
            {
                throw new ArgumentOutOfRangeException(nameof(server),"已存在相同名称的服务,无法发布");
            }
        }

        public static object Call(string serverName, string methodName, object[] args)
        {
            if (_serverCache.TryGetValue(serverName, out var server))
            {
                return server.CallMethod(methodName, args);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(serverName),"服务提供方未存在");
            }
        }

        public static object CallGeneric(string serverName, string methodName, Type[] types, object[] args)
        {
            if (_serverCache.TryGetValue(serverName, out var server))
            {
                return server.CallMethod(methodName,types, args);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(serverName), "服务提供方未存在");
            }
        }
    }
}