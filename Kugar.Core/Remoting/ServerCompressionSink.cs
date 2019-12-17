using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Remoting
{
    public class ServerCompressionSink : BaseChannelSinkWithProperties, IServerChannelSink //,IChannelSinkBase
    {

        private IServerChannelSink _nextSink;
        private CompressionMode _compressMode = CompressionMode.None;

        public ServerCompressionSink(IServerChannelSink next, CompressionMode compressMode)
        {
            _nextSink = next;
            _compressMode = compressMode;
        }

        public IServerChannelSink NextChannelSink
        {
            get
            {
                return _nextSink;
            }
        }

        public CompressionMode CompressMode
        {
            get { return _compressMode; }
        }

        //大于10K的数据才进行压缩
        private const int compressLength = 1024 * 1024 * 10;

        //异步方式    
        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack,
            object state,
            IMessage msg,
            ITransportHeaders headers,
            Stream stream)
        {
            //如果客户端指定了压缩的标志，压缩数据流，默认为不压缩
            object compress = headers["Compress"];
            if (compress != null && compress.ToString() == "True")
            {
                // 压缩数据流
                //stream = CompressionHelper.getCompressedStreamCopy(stream);

                stream =stream.CompressStream();

                headers["Compress"] = true;

                // forwarding to the stack for further processing
                sinkStack.AsyncProcessResponse(msg, headers, stream);

            }
            else
            {
                _nextSink.AsyncProcessResponse(sinkStack, state, msg, headers, stream);
            }


        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack,
            object state,
            IMessage msg,
            ITransportHeaders headers)
        {
            return null;
        }

        private static readonly EnumConverter enumConvert = new EnumConverter(typeof(CompressionMode));
        private static readonly EnumConverter enumConvertStatus = new EnumConverter(typeof(CompressionStatus));
        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack,
            IMessage requestMsg,
            ITransportHeaders requestHeaders,
            Stream requestStream,
            out IMessage responseMsg,
            out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {
            Stream localrequestStream = requestStream;

            // 解压请求的数据流,using NZipLib

            if (requestHeaders["CompressStatus"] != null )
            {
                var t = enumConvertStatus.ConvertFrom(requestHeaders["CompressStatus"]);
                if (t != null && (CompressionStatus)t == CompressionStatus.Compression)
                {
                    //localrequestStream = CompressionHelper.getUncompressedStreamCopy(requestStream);
                    localrequestStream = requestStream.UnCompressStream();

                    requestStream.Close();
                    requestStream.Dispose();
                }

            }

            //var localrequestStream =requestStream.UnCompressStream();

            // pushing onto stack and forwarding the call
            sinkStack.Push(this, null);

            Stream localresponseStream;
            ServerProcessing srvProc = _nextSink.ProcessMessage(sinkStack,
                                                                requestMsg,
                                                                requestHeaders,
                                                                localrequestStream,
                                                                out responseMsg,
                                                                out responseHeaders,
                                                                out localresponseStream);

            CompressionMode compressionMode =this.CompressMode;

            if (requestHeaders["CompressionMode"] != null)
            {
                var t = enumConvert.ConvertFrom(requestHeaders["CompressionMode"]);

                if (t != null)
                {
                    compressionMode = (CompressionMode)t;
                }
            }

            bool isNeedCompress = false;

            if (compressionMode == CompressionMode.None)
            {
                isNeedCompress = false;
            }
            else if (compressionMode == CompressionMode.Auto && localresponseStream.Length > 1024)
            {
                isNeedCompress = true;
            }
            else if (compressionMode == CompressionMode.Compress)
            {
                isNeedCompress = true;
            }


            if (isNeedCompress)
            {
                // compressing the response
                //responseStream = CompressionHelper.getCompressedStreamCopy(localresponseStream);
                responseStream = localresponseStream.CompressStream();

                localresponseStream.Close();
                localresponseStream.Dispose();

                responseHeaders["CompressStatus"] = CompressionStatus.Compression;
                return srvProc;
            }
            else
            {
                responseHeaders["CompressStatus"] = null;

                responseStream = localresponseStream;

                return srvProc;

                //return _nextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            }


            //responseStream = localresponseStream.CompressStream();

            // returning status information





            //}
            //else
            //{
            //    //System.Net.IPAddress ip = requestHeaders[CommonTransportKeys.IPAddress] as System.Net.IPAddress;
            //    return _nextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            //}

        }
    }
}
