using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Services;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using Kugar.Core.ExtMethod;
using Kugar.Core.Remoting;
using Kugar.Core.Remoting.Sinks;

namespace Kugar.Core.Remoting
{

    public enum ChannelType
    {
        ServerOnly,
        ClientOnly,
        Both
    }

    public enum FormatterSinkProviderType
    {
        Binary,
        SOAP,
        Other
    }



    public abstract class GeneralChannelInfo
    {
        /// <summary>
        ///     机器名称
        /// </summary>
        public string machineName { set; get; }

        /// <summary>
        ///     客户端信道名称
        /// </summary>
        public string ClientChannelName { protected set; get; }

        /// <summary>
        ///     服务器端信道名称
        /// </summary>
        public string ServerChannelName { protected set; get; }

        /// <summary>
        ///     要创建的信道类型,如:只创建服务器端、只创建客户端、两种都创建
        /// </summary>
        public ChannelType channelType { protected set; get; }

        //是否启用安全检查,要使用安全检查,则客户端与服务器端同时启用
        public bool IsSecure { set; get; }

        /// <summary>
        ///     客户端连接服务器端超时的时间
        /// </summary>
        public int ClientConnectToServerTimeout { set; get; }

        /// <summary>
        ///     格式化器类型,分为:
        ///                       Binary:二进制
        ///                       SOAP:SOAP格式
        ///                       Other:其他,由用户指定
        /// </summary>
        public FormatterSinkProviderType DataFormatterType { set; get; }


        public IServerFormatterSinkProvider ServerFormatterSinkProvider { set; get; }
        public IClientFormatterSinkProvider ClientFormatterSinkProvider { set; get; }

        /// <summary>
        ///     根据类中的信息构建相应的信道并返回IChannel接口数组
        /// </summary>
        /// <param name="isAutoRegister">是否自动将信道注册</param>
        /// <returns></returns>
        public IChannel[] BuildChannel(bool isAutoRegister)
        {
            List<IChannel> lst = new List<IChannel>(2);

            switch (channelType)
            {
                case ChannelType.ServerOnly:
                    {
                        var ser = getServerChannel();

                        lst.Add(ser);
                    }
                    break;
                case ChannelType.ClientOnly:
                    {
                        var client = getClientChannel();
                        lst.Add(client);
                    }
                    break;
                case ChannelType.Both:
                    {
                        lst.Add(getServerChannel());
                        lst.Add(getClientChannel());
                    }
                    break;
            }

            if (isAutoRegister)
            {
                foreach (var channel in lst)
                {
                    if (channel != null)
                    {
                        ChannelServices.RegisterChannel(channel, this.IsSecure);
                    }
                }
            }

            return lst.ToArray();
        }

        protected abstract IChannel getServerChannel();


        protected abstract IChannel getClientChannel();
    }

    /// <summary>
    ///     用于方便构建相应的TCP信道对象,并提供自动注册
    /// </summary>
    public class TCPChannelBuilder : GeneralChannelInfo
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="serverPort">服务器端口号</param>
        /// <param name="clientPort">客户端端口号,不指定端口号可设为0</param>
        /// <param name="serverName">服务器信道名称</param>
        /// <param name="clientName">客户端信道名称</param>
        /// <param name="_channelType">要创建的信道类型,如:只创建服务器端、只创建客户端、两种都创建</param>
        /// <param name="isSecure">是否创建通信安全的信道</param>
        public TCPChannelBuilder(int serverPort, int clientPort, string serverName, string clientName, ChannelType _channelType, bool isSecure)
        {
            ServerChannelName = serverName;
            ClientChannelName = clientName;
            channelType = _channelType;

            ServerPort = serverPort;
            ClientPort = clientPort;

            IsSecure = isSecure;

            DataFormatterType = FormatterSinkProviderType.Binary;

            ClientConnectToServerTimeout = 5;

            CompressMode = CompressionMode.None;

            if (isSecure)
            {
                tokenImpersonationLevel = TokenImpersonationLevel.Identification;
                protectionLevel = ProtectionLevel.EncryptAndSign;
                Impersonate = false;
            }
        }

        /// <summary>
        ///     服务器端端口
        /// </summary>
        public int ServerPort { set; get; }

        /// <summary>
        ///     客户端端口
        /// </summary>
        public int ClientPort { set; get; }

        /// <summary>
        ///  属性相关:
        /// 
        ///     指定服务器对客户端进行身份验证的方式
        /// </summary>
        public TokenImpersonationLevel tokenImpersonationLevel { set; get; }

        /// <summary>
        /// 属性相关:服务器端
        /// 
        ///     服务器是否应该模拟客户端
        /// </summary>
        public bool Impersonate { set; get; }

        /// <summary>
        /// 属性相关:客户端,服务器端
        /// 
        ///     保护等级,分别为:
        ///                     None:仅身份验证;
        ///                     Sign:确保所传输数据的完整性;
        ///                     EncryptAndSign:加密并确保数据的完整性;
        ///     注:保护等级过高可能导致效率降低.默认为EncryptAndSign
        /// </summary>
        public ProtectionLevel protectionLevel { set; get; }

        /// <summary>
        /// 属性相关:服务器端
        /// 
        ///     用于当出现IP连接时,引发的事件,可在事件中检查接入的IP是否合法
        ///     备注:允许连接时返回true,不允许连接时返回false
        /// </summary>
        public event Authorization_IPConnectCheck IPConnectCheck;

        /// <summary>
        /// 属性相关:服务器端
        /// 
        ///     用于当IP连接检查通过后,在该事件中可检查由客户端提交的数据是否合法
        ///     备注:允许连接返回true,不允许连接返回false
        /// </summary>
        public event Authorization_IdentityAuthorizingCheck ClientDataCheck;

        public event DecodeClientInfo ClientInfo_Setup;

        public event EventHandler AfterMethodCalled;

        public event ClientRequestInfoHandler BeforeClientRequest;

