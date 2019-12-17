using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Services;
using System.Text;

namespace Kugar.Core.Remoting
{
    /// <summary>
    ///     用于解决Remoting在外网无法连接的错误<br/>
    ///     参考:http://www.glacialcomponents.com/ArticleDetails/CAOMN.aspx
    /// </summary>
    /// <see cref="http://www.glacialcomponents.com/ArticleDetails/CAOMN.aspx"/>
    public class WanTrackingHandler : ITrackingHandler
    {
        // Notify a handler that an object has been marshaled.
        public void MarshaledObject(Object obj, ObjRef or)
        {
            // Assumption: We have a server channel sink that sets a call context flag
            // called "ObservedIP" whenever we are processing a remote request
            object ObservedIP = CallContext.GetData("ObservedIP");

            var o = CallContext.GetData("ObservedIPStr");
                
            // for local clients we don't do anything here
            // if they don't specify the remote IP then we just use the servers IP
            if (ObservedIP == null)
                return;

            if (or.ChannelInfo == null)
                return;

            string strAddress = (string)ObservedIP;

            for (int i = or.ChannelInfo.ChannelData.GetLowerBound(0);
                            i <= or.ChannelInfo.ChannelData.GetUpperBound(0); i++)
            {
                // Check for the ChannelDataStore object that we don't want to copy
                if (or.ChannelInfo.ChannelData[i] is ChannelDataStore)
                {
                    // Personally I don't know why ChannelURIs is an array... I am only
                    // familiar with there being one URI in each ChannelDataStore object
                    foreach (var uri in ((ChannelDataStore)or.ChannelInfo.ChannelData[i]).ChannelUris)
                    {
                        // this will get the first part of the uri
                        int nOffset = uri.IndexOf("//") + 2;

                        string strNewUri = uri.Substring(0, nOffset);
                        strNewUri += strAddress;
                        nOffset = uri.IndexOf(":", nOffset);
                        strNewUri += uri.Substring(nOffset, uri.Length - nOffset);
                        string[] strarray = new string[1] { strNewUri };

                        var cds = new ChannelDataStore(strarray);
                        or.ChannelInfo.ChannelData[i] = cds;
                    }
                }
            }
        }

        public void UnmarshaledObject(object obj, ObjRef or)
        {
            
        }

        public void DisconnectedObject(object obj)
        {
            
        }
    }
}
