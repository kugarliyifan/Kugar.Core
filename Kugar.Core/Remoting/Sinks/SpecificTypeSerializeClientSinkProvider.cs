using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Kugar.Core.Remoting.Sinks
{
    /// <summary>
    /// UAPClientSinkProvider 的摘要说明。
    /// </summary>
    public class SpecificTypeSerializeClientSinkProvider : IClientChannelSinkProvider
    {

        public SpecificTypeSerializeClientSinkProvider(IDictionary properties, ICollection providerData)
        {

        }

        #region IClientChannelSinkProvider 成员

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            return new SpecificTypeSerializeClientSink(m_Next.CreateSink(channel, url, remoteChannelData));
        }

        private IClientChannelSinkProvider m_Next;
        public IClientChannelSinkProvider Next
        {
            get
            {
                return m_Next;
            }
            set
            {
                m_Next = value;
            }
        }

        #endregion
    }

}
