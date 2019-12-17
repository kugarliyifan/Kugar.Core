RemotingHelper 使用方法：
例：
Class1：MarshalByRefObject，IClass1

1.使用CAO类型：

	服务器端使用：
			var channel = new TCPChannelBuilder(19860, 0, "kugar", "", ChannelType.ServerOnly, false); //注册非安全型的tcp信道
            channel.CompressMode = CompressionMode.Compress; //强制使用压缩
            channel.BuildChannel(true); //构建并注册信道
            RemotingHelper.RegisterCAOType<RemotingTestClass.Class1>("kugar");	//使用kugar为名称，注册Class1为CAO类型的远程类型
	客户端使用：
			var channel = new TCPChannelBuilder(0, 0, "", "kugarclient", ChannelType.ClientOnly,false);
            channel.CompressMode = CompressionMode.Compress;	//该项需与服务器端注册的压缩方式相同
            channel.BuildChannel(true);

			//创建远程对象
			var url = RemotingHelper.BuildConnectUri_TCP("127.0.0.1", 19860, "kugar"); //指定远程Remoting服务的IP，端口，以及要调用的程序名称

            var remotingObj = RemotingHelper.GetCAOTypeObj<RemotingTestClass.Class1>(url);  //新建一个远程代理对象
			remotingObj.sss();  //调用远程对象的一个方法

2.使用CAO类型，并在客户端返回接口
		服务器端使用：
			var channel = new TCPChannelBuilder(19860, 0, "kugar", "", ChannelType.ServerOnly, false); //注册非安全型的tcp信道
            channel.CompressMode = CompressionMode.Compress; //强制使用压缩
            channel.BuildChannel(true); //构建并注册信道
            RemotingHelper.RegisterCAOInterface<IClass1>("kugar");	//使用kugar为名称，注册IClass1为CAO类型的远程类型

		客户端使用：
			var url = RemotingHelper.BuildConnectUri_TCP("127.0.0.1", 19860, "kugar"); //指定远程Remoting服务的IP，端口，以及要调用的程序名称

            var remotingObj = RemotingHelper.GetCAOTypeObj<IClass1>(url);  //新建一个远程代理对象，调用的方式与普通CAO对象相同
			remotingObj.sss();  //调用远程对象的一个方法