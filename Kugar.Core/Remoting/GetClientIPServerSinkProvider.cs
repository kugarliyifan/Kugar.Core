using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Kugar.Core.Remoting
{
    public class GetClientInfoServerSinkProvider : IServerChannelSinkProvider
    {
        private IServerChannelSinkProvider _nextProvider;
        private DecodeClientInfo _handler = null;
        private EventHandler _afterCall = null;

        public GetClientInfoServerSinkProvider(DecodeClientInfo handler,EventHandler afterCall)
        {
            _handler = handler;
            _afterCall = afterCall;
        }

        public void GetChannelData(IChannelDataStore channelData)
        {
            
        }

        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            var t = _nextProvider.CreateSink(channel);

            var sink = new GetClientInfoServerSink(t);

            if (_handler!=null)
            {
                sink.SetupClientInfo += _handler;
            }

            if (_afterCall!=null)
            {
                sink.AfterCallMethod += _afterCall;
            }

            return sink;

        }

        public IServerChannelSinkProvider Next
        {
            get { return _nextProvider; }
            set { _nextProvider = value; }
        }
    }
}
