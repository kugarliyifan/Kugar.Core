using System.ComponentModel;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Remoting
{
    public class ClientCompressionSink: BaseChannelSinkWithProperties,  
                                       IClientChannelSink 
   { 
       private IClientChannelSink _nextSink;
        private CompressionMode _compressionMode;

       public ClientCompressionSink(IClientChannelSink next,CompressionMode compressionMode)  
       { 
           _nextSink = next;
           _compressionMode = compressionMode;
       } 
 
        public IClientChannelSink NextChannelSink  
        { 
            get { 
                return _nextSink; 
            } 
        } 
 
 
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack,  
                                        IMessage msg,  
                                        ITransportHeaders headers,  
                                        Stream stream)  
        { 
 
 
            // generate a compressed stream using NZipLib 
            //stream = CompressionHelper.getCompressedStreamCopy(stream);

            stream = stream.CompressStream(); 
 
            // push onto stack and forward the request 
            sinkStack.Push(this,null); 
            _nextSink.AsyncProcessRequest(sinkStack,msg,headers,stream); 
        } 
 
 
        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack,  
                                            object state,  
                                            ITransportHeaders headers,  
                                            Stream stream)  
        { 
 
            // deflate the response 
            //stream =  CompressionHelper.getUncompressedStreamCopy(stream);
            stream =stream.UnCompressStream(); 
            // forward the request 
            sinkStack.AsyncProcessResponse(headers,stream); 
        } 
 
 
        public Stream GetRequestStream(IMessage msg,  
                                       ITransportHeaders headers)
        {
            var s = _nextSink.GetRequestStream(msg, headers);

            if (s!=null && s.CanSeek)
            {
                s.Seek(0, SeekOrigin.Begin);
            }

            return s; 
        }

        private static readonly EnumConverter enumConvertModel = new EnumConverter(typeof(CompressionMode));
        private static readonly EnumConverter enumConvertStatus = new EnumConverter(typeof(CompressionStatus));

        public void ProcessMessage(IMessage msg,  
                                   ITransportHeaders requestHeaders,  
                                   Stream requestStream,  
                                   out ITransportHeaders responseHeaders,  
                                   out Stream responseStream)  
        { 
            // generate a compressed stream using NZipLib 
 
            //Stream localrequestStream  =  CompressionHelper.getCompressedStreamCopy(requestStream);

            ////Stream localrequestStream =requestStream.CompressStream();

            //Stream localresponseStream; 
            //// forward the call to the next sink 
            //_nextSink.ProcessMessage(msg, 
            //                         requestHeaders, 
            //                         localrequestStream,  
            //                         out responseHeaders,  
            //                         out localresponseStream); 
 
            //// deflate the response 
            //responseStream =   CompressionHelper.getUncompressedStreamCopy(localresponseStream); 
            ////responseStream = localresponseStream.UnCompressStream(); 

            bool isNeedCompress = false;

            Stream localrequestStream=requestStream;

            requestHeaders["CompressionMode"] = _compressionMode;

            if (_compressionMode==CompressionMode.None)
            {
                isNeedCompress = false;
            }
            else if (_compressionMode==CompressionMode.Auto && requestStream.Length>1024)
            {
                isNeedCompress = true;
            }
            else if (_compressionMode==CompressionMode.Compress)
            {
                isNeedCompress = true;
            }

            if (isNeedCompress)
            {
                //localrequestStream =CompressionHelper.getCompressedStreamCopy(requestStream);

                localrequestStream =requestStream.CompressStream();
                requestStream.Close();
                requestStream.Dispose();
                requestHeaders["CompressStatus"] =CompressionStatus.Compression;
            }
            else
            {
                requestHeaders["CompressStatus"] = null;
            }
            Stream localresponseStream;
            // forward the call to the next sink
            _nextSink.ProcessMessage(msg,
                                     requestHeaders,
                                     localrequestStream,
                                     out responseHeaders,
                                     out localresponseStream);

            bool isNeedUnCompress = false;

            if (responseHeaders["CompressStatus"] != null)
            {
                var t = enumConvertStatus.ConvertFrom(responseHeaders["CompressStatus"]);

                if (t != null && (CompressionStatus)t==CompressionStatus.Compression)
                {
                    isNeedUnCompress = true;
                }
            }

            if (isNeedUnCompress)
            {
                //responseStream = CompressionHelper.getUncompressedStreamCopy(localresponseStream);
                responseStream = localresponseStream.UnCompressStream();
            }
            else
            {
                responseStream = localresponseStream.CopyToMemory();
            }

            localresponseStream.Close();
            localresponseStream.Dispose();
        } 
    }
}