using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.Network.NamedPipe
{
    /// <summary>
    /// 进程间命名管道通信类,自动封装了Server和Client,无需对此进行处理,只需要发送时,指定接收方名称即可
    /// </summary>
    public class ProcessNamedPipe : MarshalByRefObject
    {
        private ConcurrentDictionary<string, PipeConnectionBlock> _pipeConnections = new ConcurrentDictionary<string, PipeConnectionBlock>();
        //private Dictionary<string, PipeConnectionBlock> _pipeClients = new Dictionary<string, PipeConnectionBlock>();
        //private NamedPipeServerStream _pipeServer = null;
        //private NamedPipeClientStream _pipeClient = null;
        //private string _currentProcessName = "";
        private string _pipName = "";

        //用于检查链接是否正常,已断开的链接自动移除
        private TimerEx _checkConnect = null;

        private Mutex _mutex = null;

        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="currentNodeName">当前节点名称</param>
        public ProcessNamedPipe(string currentNodeName)
        {
            _mutex = new Mutex(true, currentNodeName, out var isSuccess);

            if (!isSuccess)
            {
                throw new ArgumentOutOfRangeException("currentNodeName", "指定名称的管道已被注册,无法重复注册");
            }

            _pipName = currentNodeName;

            beginwaitForConnection();

            _checkConnect = new TimerEx(checkServer, 5000, 0);

            _checkConnect.Start();
        }

        /// <summary>
        /// 向指定接收方发送数据
        /// </summary>
        /// <param name="targetName">接收方名称,即接收方构造函数中传入的名称</param>
        /// <param name="data">将要发送的数据</param>
        public void SendTo(string targetName, string data)
        {
            var writer = getWriter(targetName);

            if (writer != null)
            {
                var json = new JObject()
                {
                    ["Data"] = data,
                    ["ClientName"] = _pipName
                };

                writer.WriteLine(json.ToStringEx(Formatting.None));

                writer.Flush();
            }
        }

        /// <summary>
        /// 向指定接收方发送数据
        /// </summary>
        /// <param name="targetName">接收方名称,即接收方构造函数中传入的名称</param>
        /// <param name="data">将要发送的数据</param>
        public async void SendToAsync(string targetName, string data)
        {
            var writer = getWriter(targetName);

            if (writer != null)
            {
                var json = new JObject()
                {
                    ["Data"] = data,
                    ["ClientName"] = _pipName
                };

                await writer.WriteLineAsync(json.ToStringEx(Formatting.None));

                await writer.FlushAsync();
            }
            else
            {
                throw new Exception("链接不存在");
            }
        }

        /// <summary>
        /// 关闭所有链接
        /// </summary>
        public void Close()
        {
            foreach (var pipe in _pipeConnections)
            {
                pipe.Value.Close();
            }

            _pipeConnections.Clear();

            _mutex.Close();

            _mutex.Dispose();
        }

        /// <summary>
        /// 接收到数据时触发该事件
        /// </summary>
        public event EventHandler<PipeDataReceivedEventArgs> DataReceived;

        private void beginwaitForConnection()
        {
            var pipeServer = new NamedPipeServerStream(_pipName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            pipeServer.BeginWaitForConnection(endWaitForConnection, pipeServer);

        }

        private async void endWaitForConnection(IAsyncResult ar)
        {
            var pipeServer = (NamedPipeServerStream)ar.AsyncState;

            pipeServer.EndWaitForConnection(ar);

            pipeServer.ReadMode = PipeTransmissionMode.Message;

            var block = new PipeConnectionBlock()
            {
                Stream = pipeServer,
                Reader = new StreamReader(pipeServer, Encoding.UTF8),
                Writer = new StreamWriter(pipeServer, Encoding.UTF8)
                //ClientName = clientName
            };

            //using (var sr = new StreamReader(pipeServer, Encoding.UTF8))
            {
                var clientInfoStr = await block.Reader.ReadLineAsync();

                var json = JsonConvert.DeserializeObject<JObject>(clientInfoStr);

                var clientName = json.GetString("ClientName");

                block.TargetName = clientName;

                if (!_pipeConnections.TryAdd(clientName, block))
                {
                    block.Close();
                }

                var thread = new Thread(() =>
                  {
                      readData(block.Reader, block.Stream);
                  });

                block.Thread = thread;

                thread.Start();
            }

            beginwaitForConnection();
        }

        private void readData(StreamReader reader, PipeStream pip)
        {
            do
            {
                var jsonStr = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(jsonStr))
                {
                    continue;
                }

                var json = JObject.Parse(jsonStr);

                var data = json.GetString("Data");
                var clientName = json.GetString("ClientName");

                Task.Run(() =>
                {
                    DataReceived(this, new PipeDataReceivedEventArgs(data, clientName));
                });
            } while (pip.IsConnected);
        }

        private PipeConnectionBlock connectTo(string serverPipeName)
        {


            var client = new NamedPipeClientStream(".", serverPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            client.Connect(3000);

            if (client.IsConnected)
            {
                client.ReadMode = PipeTransmissionMode.Message;

                var data = new JObject()
                {
                    ["ClientName"] = _pipName
                }.ToStringEx(Formatting.None);

                var block = new PipeConnectionBlock()
                {
                    Stream = client,
                    TargetName = serverPipeName,
                    Reader = new StreamReader(client, Encoding.UTF8),
                    Writer = new StreamWriter(client, Encoding.UTF8)
                };

                block.Writer.WriteLine(data); ;

                var thread = new Thread(() =>
                {
                    readData(block.Reader, block.Stream);
                });

                block.Thread = thread;

                thread.Start();

                if (!_pipeConnections.TryAdd(serverPipeName, block))
                {
                    block.Close();
                }

                return block;
            }
            else
            {
                return null;
            }
        }

        private StreamWriter getWriter(string targetName)
        {
            StreamWriter stream = null;

            _locker.EnterUpgradeableReadLock();

            try
            {
                //获取并检查链接是否正常
                if (_pipeConnections.TryGetValue(targetName, out var tmp1) && tmp1.Stream.IsConnected)
                {
                    stream = tmp1?.Writer;
                }
                else
                {
                    _locker.EnterWriteLock();

                    if (_pipeConnections.TryGetValue(targetName, out var tmp2))
                    {
                        tmp2.Close();

                        _pipeConnections.TryRemove(targetName);
                    }

                    try
                    {
                        var block = connectTo(targetName);

                        stream = block.Writer;
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }


            return stream;
        }

        private List<string> _removeLst = new List<string>();

        private void checkServer(object state)
        {
            _locker.EnterReadLock();

            try
            {
                foreach (var block in _pipeConnections)
                {
                    if (!block.Value.Stream.IsConnected)
                    {
                        _removeLst.Add(block.Key);
                    }
                }

                if (_removeLst.HasData())
                {
                    foreach (var item in _removeLst)
                    {
                        _pipeConnections.TryRemove(item);
                    }

                    _removeLst.Clear();
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                _locker.ExitReadLock();
            }


        }

        private class PipeConnectionBlock
        {
            public PipeStream Stream { set; get; }

            public string TargetName { set; get; }

            public StreamReader Reader { set; get; }

            public StreamWriter Writer { set; get; }

            public Thread Thread { set; get; }

            public void Close()
            {
                try
                {
                    Thread.Abort();
                }
                catch (Exception e)
                {
                }

                Reader.Close();
                Reader.Dispose();

                Writer.Close();
                Writer.Dispose();

                Stream.Close();
                Stream.Dispose();

            }
        }


        public class PipeDataReceivedEventArgs : EventArgs
        {
            public PipeDataReceivedEventArgs(string data, string clientName)
            {
                Data = data;
                ClientName = clientName;
            }

            public string Data { set; get; }

            public string ClientName { set; get; }
        }
    }

    public class ProcessNamedPipeServer
    {
        private static Mutex _mutex = null;
        private string _pipName = "";

        //static ProcessNamedPipeServer()
        //{
        //    AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        //}

        //private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public ProcessNamedPipeServer(string currentPipeName)
        {
            _pipName = currentPipeName;

            //_mutex = new Mutex(true, currentPipeName, out var isSuccess);

            //if (!isSuccess)
            //{
            //    throw new ArgumentOutOfRangeException("currentNodeName", "指定名称的管道已被注册,无法重复注册");
            //}

            beginwaitForConnection();
            
            
        }

        public  event ProcessNamedPipeServer.RequestCall OnRequestCall;

        public delegate ResultReturn RequestCall(string serviceName, JObject args);
        
        private void beginwaitForConnection()
        {
            var pipeServer = new NamedPipeServerStream(_pipName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            pipeServer.BeginWaitForConnection(endWaitForConnection, pipeServer);

        }

        private async void endWaitForConnection(IAsyncResult ar)
        {
            var pipeServer = (NamedPipeServerStream) ar.AsyncState;

            pipeServer.EndWaitForConnection(ar);

            pipeServer.ReadMode = PipeTransmissionMode.Message;

            beginwaitForConnection();


            try
            {
                using (pipeServer)
                using (var reader = new StreamReader(pipeServer, Encoding.UTF8))
                {
                    var requestStr = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(requestStr))
                    {
                        return;
                    }

                    var requestJson = JObject.Parse(requestStr);

                    ResultReturn ret = null;

                    if (OnRequestCall != null)
                    {
                        try
                        {
                            ret = OnRequestCall(requestJson.GetString("ServiceName"), requestJson.GetJObject("Args"));
                        }
                        catch (Exception e)
                        {
                            ret = new FailResultReturn(e);
                        }
                    }
                    else
                    {
                        ret = new FailResultReturn("不存在指定服务");
                    }

                    var sw = new StreamWriter(pipeServer, Encoding.UTF8);

                    try
                    {
                        await sw.WriteLineAsync(JsonConvert.SerializeObject(ret,Formatting.None));

                        await sw.FlushAsync();

                        var replyStr = await reader.ReadLineAsync();

                        if (replyStr == "ok")
                        {

                        }

                        sw.Close();
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                    finally
                    {
                        if (pipeServer.IsConnected)
                        {
                            pipeServer.Close();
                            sw.Dispose();
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                if (pipeServer.IsConnected)
                {
                    
                    pipeServer.Dispose();
                }
            }
        }
        
        
    }

    public class ProcessNamedPipeClient
    {
        private string _serverPipeName = "";
        private string _serverAddress =".";
        
        public ProcessNamedPipeClient(string serverPipeName)
        {
            _serverPipeName = serverPipeName;
        }
        
        public ProcessNamedPipeClient(string serverAddress, string serverPipeName)
        {
            _serverPipeName = serverPipeName;
            _serverAddress = serverAddress;
        }

        public T Call<T>(string serviceName, JObject args)
        {
            return CallAsync<T>(serviceName, args).Result;
        }

        public async Task<T> CallAsync<T>(string serviceName, JObject args)
        {
            var responseStr = await CallAsync(serviceName, args);

            return JsonConvert.DeserializeObject<T>(responseStr);
        }

        public string Call(string serviceName, JObject args)
        {
            return CallAsync(serviceName, args).Result;
        }

        public async Task<string> CallAsync(string serviceName, JObject args)
        {
            using (var client =
                new NamedPipeClientStream(".", _serverPipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                try
                {
                    client.Connect(3000);
                }
                catch (Exception e)
                {
                    //return new FailResultReturn<T>(e);
                    throw;
                }


                if (client.IsConnected)
                {
                    client.ReadMode = PipeTransmissionMode.Message;

                    using (var sr = new StreamReader(client, Encoding.UTF8))
                    using (var sw = new StreamWriter(client, Encoding.UTF8))
                    {
                        try
                        {
                            var json = new JObject()
                            {
                                ["ServiceName"] = serviceName,
                                ["Args"] = args
                            };

                            await sw.WriteLineAsync(json.ToStringEx(Formatting.None));
                            await sw.FlushAsync();

                            var responseStr =await sr.ReadLineAsync();

                            if (string.IsNullOrWhiteSpace(responseStr))
                            {
                                //return new FailResultReturn<T>("服务器回复空数据");
                                throw new Exception("服务器回复空数据");
                            }

                            await sw.WriteLineAsync("ok");
                            await sw.FlushAsync();

                            sw.Close();
                            sr.Close();

                            return responseStr;
                        }
                        catch (Exception e)
                        {
                            //return new FailResultReturn<T>(e);
                            throw;
                        }
                        finally
                        {
                            if (client.IsConnected)
                            {
                                sw.Close();
                                sr.Close();

                                sw.Dispose();
                                sr.Dispose();

                                client.Close();
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("连接服务失败");
                    //return new FailResultReturn<T>("连接服务失败");
                }
            }
        }


        //public ResultReturn<JObject> Call(string serviceName, JObject args)
        //{

        //}

        //private ResultReturn<string> callInternal(string serviceName, JObject args)
        //{
        //    using (var client =
        //        new NamedPipeClientStream(_serverAddress, _serverPipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
        //    {
        //        try
        //        {
        //            client.Connect(3000);
        //        }
        //        catch (Exception e)
        //        {
        //            return new FailResultReturn<string>(e);
        //        }


        //        if (client.IsConnected)
        //        {
        //            client.ReadMode = PipeTransmissionMode.Message;

        //            using (var sr = new StreamReader(client, Encoding.UTF8))
        //            using (var sw = new StreamWriter(client, Encoding.UTF8))
        //            {
        //                try
        //                {
        //                    var json = new JObject()
        //                    {
        //                        ["ServiceName"] = serviceName,
        //                        ["Args"] = args
        //                    };

        //                    sw.WriteLine(json.ToStringEx(Formatting.None));
        //                    sw.Flush();

        //                    var responseStr = sr.ReadLine();

        //                    if (string.IsNullOrWhiteSpace(responseStr))
        //                    {
        //                        return new FailResultReturn<string>("服务器回复空数据");
        //                    }

        //                    sw.WriteLine("ok");
        //                    sw.Flush();

        //                    sw.Close();
        //                    sr.Close();

        //                    return JsonConvert.DeserializeObject<ResultReturn<string>>(responseStr);
        //                }
        //                catch (Exception e)
        //                {
        //                    return new FailResultReturn<string>(e);
        //                }
        //                finally
        //                {
        //                    if (client.IsConnected)
        //                    {
        //                        sw.Close();
        //                        sr.Close();

        //                        sw.Dispose();
        //                        sr.Dispose();

        //                        client.Close();
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return new FailResultReturn<string>("连接服务失败");
        //        }
        //    }
        //}

        //private void callWithCallback(string serviceName, Action<string> callback)
        //{
            
        //}
    }
}
