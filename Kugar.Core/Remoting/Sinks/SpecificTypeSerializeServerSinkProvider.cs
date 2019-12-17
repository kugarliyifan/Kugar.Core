using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Kugar.Core.Remoting.Sinks
{
    /// <summary>
    /// UAPServerSinkProvider 的摘要说明。
    /// </summary>
    public class SpecificTypeSerializeServerSinkProvider : IServerChannelSinkProvider
    {
        public SpecificTypeSerializeServerSinkProvider(IDictionary properties, ICollection providerData)
        {

        }

        #region IServerChannelSinkProvider 成员

        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            IServerChannelSink nextSink = m_Next.CreateSink(channel);
            return new SpecificTypeSerializeServerSink(nextSink);
        }

        private IServerChannelSinkProvider m_Next;
        public IServerChannelSinkProvider Next
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

        public void GetChannelData(IChannelDataStore channelData)
        {
            //not need
        }

        #endregion
    }
}