        /// <summary>
        ///     压缩模式
        /// </summary>
        public CompressionMode CompressMode { set; get; }

        public IServerChannelSinkProvider ServerChannelSinkProviders { get; private set; }

        public IClientChannelSinkProvider ClientChannelSinkProviders { get; private set; }


        public static TCPChannelBuilder BuildServerOnly(int serverPort, string serverName)
        {
            return BuildServerOnly(serverPort, serverName, false);
        }

        /// <summary>
        ///     创建只有服务器端信息的TCPChannelBuilder对象
        /// </summary>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="serverName">服务器端信道名称</param>
        /// <param name="isSecure">是否创建通信安全信道</param>
        /// <returns>返回一个包含信道信息的TCPChannelBuilder</returns>
        public static TCPChannelBuilder BuildServerOnly(int serverPort, string serverName, bool isSecure)
        {
            return new TCPChannelBuilder(serverPort, 0, serverName, "", ChannelType.ServerOnly, isSecure);
        }


        public static TCPChannelBuilder BuildClientOnly(string clientName)
        {
            return BuildClientOnly(0, clientName, false);
        }

        public static TCPChannelBuilder BuildClientOnly(string clientName, bool isSecure)
        {
            return BuildClientOnly(0, clientName, isSecure);
        }

        /// <summary>
        ///     创建只有客户端信息的TCPChannelBuilder对象
        /// </summary>
        /// <param name="clientPort">客户端端口,不需要指定的设置为0</param>
        /// <param name="clientName">客户端信道名称</param>
        /// <param name="isSecure">是否创建通信安全的信道</param>
        /// <returns>返回一个包含信道信息的TCPChannelBuilder</returns>
        public static TCPChannelBuilder BuildClientOnly(int clientPort, string clientName, bool isSecure)
        {
            return new TCPChannelBuilder(0, clientPort, "", clientName, ChannelType.ClientOnly, isSecure);
        }



        public static TCPChannelBuilder BuildBothChannel(int serverPort, string channelName)
        {
            return BuildBothChannel(serverPort, 0, channelName, false);
        }

        public static TCPChannelBuilder BuildBothChannel(int serverPort, string channelName, bool isSecure)
        {
            return BuildBothChannel(serverPort, 0, channelName, isSecure);
        }

        /// <summary>
        ///     创建同时带有客户端及服务器端信息的TCPChannelBuilder对象
        /// </summary>
        /// <param name="serverPort">服务器端端口号</param>
        /// <param name="clientPort">客户端端口好</param>
        /// <param name="channelName">信道名称的前缀.服务器端名称为 channelName_Server;客户端信道名称为:channelName_Client</param>
        /// <param name="isSecure">是否创建通信安全的信道</param>
        /// <returns>返回一个包含信道信息的TCPChannelBuilder</returns>
        public static TCPChannelBuilder BuildBothChannel(int serverPort, int clientPort, string channelName, bool isSecure)
        {
            return new TCPChannelBuilder(serverPort, clientPort, channelName + "_Server", channelName + "_Client", ChannelType.Both, isSecure);
        }

        protected override IChannel getServerChannel()
        {
            TCPAuthorizationModule au = null;
            //IServerFormatterSinkProvider serFormat = null;
            //ServerCompressionSinkProvider compressSink = null;

            IServerChannelSinkProvider sink = null;

            var param = new Hashtable(5);

            param["name"] = this.ServerChannelName;
            param["port"] = this.ServerPort;
            param["secure"] = this.IsSecure;
            param["socketCacheTimeout"] = 10;

            if (IsSecure)
            {
                param["impersonate"] = this.Impersonate;
                param["protectionLevel"] = this.protectionLevel;
            }

            if (!string.IsNullOrEmpty(this.machineName))
            {
                param["machineName"] = this.machineName;
            }

            if (IsSecure && (IPConnectCheck != null || ClientDataCheck != null))
            {
                au = new TCPAuthorizationModule();

                if (this.IPConnectCheck != null) au.IPCheck += this.IPConnectCheck;

                if (this.ClientDataCheck != null) au.IdentityAuthorizingCheck += this.ClientDataCheck;

            }

            //IServerChannelSinkProvider rear = null;

            switch (DataFormatterType)
            {
                case FormatterSinkProviderType.Binary:
                    if (ServerFormatterSinkProvider == null)
                    {
                        //compressSink = new ServerCompressionSinkProvider(param,null);

                        sink = new BinaryServerFormatterSinkProvider() { TypeFilterLevel = TypeFilterLevel.Full };
                        //((BinaryServerFormatterSinkProvider)sink).TypeFilterLevel = TypeFilterLevel.Full;

                        //var serialSink = new SpecificTypeSerializeServerSinkProvider(null, null);

                        ////serialSink.Next = sink.Next;

  


                        //sink.Next = serialSink;
                        //rear = serialSink;
                        //sink = serialSink;

                        //compressSink.Next = sink;
                    }
                    else
                    {
                        sink = this.ServerFormatterSinkProvider;
                    }
                    break;
                case FormatterSinkProviderType.SOAP:
                    sink = ServerFormatterSinkProvider == null ? new SoapServerFormatterSinkProvider() { TypeFilterLevel = TypeFilterLevel.Full } : this.ServerFormatterSinkProvider;
                    break;
                case FormatterSinkProviderType.Other:
                    sink = this.ServerFormatterSinkProvider;
                    break;

            }

            if (CompressMode != CompressionMode.None)
            {
                var temp = new ServerCompressionSinkProvider(param, null, CompressMode);
                temp.Next = sink;

                sink = temp;
            }


            sink.Next = new GetClientInfoServerSinkProvider(ClientInfo_Setup, AfterMethodCalled);

            var server = new TcpServerChannel(param, sink, au);

            return server;
        }

