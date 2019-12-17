using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Kugar.Core.Remoting
{
    /// <summary>
    /// 
    /// </summary>
    public class SetRequestInfoClientSink : BaseChannelSinkWithProperties,
        IClientChannelSink
    {
        private IClientChannelSink _nextSink;

        public SetRequestInfoClientSink(IClientChannelSink next)
        {
            _nextSink = next;
        }

        public event ClientRequestInfoHandler BeforeSendRequest;

        public IClientChannelSink NextChannelSink
        {
            get
            {
                return _nextSink;
            }
        }


        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack,
            IMessage msg,
            ITransportHeaders headers,
            Stream stream)
        {

            // push onto stack and forward the request 
            sinkStack.Push(this, null);
            _nextSink.AsyncProcessRequest(sinkStack, msg, headers, stream);
        }


        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack,
            object state,
            ITransportHeaders headers,
            Stream stream)
        {

            // forward the request 
            sinkStack.AsyncProcessResponse(headers, stream);
        }


        public Stream GetRequestStream(IMessage msg,
            ITransportHeaders headers)
        {
            

            var s = _nextSink.GetRequestStream(msg, headers);

            if (s != null && s.CanSeek)
            {
                s.Seek(0, SeekOrigin.Begin);
            }

            return s;
        }


        public void ProcessMessage(IMessage msg,
            ITransportHeaders requestHeaders,
            Stream requestStream,
            out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {

            try
            {
                if (BeforeSendRequest!=null)
                {
                    BeforeSendRequest(requestHeaders, requestStream);
                }
            }
            catch (Exception ex)
            {
            }


            _nextSink.ProcessMessage(msg, requestHeaders, requestStream,
                out responseHeaders, out responseStream);
        }
    }

    public delegate void ClientRequestInfoHandler(ITransportHeaders requestHeaders, Stream requestStream);

    public class SetRequestInfoClientSinkProvider : IClientChannelSinkProvider
    {
        private IClientChannelSinkProvider _nextProvider;
        private ClientRequestInfoHandler _handler;
        private EventHandler _afterCall;

        public SetRequestInfoClientSinkProvider(ClientRequestInfoHandler handler,EventHandler afterCall)
        {
            // not yet needed 
            _handler = handler;
            _afterCall = afterCall;
        }

        public IClientChannelSinkProvider Next
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

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            // create other sinks in the chain 
            IClientChannelSink next = _nextProvider.CreateSink(channel,
                url,
                remoteChannelData);

            var sink=new SetRequestInfoClientSink(next);

            if (_handler!=null)
            {
                sink.BeforeSendRequest += _handler;
            }
            

            // put our sink on top of the chain and return it                 
            return sink;
        }
    }
}