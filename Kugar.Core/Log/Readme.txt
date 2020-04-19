配置该类的时候，在config文件中的
	  <configSections>
		<section name="KugarCore" type="Kugar.Core.Configuration.NameValueCollectionConfiguration, Kugar.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" allowLocation="true" allowDefinition="Everywhere" allowExeDefinition="MachineToApplication" restartOnExternalChanges="true" requirePermission="true" />
	  </configSections>

	  <KugarCore>
		<add key="LoggerFactoryFilePath" value="Kugar.Core.Log.NLogFactory.dll"/> <!--指定包含了日志记录的程序集文件路径 -->
		<add key="LoggerFactoryTypeName" value="Kugar.Core.Log.NLogFactory.NLogFactory"/> <!--指定的类型名称，如果为空，则自动搜索在指定dll文件中，第一个实现ILoggerFactory接口的类-->
	  </KugarCore>

NLog.config 的配置如下
  <targets>
    <target name="file" xsi:type="File" fileName="${basedir}/log/og_${date:format=yyyy-MM-dd}.txt" layout="${date:format=yyyy-MM-dd HH\:mm\:ss}|${level}|${message}"/>
  </targets>

  <rules> 
    <logger name="aa" writeTo="file"/>
	<logger name="KugarCore" writeTo="file"/>
  </rules>