        protected override IChannel getClientChannel()
        {
            IClientChannelSinkProvider clientformat = null;

            var param = new Hashtable(5);

            param["name"] = this.ClientChannelName;
            param["port"] = this.ClientPort;
            param["secure"] = this.IsSecure;
            //param["socketCacheTimeout"] = 5;
            //param["timeout"] = this.ClientConnectToServerTimeout;
            if (IsSecure)
            {
                param["tokenImpersonationLevel"] = this.tokenImpersonationLevel;
            }

            if (!string.IsNullOrEmpty(this.machineName))
            {
                param["machineName"] = this.machineName;
            }

            switch (DataFormatterType)
            {
                case FormatterSinkProviderType.Binary:
                    clientformat = ClientFormatterSinkProvider == null ? new BinaryClientFormatterSinkProvider() : this.ClientFormatterSinkProvider;
                    break;
                case FormatterSinkProviderType.SOAP:
                    clientformat = ClientFormatterSinkProvider == null ? new SoapClientFormatterSinkProvider() : this.ClientFormatterSinkProvider;
                    break;
                case FormatterSinkProviderType.Other:
                    clientformat = this.ClientFormatterSinkProvider;
                    break;

            }

            if (CompressMode != CompressionMode.None)
            {
                if (clientformat == null)
                {
                    clientformat = new ClientCompressionSinkProvider(null, null, this.CompressMode);
                }
                else
                {
                    clientformat.Next = new ClientCompressionSinkProvider(null, null, this.CompressMode);
                }
            }

            if (BeforeClientRequest!=null)
            {
                clientformat.Next = new SetRequestInfoClientSinkProvider(BeforeClientRequest,null);
            }
            
            //var serialSink = new SpecificTypeSerializeClientSinkProvider(null, null);

            //serialSink.Next = clientformat;

            //clientformat = serialSink;

            var client = new TcpClientChannel(param, clientformat);

            return client;
        }
    }

    /// <summary>
    ///     用于方便构建相应的IPC信道对象,并提供自动注册
    /// </summary>
    public class IPCChannelBuilder : GeneralChannelInfo
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="portName">服务器端端口名称</param>
        /// <param name="serverName">服务器端信道名称</param>
        /// <param name="clientName">客户端信道名称</param>
        /// <param name="type">要创建的信道类型,如:只创建服务器端、只创建客户端、两种都创建</param>
        /// <param name="isSecure">是否为通信安全的信道</param>
        public IPCChannelBuilder(string portName, string serverName, string clientName, ChannelType type, bool isSecure)
        {
            PortName = portName;
            ServerChannelName = serverName;
            ClientChannelName = clientName;
            IsSecure = isSecure;
            machineName = "";
            DataFormatterType = FormatterSinkProviderType.Binary;
            base.channelType = type;
        }

        /// <summary>
        ///     端口名称
        /// </summary>
        public string PortName { set; get; }

        public static IPCChannelBuilder BuildServerOnly(string portName, string channelName)
        {
            return BuildServerOnly(portName, channelName, false);
        }

        /// <summary>
        ///     创建只有服务器端信息的IPCChannelBuilder对象
        /// </summary>
        /// <param name="portName">服务器端端口名称</param>
        /// <param name="channelName">信道名称</param>
        /// <param name="isSecure">是否创建通信安全信道</param>
        /// <returns>返回一个包含信道信息的IPCChannelBuilder</returns>
        public static IPCChannelBuilder BuildServerOnly(string portName, string channelName, bool isSecure)
        {
            return new IPCChannelBuilder(portName, channelName, "", ChannelType.ServerOnly, isSecure);
        }


        public static IPCChannelBuilder BuildClientOnly(string channelName)
        {
            return BuildClientOnly(channelName, false);
        }

        /// <summary>
        ///     创建只有客户端信息的IPCChannelBuilder对象
        /// </summary>
        /// <param name="channelName">信道名称</param>
        /// <param name="isSecure">是否创建通信安全信道</param>
        /// <returns>返回一个包含信道信息的IPCChannelBuilder</returns>
        public static IPCChannelBuilder BuildClientOnly(string channelName, bool isSecure)
        {
            return new IPCChannelBuilder("", "", channelName, ChannelType.ClientOnly, isSecure);
        }

        public static IPCChannelBuilder BuildBothChannel(string portName, string channelName)
        {
            return BuildBothChannel(portName, channelName, false);
        }

        /// <summary>
        ///     创建包含服务器端以及客户端信息的IPCChannelBuilder对象
        /// </summary>
        /// <param name="portName">服务器端端口名称</param>
        /// <param name="channelName">信道名称的前缀.服务器端名称为 channelName_Server;客户端信道名称为:channelName_Client</param>
        /// <param name="isSecure">是否创建通信安全信道</param>
        /// <returns>返回一个包含信道信息的IPCChannelBuilder</returns>
        public static IPCChannelBuilder BuildBothChannel(string portName, string channelName, bool isSecure)
        {
            return new IPCChannelBuilder(portName, channelName + "_Server", channelName + "_Client", ChannelType.Both, isSecure);
        }

        protected override IChannel getServerChannel()
        {
            IServerFormatterSinkProvider serFormat = null;

            switch (DataFormatterType)
            {
                case FormatterSinkProviderType.Binary:
                    if (ServerFormatterSinkProvider == null)
                    {
                        serFormat = new BinaryServerFormatterSinkProvider();
                        ((BinaryServerFormatterSinkProvider)serFormat).TypeFilterLevel = TypeFilterLevel.Full;
                    }
                    else
                    {
                        serFormat = this.ServerFormatterSinkProvider;
                    }
                    break;
                case FormatterSinkProviderType.SOAP:
                    if (ServerFormatterSinkProvider == null)
                    {
                        serFormat = new SoapServerFormatterSinkProvider();
                        ((SoapServerFormatterSinkProvider)serFormat).TypeFilterLevel = TypeFilterLevel.Full;
                    }
                    else
                    {
                        serFormat = this.ServerFormatterSinkProvider;
                    }
                    break;
                case FormatterSinkProviderType.Other:
                    serFormat = this.ServerFormatterSinkProvider;
                    break;

            }

            var param = new Hashtable(5);

            param["name"] = this.ServerChannelName;
            param["portName"] = this.PortName;
            param["secure"] = this.IsSecure;

            if (!string.IsNullOrEmpty(this.machineName))
            {
                param["machineName"] = this.machineName;
            }

            var server = new IpcServerChannel(param, serFormat);

            return server;
        }

        protected override IChannel getClientChannel()
        {
            IClientFormatterSinkProvider clientformat = null;

            switch (DataFormatterType)
            {
                case FormatterSinkProviderType.Binary:
                    if (ServerFormatterSinkProvider == null)
                    {
                        clientformat = new BinaryClientFormatterSinkProvider();

                    }
                    else
                    {
                        clientformat = this.ClientFormatterSinkProvider;
                    }
                    break;
                case FormatterSinkProviderType.SOAP:
                    if (ServerFormatterSinkProvider == null)
                    {
                        clientformat = new SoapClientFormatterSinkProvider();

                    }
                    else
                    {
                        clientformat = this.ClientFormatterSinkProvider;
                    }
                    break;
                case FormatterSinkProviderType.Other:
                    clientformat = this.ClientFormatterSinkProvider;
                    break;

            }

            var param = new Hashtable(5);

            param["name"] = this.ClientChannelName;
            param["secure"] = this.IsSecure;
            param["connectionTimeout"] = this.ClientConnectToServerTimeout;

            if (!string.IsNullOrEmpty(this.machineName))
            {
                param["machineName"] = this.machineName;
            }

            var client = new IpcClientChannel(param, clientformat);

            return client;
        }
    }


    public delegate bool Authorization_IPConnectCheck(EndPoint endPoint);

    public delegate bool Authorization_IdentityAuthorizingCheck(IIdentity identity);

    /// <summary>
    ///     TCP身份验证使用的类
    /// </summary>
    public class TCPAuthorizationModule : IAuthorizeRemotingConnection
    {
        public bool IsConnectingEndPointAuthorized(EndPoint endPoint)
        {
            //return true;

            if (IPCheck == null)
            {
                return true;
            }

            var dels = IPCheck.GetInvocationList();

            foreach (var del in dels)
            {
                var temp = (Authorization_IPConnectCheck)del;

                try
                {
                    if (!temp(endPoint))
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    continue;
                }

            }

            return true;
        }

        public bool IsConnectingIdentityAuthorized(IIdentity identity)
        {
            //return true;

            if (IdentityAuthorizingCheck == null)
            {
                return true;
            }

            var dels = IdentityAuthorizingCheck.GetInvocationList();

            foreach (var del in dels)
            {
                var temp = (Authorization_IdentityAuthorizingCheck)del;

                try
                {
                    if (!temp(identity))
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    continue;
                }

            }

            return true;
        }

        /// <summary>
        ///     身份验证的时候引发该事件
        /// </summary>
        public event Authorization_IdentityAuthorizingCheck IdentityAuthorizingCheck;

        /// <summary>
        ///     IP验证的时候，引发该事件
        /// </summary>
        public event Authorization_IPConnectCheck IPCheck;

    }

    /// <summary>
    ///     一些通用的Remoting注册以及对象获取的函数
    /// </summary>
    public static class RemotingHelper
    {
        static RemotingHelper()
        {
            EnableClientError = true;
            ApplicationName = "kugar";
        }

        #region "RegisterCAOType"

        /// <summary>
        ///     注册客户端激活类型(CAO)的远程对象
        /// </summary>
        /// <typeparam name="T">指定注册的类型</typeparam>
        /// <returns>成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册用:
        ///         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        ///     客户端调用时
        ///         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        /// </example>
        /// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        public static bool RegisterCAOType<T>() where T : MarshalByRefObject
        {
            return RegisterCAOType(typeof(T), "kugar_App");
        }

        /// <summary>
        ///     注册客户端激活类型(CAO)的远程对象
        /// </summary>
        /// <typeparam name="T">指定注册的类型</typeparam>
        /// <param name="applicationName">指定的应用程序名</param>
        /// <returns>成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册用:
        ///         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        ///     客户端调用时
        ///         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        /// </example>
        /// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        public static bool RegisterCAOType<T>(string applicationName) where T : MarshalByRefObject
        {
            return RegisterCAOType(typeof(T), applicationName);
        }

        /// <summary>
        ///     注册客户端激活类型(CAO)的远程对象,通用于客户端使用接口或直接类型
        /// </summary>
        /// <param name="type">指定注册的类型,必须是MarshalByRefObject的子类</param>
        /// <returns>成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册用:
        ///         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        ///     客户端调用时
        ///         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        /// </example>
        /// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        public static bool RegisterCAOType(Type type)
        {
            return RegisterCAOType(type, "kugar_App");
        }

        /// <summary>
        ///     注册客户端激活类型(CAO)的远程对象,通用于客户端使用接口或直接类型
        /// </summary>
        /// <param name="type">指定注册的类型,必须是MarshalByRefObject的子类</param>
        /// <param name="applicationName">指定的应用程序名</param>
        /// <returns>成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册用:
        ///         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        ///     客户端调用时
        ///         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        /// </example>
        /// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        public static bool RegisterCAOType(Type type, string applicationName)
        {
            if (type == null || !type.IsMarshalByRef)
            {
                throw new TypeLoadException("类型必须是继承自MarshalByRefObject的类");
            }

            RegisterTrackingHandler();

            try
            {
                //var gType=typeof (SAOCAOClassFactory<>).MakeGenericType(typeof (RemotingCAOInterface<>).MakeGenericType(type));

                var gType = typeof(RemotingCAOInterfaceOrInstance<>).MakeGenericType(type);

                //RemotingConfiguration.RegisterActivatedServiceType()
                RemotingConfiguration.RegisterWellKnownServiceType(gType, applicationName, WellKnownObjectMode.SingleCall);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #endregion

        #region "RegisterMarshalType"

        /// <summary>
        ///     用Marshal(按引用编组)函数注册singleston类型的远程对象
        /// </summary>
        /// <param name="obj">要注册的对象实例</param>
        /// <returns>注册成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册使用:
        ///         var isSuccess=RegisterMarshalType(new Class1(),"kugar",true,typeof(IClass1));
        ///     
        ///     客户端调用时:
        ///         var temp= GetMarshalTypeObj《IClass1》("tcp://localhsot/kugar");  //示例使用tcp信道
        /// </example>
        /// <remarks>
        ///     如果使用该函数注册远程对象时,客户端必须使用GetMarshalTypeObj函数配对获取
        /// </remarks>
        public static bool RegisterMarshalType(MarshalByRefObject obj)
        {
            return RegisterMarshalType(obj, obj.GetType().Name);
        }

        /// <summary>
        ///     用Marshal(按引用编组)函数注册singleston类型的远程对象
        /// </summary>
        /// <param name="obj">要注册的对象实例</param>
        /// <param name="objUrl">注册的对象uri</param>
        /// <returns>注册成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册使用:
        ///         var isSuccess=RegisterMarshalType(new Class1(),"kugar",true,typeof(IClass1));
        ///     
        ///     客户端调用时:
        ///         var temp= GetMarshalTypeObj《IClass1》("tcp://localhsot/kugar");  //示例使用tcp信道
        /// </example>
        /// <remarks>
        ///     如果使用该函数注册远程对象时,客户端必须使用GetMarshalTypeObj函数配对获取
        /// </remarks>
        public static bool RegisterMarshalType(MarshalByRefObject obj, string objUrl)
        {
            return RegisterMarshalType(obj, objUrl, true, null);
        }

        /// <summary>
        ///     用Marshal(按引用编组)函数注册singleston类型的远程对象
        /// </summary>
        /// <param name="obj">要注册的对象实例</param>
        /// <param name="objUrl">注册的对象uri</param>
        /// <param name="isShowErrorToCustom">是否在客户端显示完整错误信息</param>
        /// <param name="RequestedType">将obj对象所属的类型转换为指定类型封装,如果在客户端想使用interface的话,就必须在此处设置;如果不打算转换类型,则可以为null</param>
        /// <returns>注册成功返回true,失败返回false</returns>
        /// <example>
        ///     服务器端注册使用:
        ///         var isSuccess=RegisterMarshalType(new Class1(),"kugar",true,typeof(IClass1));
        ///     
        ///     客户端调用时:
        ///         var temp= GetMarshalTypeObj<IClass1>("tcp://localhsot/kugar");  //示例使用tcp信道
        /// </example>
        /// <remarks>
        ///     如果使用该函数注册远程对象时,客户端必须使用GetMarshalTypeObj函数配对获取
        /// </remarks>
        public static bool RegisterMarshalType(MarshalByRefObject obj, string objUrl, bool isShowErrorToCustom, Type RequestedType)
        {
            if (obj == null || string.IsNullOrEmpty(objUrl))
            {
                return false;

            }


            try
            {
                if (isShowErrorToCustom)
                {
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

                    RemotingConfiguration.CustomErrorsEnabled(false);
                }

                RemotingServices.Marshal(obj, "kugar", RequestedType);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     断开使用RegisterMarshalType注册的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DisconnectMarshalTypeObj(MarshalByRefObject obj)
        {
            try
            {
                return RemotingServices.Disconnect(obj);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region "RegisterSingletonCAO"

        /// <summary>
        ///     注册单实例模式值，客户端需使用GetSingletonCAOTypeObj函数获取，GetSingletonCAOTypeObj返回的值可以是类型或者接口<br/>
        ///     如需更新类型所关联的值，只需要调用UpdateSingletonCAO函数即可
        ///     备注：由于服务器端会保存注册向客户端公开的singletonValue参数的值的引用，所以，在当该值需要释放的时候，必须调用UnRegisterSingletonCAO
        /// </summary>
        /// <typeparam name="T">将用用于注册的类型，该类型取决了客户端请求值时传递的类型值，如果客户端要使用接口，则T必须是接口</typeparam>
        /// <param name="singletonValue">注册向客户端公开的值</param>
        /// <param name="applicationName">指定的应用程序名</param>
        /// <returns></returns>
        /// <example>
        ///     public class Class1:IClass1<br/>
        ///     {}<br/>
        /// 
        ///     var value=new Class1();<br/>
        /// 
        ///     1.注册客户端使用接口:<br/>
        ///         &lt;  &gt;
        ///     服务器端：<br/>
        ///         RegisterSingletonCAO &lt;IClass1&gt;(value,applicationName)<br/>
        ///     <br/>
        ///     客户端：<br/>
        ///         var value=GetCAOTypeObj&lt;IClass1&gt;(url)<br/>
        ///    <br/>
        ///     2.注册客户端使用类型名：<br/>
        ///     服务器端：<br/>
        ///         RegisterSingletonCAO &lt;Class1&gt;(value,applicationName)<br/>
        ///     <br/>
        ///     客户端：<br/>
        ///         var value=GetCAOTypeObj&lt;Class1&gt;(url)<br/>
        ///         
        /// </example>
        public static bool RegisterSingletonCAO<T>(MarshalByRefObject singletonValue, string applicationName)
        {
            return RegisterSingletonCAO(typeof (T), singletonValue, applicationName);
        }

        /// <summary>
        ///     注册单实例模式值，客户端需使用GetSingletonCAOTypeObj函数获取，GetSingletonCAOTypeObj返回的值可以是类型或者接口<br/>
        ///     备注：由于服务器端会保存注册向客户端公开的singletonValue参数的值的引用，所以，在当该值需要释放的时候，必须调用UnRegisterSingletonCAO
        /// </summary>
        /// <param name="type">将用用于注册的类型，该类型取决了客户端请求值时传递的类型值，如果客户端要使用接口，则该参数必须是typeof(接口)</param>
        /// <param name="singletonValue">注册向客户端公开的值</param>
        /// <param name="applicationName">指定的应用程序名</param>
        /// <returns></returns>
        public static bool RegisterSingletonCAO(Type type, MarshalByRefObject singletonValue, string applicationName)
        {
            if (singletonValue == null)// || !singletonValue.GetType().IsAssignableFrom(type))
            {
                throw new TypeLoadException("类型必须是继承自MarshalByRefObject的类");
            }

            RegisterTrackingHandler();

            try
            {
                //var gType=typeof (SAOCAOClassFactory<>).MakeGenericType(typeof (RemotingCAOInterface<>).MakeGenericType(type));

                RemotingCAOSingletonHelper.AddOrUpdate(type,singletonValue);

                //RemotingConfiguration.RegisterActivatedServiceType()
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemotingCAOSingleton), applicationName, WellKnownObjectMode.SingleCall);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     更新指定类型的值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="singletonValue"></param>
        /// <returns></returns>
        public static bool UpdateSingletonCAO(Type type, MarshalByRefObject singletonValue)
        {
            return RemotingCAOSingletonHelper.UpdateValue(type,singletonValue);
        }

        /// <summary>
        ///     释放由RegisterSingletonCAO函数注册的Singleton模式的值
        /// </summary>
        /// <param name="type"></param>
        public static void UnRegisterSingletonCAO<T>()
        {
            RemotingCAOSingletonHelper.Remove(typeof(T));
        }

        /// <summary>
        ///     释放由RegisterSingletonCAO函数注册的Singleton模式的值
        /// </summary>
        /// <param name="type">该参数指定的Type类型，必须与RegisterSingletonCAO函数注册时所使用的类型一致</param>
        public static void UnRegisterSingletonCAO(Type type)
        {
            RemotingCAOSingletonHelper.Remove(type);
        }

        #endregion

        #region "GetCAOTypeObj"

        /// <summary>
        ///     获取CAO注册类型的对象实例,通用于 使用RegisterCAOType函数注册的对象
        ///     注意:T类型如果需要是 interface类型的,则服务器端必须用RegisterCAOInterface 注册
        /// </summary>
        /// <typeparam name="T">要获取的对象类型可以是interface或者MarsharlRefObj类型</typeparam>
        /// <param name="url">远程的url</param>
        /// <returns>返回实例化后的远程对象</returns>
        /// <remarks>注意:T类型如果需要是 interface类型的,则服务器端必须用RegisterCAOInterface 注册</remarks>
        public static T GetCAOTypeObj<T>(string url)
        {

            CallContext.FreeNamedDataSlot("ObservedIP");

            var type = typeof(T);

            if (type.IsMarshalByRef || type.IsInterface)
            {
                try
                {
                    var factoryClass = (ICAOInterfaceFactory)Activator.GetObject(type, url);

                    var uriInfo = url.ToUri();

                    var ipaddress = uriInfo.GetHostIPAddress();

                    var ret = factoryClass.CreateInstance(ipaddress.ToString());

                    return (T)ret;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            else
            {
                throw new TypeLoadException("要获取的对象类型必须是interface或者MarsharlRefObj类型");
            }

            //if (type.IsMarshalByRef)
            //{
            //    return (T)getCAOTypeInstance(url, typeof(T));

            //}
            //else if (type.IsInterface)
            //{
            //    return (T)getCAOTypeInterface(url, typeof(T));
            //}
            //else
            //{
            //    throw new TypeLoadException("要获取的对象类型必须是interface或者MarsharlRefObj类型");
            //}
        }

        #endregion

        #region "GetMarshalTypeObj"

        /// <summary>
        ///     获取Marshal注册类型的对象
        /// </summary>
        /// <typeparam name="T">要获取的对象类型</typeparam>
        /// <param name="url">远程的url</param>
        /// <returns></returns>
        /// <example>var temp= GetMarshalTypeObj<IClass1>("tcp://localhsot/kugar")  </example>
        /// <remarks>注意:T可以是interface,但如果是interface则必须在注册的对象时,设置RequestedType参数为T</remarks>
        public static T GetMarshalTypeObj<T>(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return default(T);
            }

            object[] attrs1 = { new UrlAttribute(url) };
            try
            {
                var handle = Activator.GetObject(typeof(T), url);

                var temp = (T)handle;

                return temp;

            }
            catch (Exception)
            {
                return default(T);
            }
        }

        #endregion

        #region "GetSingletonCAOTypeObj"

        /// <summary>
        ///     获取CAO注册类型的对象实例,通用于 使用RegisterSingletonCAO
        ///     注意:T类型如果需要是 interface类型的,则服务器端必须用RegisterCAOInterface 注册
        /// </summary>
        /// <typeparam name="T">要获取的对象类型可以是interface或者MarsharlRefObj类型</typeparam>
        /// <param name="url">远程的url</param>
        /// <returns>返回实例化后的远程对象</returns>
        /// <remarks>注意:T类型如果需要是 interface类型的,则服务器端必须用RegisterCAOInterface 注册</remarks>
        public static T GetSingletonCAOTypeObj<T>(string url)
        {

            CallContext.FreeNamedDataSlot("ObservedIP");

            var type = typeof(T);

            if (type.IsMarshalByRef || type.IsInterface)
            {
                try
                {
                    var factoryClass = (ICAOSingletonFactory)Activator.GetObject(type, url);

                    var uriInfo = url.ToUri();

                    var ipaddress = uriInfo.GetHostIPAddress();

                    var ret = factoryClass.CreateInstance(typeof(T), ipaddress.ToString());

                    return (T)ret;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            else
            {
                throw new TypeLoadException("要获取的对象类型必须是interface或者MarsharlRefObj类型");
            }
        }

        #endregion

        //用于方便构建连接地址的函数
        #region "BuildConnectUri"

        /// <summary>
        ///     用于构建客户端使用IPC信道连接远程对象时说需要的连接地址
        /// </summary>
        /// <param name="portName">IPC端口名称</param>
        /// <param name="appNameOrObjUri">远程对象的注册应用程序名称或对象uri</param>
        /// <returns>返回一个连接地址字符串</returns>
        public static string BuildConnectUri_IPC(string portName, string appNameOrObjUri)
        {
            var ret = string.Format("ipc://{0}/{1}", portName, appNameOrObjUri);

            return ret;
        }

        /// <summary>
        ///     用于构建客户端使用TCP信道连接远程对象时说需要的连接地址
        /// </summary>
        /// <param name="ipaddress">远程计算机IP地址,如:localhost或者127.0.0.1等</param>
        /// <param name="port">远程计算机开放的端口</param>
        /// <param name="appNameOrObjUri">远程对象的注册应用程序名称或对象uri</param>
        /// <returns>返回一个连接地址字符串</returns>
        public static string BuildConnectUri_TCP(string ipaddress, int port, string appNameOrObjUri)
        {
            var ret = string.Format("tcp://{0}:{1}/{2}", ipaddress, port, appNameOrObjUri);

            return ret;
        }

        /// <summary>
        ///     用于构建客户端使用TCP信道连接远程对象时说需要的连接地址
        /// </summary>
        /// <param name="remoteIPEnd">远程端点</param>
        /// <param name="appNameOrObjUri">远程对象的注册应用程序名称或对象uri</param>
        /// <returns>返回一个连接地址字符串</returns>
        public static string BuildConnectUri_TCP(IPEndPoint remoteIPEnd, string appNameOrObjUri)
        {
            return BuildConnectUri_TCP(remoteIPEnd.Address.ToString(), remoteIPEnd.Port, appNameOrObjUri);
        }

        #endregion

        private static bool _enableClientError = true;
        public static bool EnableClientError
        {
            get { return _enableClientError; }
            set
            {
                if (value)
                {
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

                    RemotingConfiguration.CustomErrorsEnabled(false);
                }
                else
                {
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.RemoteOnly;

                    RemotingConfiguration.CustomErrorsEnabled(true);                    
                }

                _enableClientError = value;

            }
        }

        public static string ApplicationName
        {
            get { return RemotingConfiguration.ApplicationName; }
            set { RemotingConfiguration.ApplicationName = value; }
        }

        private static void RegisterTrackingHandler()
        {
            var trackList = TrackingServices.RegisteredHandlers;

            if (trackList != null && trackList.Length > 0)
            {
                foreach (var handler in trackList)
                {
                    if (handler is WanTrackingHandler)
                    {
                        return;
                    }
                }

            }


            var t = new WanTrackingHandler();



            //t.MarshaledObject(new object(), new ObjRef(null,null));

            TrackingServices.RegisterTrackingHandler(t);
        }

        ///// <summary>
        /////     获取CAO注册类型的对象实例,并返回指定的接口
        ///// </summary>
        ///// <param name="url">远程的url</param>
        ///// <param name="type">interface</param>
        ///// <returns>返回实例化后的远程对象</returns>
        ///// <remarks> 注意:T类型只能是interface </remarks>
        //private static object getCAOTypeInterface(string url, Type type)
        //{
        //    if (type == null || !type.IsInterface)
        //    {
        //        throw new TypeLoadException("指定类型必须是接口");
        //    }

        //    try
        //    {
        //        //(ICAOInterfaceFactory)Activator.GetObject(type, url);


        //        //var newType = typeof(SAOCAOClassFactoryDef<>).MakeGenericType(typeof(RemotingCAOInterface<>));

        //        //var factoryClass = Activator.GetObject(newType, url);

        //        //var t=factoryClass.FastInvoke("CreateCAOClass", url);

        //        //var ret = t.FastInvoke("CreateInstance");

        //        //return ret;

        //        var factoryClass = (ICAOInterfaceFactory)Activator.GetObject(type, url);

        //        var uriInfo = url.ToUri();

        //        var ipaddress = uriInfo.GetHostIPAddress();

        //        //var temp = new CheckData();

        //        //temp.IpAddress = ipaddress.ToString();

        //        //CallContext.SetData("ObservedIP", temp);

        //        //CallContext.SetData("ObservedIPStr", ipaddress.ToString());

        //        var ret = factoryClass.CreateInstance(ipaddress.ToString());



        //        return ret;
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }
        //}

        ///// <summary>
        /////     获取CAO注册类型的对象实例,并返回指定的接口,类型只能是MarshalByRefObject
        ///// </summary>
        ///// <param name="url">远程的url</param>
        ///// <param name="targetType">MarshalByRefObject</param>
        ///// <returns>返回实例化后的远程对象</returns
        //private static object getCAOTypeInstance(string url, Type targetType)
        //{
        //    if (string.IsNullOrEmpty(url))
        //    {
        //        return targetType.GetDefaultValue();
        //    }

        //    if (targetType == null || !targetType.IsMarshalByRef)
        //    {
        //        throw new TypeLoadException("指定类型必须是继承自MarsharlRefObj类");
        //    }

        //    object[] attrs = { new UrlAttribute(url) };
        //    try
        //    {
        //        var assemlbyName = targetType.Assembly.GetName().Name;
        //        var typeName = targetType.FullName;

        //        var handle = Activator.CreateInstance(assemlbyName, typeName, attrs);

        //        var temp = (ICAOInterfaceFactory)handle.Unwrap();

        //        var uriInfo = url.ToUri();

        //        var ipaddress = uriInfo.GetHostIPAddress();

        //        return temp.CreateInstance(ipaddress.ToString());

        //        //return temp;

        //    }
        //    catch (Exception)
        //    {
        //        return targetType.GetDefaultValue();
        //    }
        //}

        ///// <summary>
        /////     注册客户端激活类型(CAO)的远程对象
        ///// </summary>
        ///// <typeparam name="T">指定注册的类型</typeparam>
        ///// <param name="applicationName">指定的应用程序名</param>
        ///// <param name="isShowErrorToCustom">是否在客户端显示完整错误</param>
        ///// <returns>成功返回true,失败返回false</returns>
        ///// <example>
        /////     服务器端注册用:
        /////         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        /////     客户端调用时
        /////         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        ///// </example>
        ///// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        //public static bool RegisterCAOType<T>(string applicationName, bool isShowErrorToCustom) where T : MarshalByRefObject
        //{
        //    return RegisterCAOType(typeof(T), applicationName, isShowErrorToCustom);

        //    //try
        //    //{
        //    //    RemotingConfiguration.ApplicationName = applicationName;

        //    //    if (isShowErrorToCustom)
        //    //    {
        //    //        RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

        //    //        RemotingConfiguration.CustomErrorsEnabled(false);
        //    //    }

        //    //    RemotingConfiguration.RegisterActivatedServiceType(typeof(T));
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    return false;
        //    //}

        //    //return true;

        //}
        ///// <summary>
        /////     注册客户端激活类型(CAO)的远程对象
        ///// </summary>
        ///// <param name="type">指定注册的类型,必须是MarshalByRefObject的子类</param>
        ///// <param name="applicationName">指定的应用程序名</param>
        ///// <param name="isShowErrorToCustom">是否在客户端显示完整错误</param>
        ///// <returns>成功返回true,失败返回false</returns>
        ///// <example>
        /////     服务器端注册用:
        /////         var isSuccess=RegisterCAOType《class1》("kugarApplcation",true);
        /////     客户端调用时
        /////         var obj=GetCAOTypeObj《Class1》("tcp://localhost:19860/kugarApplcation");
        ///// </example>
        ///// <remarks>如果使用该函数注册远程对象时,客户端必须使用GetCAOTypeObj函数配对获取</remarks>
        //public static bool RegisterCAOType(Type type, string applicationName, bool isShowErrorToCustom)
        //{
        //    if (type == null || !type.IsMarshalByRef)
        //    {
        //        throw new TypeLoadException("类型必须是继承自MarshalByRefObject的类");
        //    }

        //    RegisterTrackingHandler();

        //    try
        //    {
        //        RemotingConfiguration.ApplicationName = applicationName;

        //        if (isShowErrorToCustom)
        //        {
        //            RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

        //            RemotingConfiguration.CustomErrorsEnabled(false);
        //        }

        //        var newType = typeof(RemotingCAOInstance<>).MakeGenericType(type);

        //        RemotingConfiguration.RegisterActivatedServiceType(newType);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;

        //}

        //#region "RegisterCAOInterface"

        ///// <summary>
        /////     注册一个给客户端用的CAO类型接口
        ///// </summary>
        ///// <typeparam name="T">指定注册的类型,必须是MarshalByRefObject的子类</typeparam>
        ///// <returns></returns>
        //public static bool RegisterCAOInterface<T>() where T : MarshalByRefObject, new()
        //{
        //    return RegisterCAOInterface<T>("Kugar_ServerApp");
        //}

        ///// <summary>
        /////     注册一个给客户端用的CAO类型接口
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="applicationName"></param>
        ///// <returns></returns>
        //public static bool RegisterCAOInterface<T>(string applicationName) where T : MarshalByRefObject, new()
        //{
        //    return RegisterCAOInterface(typeof(T), applicationName);

        //    //    try
        //    //    {
        //    //        RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemotingCAOInterface<T>), applicationName, WellKnownObjectMode.SingleCall);
        //    //        return true;
        //    //    }
        //    //    catch (Exception)
        //    //    {
        //    //        return false;
        //    //    }

        //}

        ///// <summary>
        /////     注册一个给客户端用的CAO类型接口
        ///// </summary>
        ///// <returns></returns>
        //public static bool RegisterCAOInterface(Type type)
        //{
        //    return RegisterCAOInterface(type, "Kugar_ServerApp");
        //}

        ///// <summary>
        /////     注册一个给客户端用的CAO类型接口
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="applicationName"></param>
        ///// <returns></returns>
        //public static bool RegisterCAOInterface(Type type, string applicationName)
        //{
        //    if (type == null || !type.IsMarshalByRef)
        //    {
        //        throw new TypeLoadException("类型必须是继承自MarshalByRefObject的类");
        //    }

        //    RegisterTrackingHandler();

        //    try
        //    {
        //        //var gType=typeof (SAOCAOClassFactory<>).MakeGenericType(typeof (RemotingCAOInterface<>).MakeGenericType(type));

        //        var gType = typeof(RemotingCAOInterface<>).MakeGenericType(type);

        //        //RemotingConfiguration.RegisterActivatedServiceType()
        //        RemotingConfiguration.RegisterWellKnownServiceType(gType, applicationName, WellKnownObjectMode.SingleCall);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //}

        //#endregion

    }



    /// <summary>
    ///     用于在客户端激活的方式中,用于处理当注册的类是单实例模式时,需要重新写一个包装类
    /// </summary>
    public class TempClassForSingleInstance<T>
    {
        private MethodInfo createmethod = null;

        public TempClassForSingleInstance()
        {
            var type = typeof(T);

            var m = type.GetMethod("Create");
            if (m == null || !m.IsStatic || m.GetParameters().Length > 0)
            {
                m = type.GetMethod("GetInstance");
                if (m == null || !m.IsStatic || m.GetParameters().Length > 0)
                {
                    throw new TargetException("指定的类必须是拥有 Create或GetInstance 无参数的静态函数的类");
                }
                createmethod = m;
            }
            else
            {
                createmethod = m;
            }
        }

        public virtual T Create()
        {
            return (T)createmethod.Invoke(null, null);
        }

        public virtual T GetInstance()
        {
            return Create();
        }
    }

    [Serializable]
    class CheckData : ILogicalThreadAffinative
    {
        public string IpAddress;
    }


}

