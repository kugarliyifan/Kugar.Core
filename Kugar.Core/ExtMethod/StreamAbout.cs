using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.ExtMethod
{
    

    public static class StreamAbout
    {
        /// <summary>
        ///     将Steam对象中的数据保存到指定的文件中,写入后,如果Steam对象支持Seek方法,则会将指针回复到复制前的位置
        /// </summary>
        /// <param name="stream">输入的数据流</param>
        /// <param name="filePath">保存文件的目标地址</param>
        /// <param name="mode">打开文件的方式</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool SaveToFile(this Stream stream,string filePath,FileMode mode)
        {
            if (stream==null)
            {
                return false;
            }

            var startPos = 0L;

            if (stream.CanSeek)
            {
                startPos = stream.Position;
            }

            using (var file=new FileStream(filePath,mode))
            {
                var b = new byte[1024];

                int getByteSize = stream.Read(b, 0, b.Length);

                while (getByteSize > 0)
                {

                    file.Write(b, 0, getByteSize);

                    getByteSize = stream.Read(b, 0, b.Length);

                }
            }

            if (stream.CanSeek)
            {
                stream.Seek(startPos, SeekOrigin.Begin);
            }

            return true;
        }

        /// <summary>
        ///     将内存流中的数据保存到指定的文件中,,如果Steam对象支持Seek方法,则会将指针回复到复制前的位置
        /// </summary>
        /// <param name="msStream">输入的内存流</param>
        /// <param name="filePath">保存文件的目标地址</param>
        /// <param name="mode">打开文件的方式</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool SaveToFile(this MemoryStream msStream,string filePath,FileMode mode)
        {
            if (msStream==null || msStream.Length<=0)
            {
                return false;
            }

            var startPos = 0L;

            if (msStream.CanSeek)
            {
                startPos = msStream.Position;
            }
            

            using (Stream localFile = new FileStream(filePath, mode))
            {
                localFile.Write(msStream.ToArray(), 0, (int)msStream.Length);
            }

            if (msStream.CanSeek)
            {
                msStream.Seek(startPos, SeekOrigin.Begin);
            }

            return true;
        }

        /// <summary>
        ///     创建对应的读取器
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding">编码类型</param>
        /// <returns></returns>
        public static StreamReader GetReader(this Stream stream,Encoding encoding)
        {
            if (stream.CanRead == false)
                throw new InvalidOperationException("Stream does not support reading.");

            encoding = (encoding ?? Encoding.Unicode);
            return new StreamReader(stream, encoding);
        }

        /// <summary>
        ///     创建对应的读取器
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static StreamReader GetReader(this Stream stream)
        {
            return stream.GetReader(null);
        }

        public static StreamWriter GetWriter(this Stream stream)
        {
            return stream.GetWriter(null);
        }

        public static StreamWriter GetWriter(this Stream stream, Encoding encoding)
        {
            if (stream.CanWrite == false)
                throw new InvalidOperationException("Stream does not support writing.");

            encoding = (encoding ?? Encoding.Unicode);
            return new StreamWriter(stream, encoding);
        }


        public static string ReadToEnd(this Stream stream, Encoding encoding=null)
        {
            encoding = (encoding ?? Encoding.UTF8);

            var startPos = 0L;

            if (stream.CanSeek)
            {
                startPos = stream.Position;
            }

            var reader = stream.GetReader(encoding);
            
                var data = reader.ReadToEnd();

                if (stream.CanSeek)
                {
                    stream.Seek(startPos, SeekOrigin.Begin);
                }
                reader.Close();
                
                return data;
            


        }

        /// <summary>
        ///     如果当前数据流允许使用Seek方法，则读取指定数据之后，回到原来的Position,否则与使用Read一样
        /// </summary>
        public static void ReadNoMove(this Stream stream,byte[] buffer,int offset,int count)
        {
            if (stream.CanRead)
            {
                throw new InvalidOperationException("指定流不允许Read操作");
            }

            var oldPos = 0L;

            if (stream.CanSeek)
            {
                oldPos = stream.Position;
            }

            stream.Read(buffer, offset, count);

            if (stream.CanSeek)
            {
                stream.Seek(oldPos, SeekOrigin.Begin);
            }
        }



        /// <summary>
        /// 	复制当前流到指定的流中,如果源Stream或目标Stream对象支持Seek方法,则会将指针回复到复制前的位置
        /// </summary>
        /// <param name = "stream">源数据流</param>
        /// <param name = "targetStream">目标数据流</param>
        /// <returns>返回复制的数据个数</returns>
#if (NET2 || SILVERLIGHT)
        public static int CopyTo(this Stream stream, Stream targetStream)
#else
        public static int CopyTo(Stream stream, Stream targetStream)
#endif
        {
            return CopyTo(stream,targetStream, 4096);
        }

        /// <summary>
        /// 	复制当前流到指定的流中,如果源Stream或目标Stream对象支持Seek方法,则会将指针回复到复制前的位置
        /// </summary>
        /// <param name = "stream">源数据流</param>
        /// <param name = "targetStream">目标数据流</param>
        /// <param name = "bufferSize">每次读写的块大小</param>
        /// <returns>返回复制的数据个数</returns> 
#if (NET2 || SILVERLIGHT)
        public static int CopyTo(this Stream stream, Stream targetStream, int bufferSize)
#else
        public static int CopyTo(Stream stream, Stream targetStream, int bufferSize)
#endif
        {
            if (stream.CanRead == false)
                throw new InvalidOperationException("Source stream does not support reading.");
            if (targetStream.CanWrite == false)
                throw new InvalidOperationException("Target stream does not support writing.");

            if (bufferSize<=0)
            {
                throw new InvalidOperationException("bufferSize must more than 0");
            }

            var buffer = new byte[bufferSize];
            int bytesRead;

            var srcStartPos = 0L;   //记录源数据流的开始复制的位置,用于在复制后,将指针调回复制前的位置
            var desStartPos = 0L;  
            if (stream.CanSeek)
            {
                srcStartPos = stream.Position;
            }

            if (targetStream.CanSeek)
            {
                desStartPos = targetStream.Position;
            }

            int totalWriteCount = 0;
            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
                totalWriteCount += bytesRead;
            }
                

            if (targetStream.CanSeek)
            {
                targetStream.Seek(desStartPos, SeekOrigin.Begin);
            }

            if (stream.CanSeek)
            {
                stream.Seek(srcStartPos, SeekOrigin.Begin);
            }

            return totalWriteCount;
        }


        /// <summary>
        ///     复制到内存流中,如果源Stream对象支持Seek方法,则会将指针回复到复制前的位置
        /// </summary>
        /// <param name="stream">源数据流</param>
        /// <returns>返回复制到的内存流</returns>
        public static MemoryStream CopyToMemory(this Stream stream)
        {
            MemoryStream memoryStream = null;

            if (stream.CanSeek)
            {
                memoryStream = new MemoryStream((int)stream.Length);
            }
            else
            {
                memoryStream=new MemoryStream();    
            }



            CopyTo(stream, memoryStream);

            memoryStream.Seek(0L, SeekOrigin.Begin);

            return memoryStream;
        }

        /// <summary>
        ///     按字节读取指定流中的数据,如果源Stream或目标Stream对象支持Seek方法,则会将指针回复到读取前的位置
        /// </summary>
        /// <param name="stream">源数据流</param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream instream)
        {
            if (!instream.CanRead)
            {
                throw new InvalidOperationException("指定流不允许Read操作");
            }

            if (instream is MemoryStream)
            {
                return ((MemoryStream)instream).ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

            //var tempBuffer = new byte[2048];
            //var readCount = 0;

            //var pos = 0L;

            //if (stream.CanSeek)
            //{
            //    pos = stream.Length;
            //}

            //var list = stream.CanSeek ? new List<byte>((int)stream.Length) : new List<byte>(2048);

            //do
            //{
            //    readCount = stream.Read(tempBuffer, 0, 2048);

            //    if (readCount==2048)
            //    {
            //        list.AddRange(tempBuffer);
            //    }
            //    else
            //    {
            //        for (int i = 0; i < readCount; i++)
            //            list.Add(tempBuffer[i]);
            //    }
            //} while (readCount>0);

            //if (stream.CanSeek)
            //{
            //    stream.Position = pos;
            //}

            //return list.ToArray();

            //using (var memoryStream = stream.CopyToMemory())
            //    return memoryStream.ToArray();
        }

        public static async Task<byte[]> ReadAllBytesAsync(this Stream instream)
        {
            if (!instream.CanRead)
            {
                throw new InvalidOperationException("指定流不允许Read操作");
            }

            if (instream is MemoryStream)
            {
                return ((MemoryStream)instream).ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                await instream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }

            //var tempBuffer = new byte[2048];
            //var readCount = 0;

            //var pos = 0L;

            //if (stream.CanSeek)
            //{
            //    pos = stream.Length;
            //}

            //var list = stream.CanSeek ? new List<byte>((int)stream.Length) : new List<byte>(2048);

            //do
            //{
            //    readCount =await stream.ReadAsync(tempBuffer, 0, 2048);

            //    if (readCount == 2048)
            //    {
            //        list.AddRange(tempBuffer);
            //    }
            //    else
            //    {
                    
            //        for (int i = 0; i < readCount; i++)
            //            list.Add(tempBuffer[i]);
            //    }
            //} while (readCount > 0);

            //if (stream.CanSeek)
            //{
            //    stream.Position = pos;
            //}

            //return list.ToArray();

            //using (var memoryStream = stream.CopyToMemory())
            //    return memoryStream.ToArray();
        }

    }

    public static class TextStreamExt
    {
        public static void WriteLineEx(this TextWriter writer, params string[] data)
        {
            foreach (var str in data)
            {
                writer.WriteLine(str);
            }
        }
    }
}
