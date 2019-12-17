using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using Kugar.Core.ExtMethod.Serialization;

namespace Kugar.Core.DataStream
{
    public class RemoteFileCopySender
    {
        public void FileCopyTo(string filePath, IPEndPoint remotePoint)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            Socket s=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);

            try
            {
                s.Connect(remotePoint);

                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                var fileInfo = new FileCopyInfo();
                fileInfo.FileName = Path.GetFileName(filePath);
                fileInfo.FileSize = (int)fs.Length;

                s.Send(fileInfo.SerializeToByte());

                var netStream = new NetworkStream(s, true);

                var streamCoper = new StreamTransfer(fs, netStream);

                streamCoper.Copy(fileInfo.FileSize);
            }
            catch (Exception)
            {
                s.Close();
                return;
            }
        }

        public void DirectoryCopyTo(string directoryPath, IPEndPoint remotePoint)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                return;
            }

            var fl = IO.FileManager.FileList(directoryPath, true);

            if (fl==null || fl.Count<=0)
            {
                return;
            }

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                

                s.Connect(remotePoint);

                s.Send(fileInfo.SerializeToByte());

                var netStream = new NetworkStream(s, true);

                var streamCoper = new StreamTransfer(fs, netStream);

                streamCoper.Copy(fileInfo.FileSize);
            }
            catch (Exception)
            {
                s.Close();
                return;
            }
        }


    }

    public class RemoteFileCopyReceive
    {
        private int port = 1986;
        private Socket socket = null;
        private NetworkStream stream = null;
        private string receivePath = "";

        public  RemoteFileCopyReceive(int listenPort)
        {
            port = listenPort;
            socket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback,port));
            socket.Listen(100);

            socket.BeginAccept(1024,acceptConnect, socket);
        }

        private void acceptConnect(IAsyncResult iar)
        {
            byte[] firstBlockBuff = null;
            int firstBlockCount = 0;

            var socketTemp=socket.EndAccept(out firstBlockBuff, out firstBlockCount, iar);

            FileStream fs = null;

            var obj= firstBlockBuff.DeserializeToObject(0,firstBlockCount);
            int copySize = 0;
            string filePath = "";

            if (obj is FileFolderCopyInfo)
            {
                var temp = (FileFolderCopyInfo) obj;

                if(!OnFileReceiveRequest(temp))
                {
                    socketTemp.Close();
                    return;
                }

                copySize = temp.FileSize;

                var folderPath = Path.Combine(receivePath, temp.FilePath);

                IO.FileManager.CreateDirectory(folderPath);

                filePath = Path.Combine(folderPath, temp.FileName);

                //fs = new FileStream(Path.Combine(folderPath,temp.FileName)+".tmp",FileMode.CreateNew,FileAccess.ReadWrite);

            }
            else if(obj is FileCopyInfo)
            {
                var temp = (FileCopyInfo)obj;

                if (!OnFileReceiveRequest(temp))
                {
                    socketTemp.Close();
                    return;
                }

                copySize = temp.FileSize;
                filePath = Path.Combine(receivePath, temp.FileName);
                //fs = new FileStream(Path.Combine(receivePath, temp.FileName) + ".tmp", FileMode.CreateNew, FileAccess.ReadWrite);
            }

            if (copySize<=0 || string.IsNullOrEmpty(filePath))
            {
                socketTemp.Close();
                return;
            }

            string tempFileName = filePath + ".tmp";

            fs = new FileStream(tempFileName, FileMode.CreateNew, FileAccess.ReadWrite);

            stream = new NetworkStream(socketTemp, FileAccess.Read, true);

            var st = new StreamTransfer(stream, fs);
            st.IsAsyn = true;
            st.StreamTransferCompleted += (s, e1) =>
                                              {
                                                  stream.Close();
                                                  fs.Close();

                                                  if (e1.HasError)
                                                  {
                                                      File.Delete(tempFileName);
                                                  }
                                                  else
                                                  {
                                                      File.Move(tempFileName, filePath);
                                                  }

                                                  OnFileReceiveCompleted(e1.Error, filePath);
                                              };

            st.Copy(copySize);

        }

        protected void OnFileReceiveCompleted(Exception error,string fileName)
        {
            if (FileReceiveCompleted != null)
            {
                FileReceiveCompleted(this, new FileReceiveCompletedEventArgs(error, fileName));
            }
        }

        protected bool OnFileReceiveRequest(FileCopyInfo info)
        {
            var e = new FileReceiveRequestEventArgs(info);

            if (FileReceiveRequest != null)
            {
                FileReceiveRequest(this, e);
            }

            return e.IsEnabled;
        }

        /// <summary>
        ///     当接收完文件后引发该事件
        /// </summary>
        public event EventHandler<FileReceiveCompletedEventArgs> FileReceiveCompleted;

        /// <summary>
        ///     当请求接收文件时引发该事件
        /// </summary>
        public event EventHandler<FileReceiveRequestEventArgs> FileReceiveRequest;
    }

    public class FileReceiveCompletedEventArgs:EventArgs
    {
        public FileReceiveCompletedEventArgs(Exception error, string sourceFileName)
        {
            Error = error;
            SourceFileName = sourceFileName;
        }

        public Exception Error { get; private set; }
        public bool HasError { get { return Error != null; } }
        public string SourceFileName { get; private set; }
    }

    public class FileReceiveRequestEventArgs : EventArgs
    {
        public FileReceiveRequestEventArgs(FileCopyInfo info)
        {
            RequestInfo = info;

            IsEnabled = true;
        }

        public FileCopyInfo RequestInfo { private set; get; }

        public bool IsEnabled { set; get; }
    }

    [Serializable]
    public class FileCopyInfo
    {
        public string FileName;
        public int FileSize;
        public short FileCRC;
    }

    [Serializable]
    public class FileFolderCopyInfo : FileCopyInfo
    {
        public string FilePath;
    }
}
