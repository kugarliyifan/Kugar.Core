using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

//内存映射文件的处理类

namespace Kugar.Core.IO
{
    internal static class UnManageAPI
    {
        #region "API函数定义"

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public uint dwOemId;
            public uint dwPageSize;
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint dwProcessorLevel;
            public uint dwProcessorRevision;
        }

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        private const int OPEN_EXISTING = 3;
        public const int INVALID_HANDLE_VALUE = -1;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
        public const uint PAGE_READWRITE = 0x04;

        public const int FILE_MAP_COPY = 1;
        public const int FILE_MAP_WRITE = 2;
        public const int FILE_MAP_READ = 4;

        /// <summary>
        /// 内存映射文件句柄
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpFileMappingAttributes"></param>
        /// <param name="flProtect"></param>
        /// <param name="dwMaximumSizeHigh"></param>
        /// <param name="dwMaximumSizeLow"></param>
        /// <param name="lpName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateFileMapping(IntPtr hFile,
            IntPtr lpFileMappingAttributes, uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow, string lpName);

        /// <summary>
        /// 内存映射文件
        /// </summary>
        /// <param name="hFileMappingObject"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwFileOffsetHigh"></param>
        /// <param name="dwFileOffsetLow"></param>
        /// <param name="dwNumberOfBytesToMap"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint
            dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap);

        /// <summary>
        /// 撤消文件映像
        /// </summary>
        /// <param name="lpBaseAddress"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        /// <summary>
        /// 关闭内核对象句柄
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// 打开要映射的文件
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwShareMode"></param>
        /// <param name="securityAttrs"></param>
        /// <param name="dwCreationDisposition"></param>
        /// <param name="dwFlagsAndAttributes"></param>
        /// <param name="hTemplateFile"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateFile(string lpFileName,
            uint dwDesiredAccess, FileShare dwShareMode, IntPtr securityAttrs,
            FileMode dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        /// <summary>
        /// 得到文件大小
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="highSize"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint GetFileSize(IntPtr hFile, out uint highSize);

        /// <summary>
        /// 得到系统信息
        /// </summary>
        /// <param name="lpSystemInfo"></param>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        /// <summary>
        ///     将写入文件映射缓冲区的所有数据都刷新到磁盘 
        /// </summary>
        /// <param name="lpBaseAddress"></param>
        /// <param name="dwNumberOfBytesToFlush">要刷新的字节</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern bool FlushViewOfFile(IntPtr lpBaseAddress, long dwNumberOfBytesToFlush);

        #endregion
    }

    /// <summary>
    ///     内存文件映射的处理类,该类使用后,必须调用Dispose或者Close关闭文件映射,避免残留映射在内存中
    /// </summary>
    public class MemoryMappingFile:IDisposable
    {
        private IntPtr targetMappingFileHandle ;
        private long fileLength;    //文件总大小

        //文件映射的最小尺寸
        private static long systemAllocMin;

        public MemoryMappingFile(string fileName)
        {
            var sysInf=new UnManageAPI.SYSTEM_INFO();

            UnManageAPI.GetSystemInfo(ref sysInf);

            if (systemAllocMin==0L)
            {
                systemAllocMin = sysInf.dwAllocationGranularity;
            }

            OpenFile(fileName);
        }

        private void OpenFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException();
            }

            IntPtr fileHandle = UnManageAPI.CreateFile(fileName, UnManageAPI.GENERIC_READ | UnManageAPI.GENERIC_WRITE, FileShare.Read | FileShare.Write, IntPtr.Zero, FileMode.Open, UnManageAPI.FILE_ATTRIBUTE_NORMAL | UnManageAPI.FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero);

            if (UnManageAPI.INVALID_HANDLE_VALUE == (int)fileHandle)
            {
                throw new FileLoadException("文件打开出错");
            }

            uint fileSizeHigh = 0;

            //读取文件大小
            long fileSize = UnManageAPI.GetFileSize(fileHandle, out fileSizeHigh);

            IntPtr mappingFileHandle = UnManageAPI.CreateFileMapping(fileHandle, IntPtr.Zero, UnManageAPI.PAGE_READWRITE, 0, 0, "KugarMapping");
                    
            if (mappingFileHandle == IntPtr.Zero)
            {
                throw new FileLoadException("文件映射出错");
            }

            fileSize |= (((uint)fileSizeHigh) << 32);

            UnManageAPI.CloseHandle(fileHandle);

            targetMappingFileHandle = mappingFileHandle;

            fileLength = fileSize;

        }

        /// <summary>
        ///     创建整个映射文件的读写流
        /// </summary>
        /// <returns></returns>
        public MemoryMappingFileStream CreateMappingStream()
        {
            return CreateMappingStream(0, fileLength);
        }

        /// <summary>
        ///     从内存文件映射中,分段创建一个读写的流
        /// </summary>
        /// <param name="offset">读写的起始偏移</param>
        /// <param name="count">总映射的数量,建议映射的数量少于1G</param>
        /// <returns>返回一个读写流</returns>
        public MemoryMappingFileStream CreateMappingStream(long offset,long count)
        {

            var readCount = Math.Min(count, fileLength - offset);

            var realOffsetValue =offset - offset%systemAllocMin; //计算出所需的真实起始偏移

            var baseOffset = 0L;

            if (offset<systemAllocMin)
            {
                baseOffset = offset;
                readCount += offset;    
            }

            uint dwFileOffsetLow = (uint)(realOffsetValue & 0xffffffffL);
            uint dwFileOffsetHigh = (uint)(realOffsetValue >> 0x20);

            IntPtr lpbMapAddress = UnManageAPI.MapViewOfFile(targetMappingFileHandle, UnManageAPI.FILE_MAP_COPY | UnManageAPI.FILE_MAP_READ | UnManageAPI.FILE_MAP_WRITE,
                           dwFileOffsetHigh, dwFileOffsetLow,
                           (uint)readCount);

            if (lpbMapAddress==IntPtr.Zero)
            {
                throw new IOException("无法映射指定的偏移");
            }

            return new MemoryMappingFileStream(lpbMapAddress,baseOffset,readCount-baseOffset);
        }

        public void Close()
        {
            if (!_isDispose)
            {
                Disposed(true);
            }
        }

        private bool _isDispose=false;

        protected void Disposed(bool disposing)
        {
            if (_isDispose)
            {
                return;
            }

            if (disposing)
            {
                UnManageAPI.CloseHandle(targetMappingFileHandle);

                _isDispose = true;                
            }

        }

        //public override void Flush()
        //{
        //    //throw new NotImplementedException();
        //}

        //public override long Seek(long offset, SeekOrigin origin)
        //{
        //    if (offset<0 || offset>fileLength)
        //    {
        //        throw new ArgumentOutOfRangeException("offset");
        //    }

        //    var tempPosition = 0L;

        //    switch (origin)
        //    {
        //        case SeekOrigin.Begin:
        //            tempPosition = offset;
        //            break;
        //        case SeekOrigin.Current:
        //            tempPosition=currentPosition +offset;
        //            break;
        //        case SeekOrigin.End:
        //            tempPosition = fileLength - offset;
        //            break;
        //    }

        //    if (tempPosition==0 || tempPosition>fileLength)
        //    {
        //        throw new ArgumentOutOfRangeException("offset");
        //    }

        //    currentPosition = tempPosition;

        //    return tempPosition;
        //}

        //public override void SetLength(long value)
        //{
        //    //throw new NotImplementedException();
        //}

        //public override int Read(byte[] buffer, int offset, int count)
        //{
        //    if (count<=0)
        //    {
        //        throw new ArgumentOutOfRangeException("count");
        //    }

        //    var readCount = Math.Min(Math.Min(buffer.Length - offset, count), fileLength - currentPosition);

        //    var heighLength = BitConverter.ToUInt32(BitConverter.GetBytes(currentPosition), 0);
        //    var lowLength = BitConverter.ToUInt32(BitConverter.GetBytes(currentPosition), 4);

        //    IntPtr lpbMapAddress = MapViewOfFile(targetMappingFileHandle, FILE_MAP_COPY | FILE_MAP_READ | FILE_MAP_WRITE,
        //                   heighLength,lowLength, 
        //                   (uint)readCount);

        //    if (lpbMapAddress==IntPtr.Zero)
        //    {
        //        return 0;
        //    }


        //    Marshal.Copy(lpbMapAddress, buffer, offset, (int)readCount);

        //    UnmapViewOfFile(lpbMapAddress);

        //    currentPosition += readCount;

        //    return (int)readCount;
        //    //fileSize -= blockBytes;
        //}

        //public override void Write(byte[] buffer, int offset, int count)
        //{
        //    //throw new NotImplementedException();
        //}

        ////public override void Close()
        ////{
        ////    base.Close();

        ////    CloseHandle(targetFileHande);

        ////}

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);

        //    if (disposing)
        //    {
        //        CloseHandle(targetFileHandle);
        //        CloseHandle(targetMappingFileHandle);
        //    }

            
        //}

        //public override bool CanRead
        //{
        //    get { return true; }
        //}

        //public override bool CanSeek
        //{
        //    get { return true; }
        //}

        //public override bool CanWrite
        //{
        //    get { return false; }
        //}

        //public override long Length { get { return fileLength; } }
       
        //public override long Position
        //{
        //    get { return currentPosition; }
        //    set { Seek(value, SeekOrigin.Begin); }
        //}
        public void Dispose()
        {
            Disposed(true);
        }
    }

    public class MemoryMappingFileStream:Stream
    {
        private long currentPosition;
        private readonly IntPtr _mappingAddress;  //映射的地址句柄
        private readonly long _baseOffset = 0L;   //基础的偏移位移
        private long _mappingLength = 0L;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="mappingAddress">映射的基础地址</param>
        /// <param name="baseOffset">映射的基础偏移,用于修正当调用CreateMappingStream函数的时候，指定的偏移非64KB的整倍数的时候</param>
        /// <param name="mappingLength">映射的长度</param>
        internal MemoryMappingFileStream(IntPtr mappingAddress, long baseOffset, long mappingLength)
            : base()
        {
            _baseOffset = baseOffset;
            _mappingAddress = mappingAddress;
            _mappingLength = mappingLength;
        }

        public override void Flush()
        {
            var tempMappingLength = Interlocked.Read(ref _mappingLength);

            Interlocked.Add(ref tempMappingLength, _baseOffset);


            UnManageAPI.FlushViewOfFile(_mappingAddress, (int)tempMappingLength);
            //return false;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long tempPositon = 0L;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    tempPositon = offset;
                    break;
                case SeekOrigin.Current:
                    tempPositon = currentPosition + offset;
                    break;
                case SeekOrigin.End:
                    tempPositon = _mappingLength - offset;
                    break;
                
            }

            if (tempPositon<=0 || tempPositon>_mappingLength)
            {
                throw new IndexOutOfRangeException("文件偏移位置错误");
            }

            currentPosition = tempPositon;
            return tempPositon;

        }

        public override void SetLength(long value)
        {
            //throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("buffer");
            }

            if (offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count + offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var tempMappinglength = Interlocked.Read(ref _mappingLength);
            var tempCurrentPosition = Interlocked.Read(ref currentPosition);

            if (count <= 0 || tempCurrentPosition >= tempMappinglength)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var realPtr = new IntPtr((_mappingAddress.ToInt32() + (int) (tempCurrentPosition + _baseOffset)));

            //var realPtr = IntPtr.Add(_mappingAddress, (int)(tempCurrentPosition + _baseOffset));

            var readCount = Math.Min(Math.Min(buffer.Length - offset, count), tempMappinglength - tempCurrentPosition);

            try
            {
                Marshal.Copy(realPtr , buffer, offset, (int)readCount);
            }
            catch (Exception)
            {
                return 0;
            }


            Interlocked.Add(ref currentPosition, readCount);

            return (int)readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length<=0)
            {
                throw new ArgumentOutOfRangeException("buffer");
            }

            if (offset>buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count+offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var tempMappinglength = Interlocked.Read(ref _mappingLength);
            var tempCurrentPosition = Interlocked.Read(ref currentPosition);

            if (count <= 0 || tempCurrentPosition >= tempMappinglength)
            {
                throw new ArgumentOutOfRangeException("count");
            }



            var realPtr = new IntPtr((_mappingAddress.ToInt32() + (int)(tempCurrentPosition + _baseOffset)));
            //var realPtr = IntPtr.Add(_mappingAddress, (int)(tempCurrentPosition + _baseOffset));

            var writeCount = Math.Min(Math.Min(buffer.Length - offset, count), tempMappinglength - tempCurrentPosition);

            Marshal.Copy(buffer, offset, realPtr, (int)writeCount);

            Interlocked.Add(ref currentPosition, writeCount);

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                UnManageAPI.UnmapViewOfFile(_mappingAddress);
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length { get { return _mappingLength; } }

        public override long Position
        {
            get { return currentPosition; }
            set { Seek(value, SeekOrigin.Begin); }
        }
    }
}
