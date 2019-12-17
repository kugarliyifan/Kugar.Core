using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.IO;
using System.Threading;
using Kugar.Core.MultiThread;

namespace Kugar.Core.DataStream
{
    /// <summary>
    ///     一个带进度事件的数据流复制类
    /// </summary>
    public class StreamTransfer
    {
        private const int bufferCount = 1024; //每次1K
        private Stream _desSteam = null;
        private Stream _srcStream = null;
        private bool _isCancel = false;

        private StreamTransfer()
        {
            IsAsyn = false;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="srcStream"></param>
        /// <param name="desSteam"></param>
        public StreamTransfer(Stream srcStream, Stream desSteam)
            : this()
        {

            _desSteam = desSteam;
            _srcStream = srcStream;
        }

        /// <summary>
        ///     开始复制
        /// </summary>
        public void Copy(int streamSize)
        {
            _isCancel = false;

            if (IsAsyn)
            {
                ThreadPool.QueueUserWorkItem(copyFile, streamSize);
            }
            else
            {
                copyFile(streamSize);
            }
        }

        /// <summary>
        ///     停止复制
        /// </summary>
        public void Cancel()
        {
            _isCancel = true;
        }

        /// <summary>
        ///     是否异步复制
        /// </summary>
        public bool IsAsyn { set; get; }

        /// <summary>
        ///     数据流传输时,每传输完一个数据块之后,就回调一次该事件,,可以用做进度显示
        /// </summary>
        public event EventHandler<StreamTransferProcessingEventArgs> StreamTransferProcessing;

        /// <summary>
        ///     当数据流复制完成后,会会掉该事件
        /// </summary>
        public event EventHandler<StreamTransferCompleteEventArgs> StreamTransferCompleted;

        private void copyFile(object state)
        {
            if (_srcStream == null || !_srcStream.CanRead)
            {
                throw new FileLoadException("无法读取指定文件");
            }

            var buf = new byte[bufferCount];
            int readCount;
            var totalCount = 0;

            //if (_srcStream.CanSeek)
            //{
            totalCount = (int)state;
            //}

            _srcStream.ReadTimeout = 1000;
            _desSteam.WriteTimeout = 1000;
            Exception error = null;

            bool isCanceled = false;
            int allCopied = 0;

            do
            {
                if (_isCancel)
                {
                    isCanceled = true;
                    break;
                }

                readCount = _srcStream.Read(buf, 0, bufferCount);

                if (readCount>0 & allCopied<totalCount)
                {
                    try
                    {
                        _desSteam.Write(buf, 0, Math.Min(readCount,totalCount-allCopied));
                        allCopied += readCount;
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                        break;
                    }

                    MultiThreadQueue.Default.Call(OnFileTransferProcess, this, new StreamTransferProcessingEventArgs(readCount, totalCount));
                }
            } while (readCount >= bufferCount && allCopied < totalCount);

            if (error == null && isCanceled)
            {
                error = new StreamTransferCancel();
            }

            OnFileTransferCompleted(error,allCopied);
        }

        protected void OnFileTransferProcess(object sender,EventArgs e)
        {
            if (StreamTransferProcessing!=null)
            {
                StreamTransferProcessing(sender, (StreamTransferProcessingEventArgs)e);
            }
        }

        protected void OnFileTransferCompleted(Exception error,int copiedCount)
        {
            if (StreamTransferCompleted != null)
            {
                StreamTransferCompleted(this, new StreamTransferCompleteEventArgs(error, copiedCount));
            }
        }

    }

    public class StreamTransferCancel:Exception
    {
        public StreamTransferCancel():base("数据流复制已取消"){}
    }

    public class StreamTransferCompleteEventArgs:EventArgs
    {
        public StreamTransferCompleteEventArgs(Exception error,int copiedCount)
        {
            Error = error;
            CopiedCount = copiedCount;
        }

        /// <summary>
        ///     已复制的总字节数
        /// </summary>
        public int CopiedCount {private set; get; }

        /// <summary>
        ///     错误类型
        /// </summary>
        public Exception Error { private set; get; }

        public bool HasError { get { return Error != null; } }
    }

    public class StreamTransferProcessingEventArgs:EventArgs
    {
        public StreamTransferProcessingEventArgs(int copiedCount, int copiedTotalCount)
        {
            CopiedCount = copiedCount;
            CopiedTotalCount = copiedTotalCount;
        }

        public int CopiedCount { protected set; get; }
        public int CopiedTotalCount { protected set; get; }
    }
}
