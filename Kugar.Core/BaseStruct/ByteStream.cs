using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    ///     使用数据流的方式处理Byte数组
    /// </summary>
    public class ByteStream : Stream
    {
        private byte[] dataBuff = null;
        private int pos = 0;
        private int baseOffset = 0;
        private int totalCount = 0;

        public ByteStream(byte[] data,int offset)
            : this(data, offset, data.Length - offset)
        {
        }

        public ByteStream(byte[] data):this(data,0,data.Length)
        {
            //dataBuff = data;
        }

        public ByteStream(byte[] data,int offset,int count)
        {
            dataBuff = data;
            baseOffset = offset;
            totalCount = count;
            pos = offset;
        }

        public byte[] SourceArray { get { return dataBuff; } }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int newPos = 0;

            Interlocked.Exchange(ref newPos, pos);

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPos = (int) offset+baseOffset;
                    break;
                case SeekOrigin.Current:
                    newPos += (int) offset;
                    break;
                case SeekOrigin.End:
                    newPos = dataBuff.Length - (int)offset;
                    break;
            }

            if (newPos<baseOffset)
            {
                newPos = baseOffset;
            }

            if (newPos > baseOffset+totalCount || newPos < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            Interlocked.Exchange(ref pos, newPos);

            return newPos;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("不支持设置长度");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length <= 0 || offset > buffer.Length - 1)
            {
                throw new ArgumentOutOfRangeException("buffer");
            }

            if (pos>=baseOffset+totalCount)
            {
                return 0;
            }

            int newCount = 0;

            var newPos = pos;

            lock (dataBuff)
            lock (buffer)
            {
                newCount = Math.Min(count, totalCount - (pos - baseOffset));

                Array.Copy(dataBuff, newPos, buffer, offset, newCount);
            }

            Interlocked.Add(ref pos, newCount);             

            return newCount;
        }

        public override void WriteByte(byte value)
        {
            if (pos >= baseOffset + totalCount)
            {
                return;
            }

            var newPos = pos;
            int newCount = 0;

            lock (dataBuff)
            {
                newCount = Math.Min(1, totalCount - (pos - baseOffset));

                if (newCount <= 0)
                {
                    return;
                }

                dataBuff[newPos] = value;

                //Array.Copy(buffer   , offset, dataBuff, newPos, newCount);
            }


            Interlocked.Add(ref pos, newCount);
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }



        public void Write(int value)
        {
            Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void Write(short value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void Write(ushort value)
        {
            Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void Write(byte value)
        {
            this.WriteByte(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length <= 0 || offset > buffer.Length - 1)
            {
                throw new ArgumentOutOfRangeException("buffer");
            }

            if (pos >= baseOffset + totalCount)
            {
                return ;
            }

            var newPos = pos;
            int newCount = 0;

            lock (dataBuff)
            lock (buffer)
            {
                newCount = Math.Min(count, totalCount-(pos-baseOffset));

                if (newCount <= 0)
                {
                    return;
                }

                Array.Copy(buffer, offset, dataBuff, newPos, newCount);
            }

            
            Interlocked.Add(ref pos, newCount);
        }

        public void Write(IList<ArraySegment<byte>> dataLst )
        {
            for (int i = 0; i < dataLst.Count; i++)
            {
                var item = dataLst[i];

                this.Write(item.Array,item.Offset,item.Count);
            }
        }

        public void Write(IEnumerable<ArraySegment<byte>> dataLst)
        {
            foreach (var item in dataLst)
            {
                this.Write(item.Array, item.Offset, item.Count);
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
            get { return true; }
        }

        public override long Length
        {
            get { return totalCount; }
        }

        public override long Position
        {
            get { return pos-baseOffset; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override void Close()
        {
            Interlocked.Exchange(ref dataBuff, null);
            base.Close();
        }
    }


}
