using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Linq;
using ExpressionSerialization;
using Kugar.Core.Expressions;

namespace Kugar.Core.Remoting.Sinks
{
    public class SpecificTypeSerializeClientSink : BaseChannelObjectWithProperties, IMessageSink, IClientChannelSink
    {
        private IMessageSink _nextMsgSink;
        public IClientChannelSink _nextClientSink;
        ExpressionSerializer serialer = new ExpressionSerializer();

        public SpecificTypeSerializeClientSink(object next)
        {
            _nextMsgSink = (IMessageSink)next;
            _nextClientSink = (IClientChannelSink)next;
        }

        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMessage myReturnMsg = null;

            if (CallContext.GetData("NotSurrogate") != null && (bool)CallContext.GetData("NotSurrogate"))
            {
                CallContext.SetData("NotSurrogate_Server", new LogicalCallContextData("true"));
                IMessage retmsg = _nextMsgSink.SyncProcessMessage(msg);
                CallContext.SetData("NotSurrogate", null);
                return retmsg;
            }
            else
            {
                CallContext.SetData("NotSurrogate_Server", new LogicalCallContextData("false"));
            }

            LogicalCallContext lcc = (LogicalCallContext)msg.Properties["__CallContext"];

            if (msg is IMethodCallMessage)
            {
                var oldValue = msg.Properties["__Args"];

                Type[] ts = (Type[])msg.Properties["__MethodSignature"];
                Object[] vs = (Object[])msg.Properties["__Args"];

                var callMsg = (IMethodCallMessage) msg;

                if (ts != null)
                {
                    var isNeedToSetValue = false;

                    for (int i = 0; i <= ts.Length - 1; i++)
                    {
                        if ((vs[i] is Expression) && vs[i] != null)
                        {
                            //msg.Properties.add

                            isNeedToSetValue = true;

                            XElement serialedExpression = null;

                            var serialValue = string.Empty;

                            try
                            {
                                serialedExpression = serialer.Serialize((Expression)vs[i]);

                                serialValue = serialedExpression.ToString();
                            }
                            catch (Exception)
                            {

                            }

                            lcc.SetData(@"Expression_" + i.ToString(), serialValue);

                            callMsg.Args[i] = null;
                            callMsg.InArgs[i] = null;

                            vs[i] = null;// new EmptyExpression(serialValue);
                            
                        }
                    }

                    if (isNeedToSetValue)
                    {
                        var newMsg = new MessageWapper(msg);

                        for (int i = 0; i < newMsg.ArgCount; i++)
                        {
                            if (newMsg.Args[i] is Expression)
                            {
                                newMsg.Args[i] = new EmptyExpression(string.Empty);
                            }
                        }

                        for (int i = 0; i < newMsg.InArgCount; i++)
                        {
                            if (newMsg.InArgs[i] is Expression)
                            {
                                newMsg.InArgs[i] = new EmptyExpression(string.Empty);
                            }
                        }

                        //newMsg.Properties["__Args"] = vs;

                        newMsg.Properties.Remove("__Args");

                        msg = newMsg;
                    }
                    
                }
            }
            //执行下一个Sink的操作，直到执行到真实的Endpoint的方法，并得到访问值
            myReturnMsg = _nextMsgSink.SyncProcessMessage(msg);

            if (myReturnMsg is IMethodReturnMessage)//处理返回
            {
                //恢复ReturnValue中DataSet
                //System.Diagnostics.Debug.WriteLine("==========处理返回的表达式=========");
                IMethodReturnMessage returnMsg = (IMethodReturnMessage)myReturnMsg;
                var ds = returnMsg.ReturnValue as Expression;
                if (ds != null)
                {
                    try
                    {
                        //System.Diagnostics.Debug.WriteLine("处理前 ReturnValue中的DataSet的数据：" + ds.GetXml());

                        //再获得returnMsg的__CallContext
                        lcc = (LogicalCallContext)returnMsg.Properties["__CallContext"];
                        //System.Diagnostics.Debug.WriteLine("=== 有DataSetSurrogate_Return ===");
                        var rootElement=XElement.Load(new XmlTextReader(lcc.GetData("Expression_Return").ToString()));

                        //var rootElement = (XElement)lcc.GetData("Expression_Return");

                        object nds = serialer.Deserialize(rootElement);

                        //Debug.WriteLine("处理后 ReturnValue中的DataSet的数据（行的数目）：" + ((System.Data.DataSet)nds).Tables[0].Rows.Count);

                        var mrw = new MethodReturnWrapper(myReturnMsg as IMethodReturnMessage);
                        mrw.SetReturnValue(nds);
                        myReturnMsg = mrw;
                        lcc.SetData("Expression_Return", null);
                    }
                    catch (Exception ex)
                    {
                        //Debug.WriteLine(ex.ToString());
                    }
                }

                CallContext.SetData("NotSurrogate", null);
                return myReturnMsg;
            }
            else
            {
                //Debug.WriteLine(myReturnMsg.GetType().FullName);
                CallContext.SetData("NotSurrogate", null);
                return myReturnMsg;
            }



        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            return _nextMsgSink.AsyncProcessMessage(msg, replySink);
        }

        public IMessageSink NextSink
        {
            get { return _nextMsgSink; }
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotImplementedException();
        }

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            throw new NotImplementedException();
        }

        public IClientChannelSink NextChannelSink
        {
            get { return _nextClientSink; }
        }
    }
}
