using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.ExtMethod
{
    /// <summary>
    ///     流压缩的类
    /// </summary>
    public static class StreamCompression
    {
        /// <summary>
        ///     压缩数据流
        /// </summary>
        /// <param name="ms">要压缩的数据流</param>
        /// <param name="type">压缩使用的算法</param>
        /// <returns>返回压缩后的数据流</returns>
        public static Stream CompressStream(this Stream ms, CompressType type)
        {
            Stream compressStream = null;

            var resultMs = new MemoryStream(4096);


            if (ms.CanSeek)
            {
                ms.Seek(0, SeekOrigin.Begin);
            }

            switch (type)
            {
                case CompressType.Zip_GZip:
                    compressStream = new GZipStream(resultMs, CompressionMode.Compress, true);
                    StreamAbout.CopyTo(ms, compressStream);
                    compressStream.Flush();
                    compressStream.Close();
                    break;
                case CompressType.Zip_Deflate:
                    compressStream = new DeflateStream(resultMs, CompressionMode.Compress, true);
                    StreamAbout.CopyTo(ms, compressStream);
                    compressStream.Flush();
                    compressStream.Close();
                    break;
                default:
                    return ms;
            }

            resultMs.Seek(0, SeekOrigin.Begin);

            return resultMs;
        }

        /// <summary>
        ///     压缩数据流
        /// </summary>
        /// <param name="ms">要压缩的数据流</param>
        /// <returns>返回压缩后的数据流</returns>
        public static Stream CompressStream(this Stream ms)
        {
            return CompressStream(ms, CompressType.Zip_GZip);
        }

        /// <summary>
        ///     解压缩数据流中的数据
        /// </summary>
        /// <param name="ms">要解压缩的数据流</param>
        /// <param name="type">解压缩使用的算法</param>
        /// <returns>返回解压缩后的数据流</returns>
        public static Stream UnCompressStream(this Stream ms, CompressType type)
        {

            Stream compressStream = null;

            MemoryStream resultMs = null;
            if (ms.CanSeek)
            {
                resultMs = new MemoryStream((int)ms.Length);
                ms.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                resultMs = new MemoryStream(4096);
            }
           

            switch (type)
            {
                case CompressType.Zip_GZip:
                    compressStream = new GZipStream(ms, CompressionMode.Decompress, true);
                    StreamAbout.CopyTo(compressStream, resultMs);
                    compressStream.Flush();
                    compressStream.Close();
                    break;
                case CompressType.Zip_Deflate:
                    compressStream = new DeflateStream(ms, CompressionMode.Decompress, true);
                    StreamAbout.CopyTo(compressStream, resultMs);
                    compressStream.Flush();
                    compressStream.Close();
                    break;
                default:
                    return ms;
            }

            resultMs.Seek(0, SeekOrigin.Begin);

            return resultMs;
        }

        /// <summary>
        ///     解压缩数据流中的数据
        /// </summary>
        /// <param name="ms">要解压缩的数据流</param>
        /// <returns>返回解压缩后的数据流</returns>
        public static Stream UnCompressStream(this Stream ms)
        {
            return UnCompressStream(ms, CompressType.Zip_GZip);
        }
    }

    public static class ByteCompression
    {

        #region "压缩数据,返回压缩后的数据"

        /// <summary>
        ///     压缩指定的byte数组
        /// </summary>
        /// <param name="buffer">将要压缩的数组</param>
        /// <returns>返回压缩后的字节数组</returns>
        public static byte[] CompressToByteArray(this byte[] buffer)
        {
            return CompressToByteArray(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///     压缩指定的byte数组
        /// </summary>
        /// <param name="buffer">将要压缩的数组</param>
        /// <param name="offset">起始位置偏移</param>
        /// <param name="count">压缩的总数量</param>
        /// <returns>返回压缩后的字节数组</returns>
        public static byte[] CompressToByteArray(this byte[] buffer,int offset,int count)
        {
            using (var bs=new ByteStream(buffer))
            using (var cs=bs.CompressStream() )
            {
                return cs.ReadAllBytes();
            }
        }

        #endregion

        #region "压缩,输出到指定的缓冲区,并返回解压缩后的数据长度"

        /// <summary>
        ///     将压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回压缩后的数据长度</returns>
        public static int CompressToByteArray(this byte[] buffer, byte[] outPutBuff)
        {
            return CompressToByteArray(buffer, 0, buffer.Length, outPutBuff, 0);
        }

        /// <summary>
        ///     将压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回压缩后的数据长度</returns>
        public static int CompressToByteArray(this byte[] buffer, int srcBuffOffset, byte[] outPutBuff)
        {
            return CompressToByteArray(buffer, srcBuffOffset, buffer.Length - srcBuffOffset, outPutBuff, 0);
        }


        /// <summary>
        ///     将压缩后的数据写入了指定的字节数组中,从输出缓冲区的0偏移开始写入
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="srcBuffCount">源数组中要压缩的数据的数量</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回压缩后的数据长度</returns>
        public static int CompressToByteArray(this byte[] buffer, int srcBuffOffset, int srcBuffCount, byte[] outPutBuff)
        {
            return CompressToByteArray(buffer, srcBuffOffset, srcBuffCount, outPutBuff, 0);
        }


        /// <summary>
        ///     将压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="srcBuffCount">源数组中要压缩的数据的数量</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <param name="outPutBuffOffset">输出缓冲区的起始位置偏移</param>
        /// <returns>返回压缩后的数据长度</returns>
        public static int CompressToByteArray(this byte[] buffer,int srcBuffOffset,int srcBuffCount,byte[] outPutBuff,int outPutBuffOffset)
        {
            if (outPutBuff==null || outPutBuff.Length<=0 || outPutBuffOffset>=outPutBuff.Length)
            {
                throw new ArgumentOutOfRangeException("outPutBuff");
            }

            using (var bs = new ByteStream(buffer,srcBuffOffset,srcBuffCount))
            using (var cs = bs.CompressStream())
            using (var outStream=new ByteStream(outPutBuff,outPutBuffOffset))
            {
                return StreamAbout.CopyTo(cs, outStream);
            }
        }

        #endregion

        #region "解压缩,返回数据流"

        /// <summary>
        ///     压缩一个字节数组，并返回压缩后的流
        /// </summary>
        /// <param name="buffer">将要压缩的字节数组</param>
        /// <returns></returns>
        public static Stream CompressToSteam(this byte[] buffer)
        {
            using (var bs = new ByteStream(buffer))
            {
                return bs.CompressStream();
            }
        }

        #endregion

        #region "解压缩,返回解压缩后的数据"

        /// <summary>
        ///     解压缩压缩指定的byte数组
        /// </summary>
        /// <param name="buffer">将要解压缩的数组</param>
        /// <returns>返回解压缩后的字节数组</returns>
        public static byte[] UnCompressToByteArray(this byte[] buffer)
        {
            return UnCompressToByteArray(buffer, 0, buffer.Length);
        }

        /// <summary>
        ///     解压缩压缩指定的byte数组
        /// </summary>
        /// <param name="buffer">将要解压缩的数组</param>
        /// <param name="offset">起始位置偏移</param>
        /// <param name="count">压缩的总数量</param>
        /// <returns>返回解压缩后的字节数组</returns>
        public static byte[] UnCompressToByteArray(this byte[] buffer,int offset,int count)
        {
            using (var bs = new ByteStream(buffer,offset,count))
            using (var cs = bs.UnCompressStream())
            {
                return cs.ReadAllBytes();
            }
        }

        #endregion

        #region "解压缩到指定的缓冲区中,返回解压缩后的数据长度"

        /// <summary>
        ///     将解压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回解压缩后的数据长度</returns>
        public static int UnCompressToByteArray(this byte[] buffer, byte[] outPutBuff)
        {
            return UnCompressToByteArray(buffer, 0, buffer.Length, outPutBuff, 0);
        }

        /// <summary>
        ///     将解压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回解压缩后的数据长度</returns>
        public static int UnCompressToByteArray(this byte[] buffer, int srcBuffOffset, byte[] outPutBuff)
        {
            return UnCompressToByteArray(buffer, srcBuffOffset, buffer.Length - srcBuffOffset, outPutBuff, 0);
        }

        /// <summary>
        ///     将解压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="srcBuffCount">源数组中要压缩的数据的数量</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <returns>返回解压缩后的数据长度</returns>
        public static int UnCompressToByteArray(this byte[] buffer, int srcBuffOffset, int srcBuffCount, byte[] outPutBuff)
        {
            return UnCompressToByteArray(buffer, srcBuffOffset, srcBuffCount, outPutBuff, 0);
        }

        /// <summary>
        ///     将解压缩后的数据写入了指定的字节数组中
        /// </summary>
        /// <param name="buffer">将要压缩的源字节数组</param>
        /// <param name="srcBuffOffset">源数组的起始位置偏移</param>
        /// <param name="srcBuffCount">源数组中要压缩的数据的数量</param>
        /// <param name="outPutBuff">用于输出的缓冲区</param>
        /// <param name="outPutBuffOffset">输出缓冲区的起始位置偏移</param>
        /// <returns>返回解压缩后的数据长度</returns>
        public static int UnCompressToByteArray(this byte[] buffer, int srcBuffOffset, int srcBuffCount, byte[] outPutBuff, int outPutBuffOffset)
        {
            if (outPutBuff == null || outPutBuff.Length <= 0 || outPutBuffOffset >= outPutBuff.Length)
            {
                throw new ArgumentOutOfRangeException("outPutBuff");
            }

            using (var bs = new ByteStream(buffer, srcBuffOffset, srcBuffCount))
            using (var cs = bs.UnCompressStream())
            using (var outStream = new ByteStream(outPutBuff, outPutBuffOffset))
            {
                return StreamAbout.CopyTo(cs, outStream);
            }
        }

        #endregion
    }



    /// <summary>
    ///     使用的压缩方式
    /// </summary>
    public enum CompressType
    {
        /// <summary>
        ///     使用Zip压缩方式中的GZip算法，无法支持超过4G的数据
        /// </summary>
        Zip_GZip,

        /// <summary>
        ///     使用Zip压缩方式中的Deflate算法，无法支持超过4G的数据
        /// </summary>
        Zip_Deflate
    }
}
