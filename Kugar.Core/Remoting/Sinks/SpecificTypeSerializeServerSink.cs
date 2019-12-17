using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Linq;
using ExpressionSerialization;

namespace Kugar.Core.Remoting.Sinks
{
    public class SpecificTypeSerializeServerSink : BaseChannelObjectWithProperties, IServerChannelSink, IChannelSinkBase
    {
        IServerChannelSink _next;

        public SpecificTypeSerializeServerSink(IServerChannelSink next)
        {
            _next = next;
        }

        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
        {

            if (requestMsg == null)
            {
                sinkStack.Push(this, null);

                return _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);

            }

            var lcc = (LogicalCallContext)requestMsg.Properties["__CallContext"];

            if (lcc != null)
            {

                if (lcc.GetData("NotSurrogate_Server") != null &&
                    ((LogicalCallContextData)lcc.GetData("NotSurrogate_Server")).Data == "true")
                {
                    sinkStack.Push(this, null);

                    return _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);

                }

                var serializer = new ExpressionSerializer();

                if (requestMsg != null && requestMsg is IMethodCallMessage)
                {
                    //恢复传入DataSet的参数值
                    Type[] ts = (Type[])requestMsg.Properties["__MethodSignature"];
                    Object[] vs = (Object[])requestMsg.Properties["__Args"];
                    var isNeedToSetValue = false;
                    if (ts != null)
                    {
                        for (int i = 0; i <= ts.Length - 1; i++)
                        {
                            //ts[i] is Expression

                            if (vs[i] is Expression)//ts[i] == typeof(Expression) || ts[i].BaseType == typeof(Expression))
                            {
                                isNeedToSetValue = true;

                                XElement rootElement = (XElement)lcc.GetData("Expression_" + i.ToString());

                                Expression exp = null;

                                try
                                {
                                    exp = serializer.Deserialize(rootElement); ;
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                vs[i] = exp;
                                lcc.SetData("Expression_" + i.ToString(), null);
                            }
                        }

                        if (isNeedToSetValue)
                        {
                            requestMsg = new MessageWapper(requestMsg);

                            requestMsg.Properties["__Args"] = vs;

                            //requestMsg.Properties.Remove("__Args");
                            //requestMsg.Properties.Add("__Args",vs);
                            //requestMsg.Properties[4] = vs;
                        }

                    }

                }

                sinkStack.Push(this, null);

                ServerProcessing spres = _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);

                if (responseMsg is IMethodReturnMessage)
                {
                    IMethodReturnMessage returnMsg = responseMsg as IMethodReturnMessage;
                    if (returnMsg != null)
                    {
                        //包装ReturnValue中DataSet为DataSetSurrogate
                        lcc = (LogicalCallContext)returnMsg.Properties["__CallContext"];
                        Object retval = returnMsg.ReturnValue;

                        if (retval != null && retval is Expression)
                        {
                            //System.Data.DataSet ds = (System.Data.DataSet)retval;
                            //DataSetSurrogate dss = new DataSetSurrogate(ds);

                            XElement serialRetValue = null;

                            try
                            {
                                serialRetValue = serializer.Serialize((Expression)retval);
                            }
                            catch (Exception)
                            {
                                serialRetValue = null;
                            }

                            if (serialRetValue != null)
                            {
                                lcc.SetData("Expression_Return", serialRetValue.ToString());
                            }
                            else
                            {
                                lcc.SetData("Expression_Return", string.Empty);
                            }



                            MethodReturnWrapper mrw = new MethodReturnWrapper(returnMsg);
                            //Object nds = Activator.CreateInstance(retval.GetType());

                            mrw.SetReturnValue(Expression.Empty());

                            responseMsg = mrw;
                        }
                    }
                }

                return spres;
            }
            else
            {
                sinkStack.Push(this, null);
                return _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
            }
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            _next.AsyncProcessResponse(sinkStack, state, msg, headers, stream);
        }

        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public IServerChannelSink NextChannelSink
        {
            get { throw new NotImplementedException(); }
        }
    }
}
