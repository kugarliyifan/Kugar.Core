using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Runtime.Remoting;

namespace Kugar.Core.ExtMethod
{
#if NET45

    public static class RemotingExtMethod
    {
        /// <summary>
        ///     取消信道的注册
        /// </summary>
        /// <param name="channel"></param>
        public static void Unregister(this IChannel channel)
        {
            if (channel is HttpChannel)
            {
                ((HttpChannel)channel).StopListening(null);
            }
            else if (channel is TcpChannel)
            {
                ((TcpChannel)channel).StopListening(null);
            }

            ChannelServices.UnregisterChannel(channel);
        }

        public static bool IsRemotingObj(this object srcObj)
        {
            return RemotingServices.IsTransparentProxy(srcObj);
        }
    }
#endif
}
