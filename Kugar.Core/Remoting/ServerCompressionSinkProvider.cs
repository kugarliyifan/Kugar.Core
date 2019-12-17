using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Kugar.Core.Remoting
{
    public class ServerCompressionSinkProvider:IServerChannelSinkProvider
    {
        private IServerChannelSinkProvider _nextProvider;
        private CompressionMode _compressMode = CompressionMode.None;

        public ServerCompressionSinkProvider(IDictionary properties, ICollection providerData,CompressionMode compressMode)
        {
            // not yet needed 
            _compressMode = compressMode;
        }

        public IServerChannelSinkProvider Next
        {
            get
            {
                return _nextProvider;
            }
            set
            {
                _nextProvider = value;
            }
        }

        public CompressionMode CompressMode
        {
            get { return _compressMode; }
        }

        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            // create other sinks in the chain 
            IServerChannelSink next = _nextProvider.CreateSink(channel);

            // put our sink on top of the chain and return it                 
            return new ServerCompressionSink(next, this.CompressMode);
        }

        public void GetChannelData(IChannelDataStore channelData)
        {
            // not yet needed 
        }
    }
}
