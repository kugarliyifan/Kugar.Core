using System.Collections;
using System.Runtime.Remoting.Channels;

namespace Kugar.Core.Remoting
{
    public class ClientCompressionSinkProvider : IClientChannelSinkProvider
    {
        private IClientChannelSinkProvider _nextProvider;
        private CompressionMode _compressionMode;

        public ClientCompressionSinkProvider(IDictionary properties, ICollection providerData,CompressionMode compressionMode)  
        { 
            // not yet needed 
            _compressionMode = compressionMode;
        } 
 
        public IClientChannelSinkProvider Next 
        { 
            get { 
                return _nextProvider; 
            } 
            set { 
                _nextProvider = value; 
            } 
        } 

        public CompressionMode Model
        {
            get { return _compressionMode; }
        }
 
        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)  
        { 
            // create other sinks in the chain 
            IClientChannelSink next = _nextProvider.CreateSink(channel, 
                url, 
                remoteChannelData);     
     
            // put our sink on top of the chain and return it                 
            return new ClientCompressionSink(next, _compressionMode); 
        } 
    }
}