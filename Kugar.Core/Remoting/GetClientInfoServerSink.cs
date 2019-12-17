using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Remoting
{
    public class GetClientInfoServerSink : BaseChannelObjectWithProperties, IServerChannelSink
    {
        private IServerChannelSink _NextSink = null;

        public GetClientInfoServerSink(IServerChannelSink nextSink)
        {
            _NextSink = nextSink;
        }

        public event DecodeClientInfo SetupClientInfo;

        public event EventHandler AfterCallMethod;

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            try
            {
                var clientIP = requestHeaders[CommonTransportKeys.IPAddress].ToIPAddress(null);
                var requestUri = requestHeaders[CommonTransportKeys.RequestUri].ToStringEx();
                var connectID = requestHeaders[CommonTransportKeys.ConnectionId].ToStringEx();
                CallContext.SetData("ClientIP",clientIP);
                CallContext.SetData("RequestUri",requestUri);
                CallContext.SetData("ConnectionId",connectID);

                if (SetupClientInfo!=null)
                {
                    SetupClientInfo(requestMsg, requestHeaders, requestStream);
                }
            }
            catch (Exception)
            {
                
            }

            sinkStack.Push(this,null);

            var srvProc = _NextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg,
                                                   out responseHeaders, out responseStream);

            if (srvProc==ServerProcessing.Complete)
            {
                if (AfterCallMethod!=null)
                {
                    AfterCallMethod(this,EventArgs.Empty);
                }
            }

            return srvProc;
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            try
            {
                var clientIP = headers[CommonTransportKeys.IPAddress].ToIPAddress(null);

                CallContext.SetData("ClientIP",clientIP);
            }
            catch (Exception)
            {
                
            }

            sinkStack.AsyncProcessResponse(msg,headers,stream);


        //            Try
        //    Dim ipa As IPAddress = headers(CommonTransportKeys.IPAddress)
        //    CallContext.SetData("ClientIP", ipa.ToString)

        //Catch ex As Exception

        //End Try
        //sinkStack.AsyncProcessResponse(msg, headers, stream)

        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public IServerChannelSink NextChannelSink
        {
            get { return _NextSink; }
        }
    }

    public delegate void DecodeClientInfo(IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream);
}
