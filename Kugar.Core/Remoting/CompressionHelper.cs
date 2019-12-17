using System;
using System.IO;
using NZlib.Compression;
using NZlib.Streams;

namespace Kugar.Core.Remoting
{

	public class CompressionHelper 
	{

		/// <summary>
		/// refactor  by zendy
		/// </summary>
		/// <param name="inStream"></param>
		/// <returns></returns>
		public static Stream getCompressedStreamCopy(Stream inStream) 
		{
			MemoryStream outStream = new MemoryStream();
			Deflater mDeflater = new Deflater(Deflater.BEST_COMPRESSION);
			DeflaterOutputStream compressStream = new DeflaterOutputStream(outStream,mDeflater);

			byte[] buf = new Byte[4096];
			int cnt = inStream.Read(buf,0,4096);
			while (cnt>0) {
				compressStream.Write(buf,0,cnt);
				cnt = inStream.Read(buf,0,4096);
			}
			compressStream.Finish();
			//modify by zendy //这个设置非常重要,否则会导致后续Sink在处理该stream时失败,在原来的源码中就是因为没有这个处理导致程序运行失败
			outStream.Seek(0,SeekOrigin.Begin);
			return outStream;
		}

		/// <summary>
		/// refactor  by zendy
		/// </summary>
		/// <param name="inStream"></param>
		/// <returns></returns>
		public static Stream getUncompressedStreamCopy(Stream inStream) 
		{
			InflaterInputStream unCompressStream = new InflaterInputStream(inStream); 
			MemoryStream outStream = new MemoryStream();
			int mSize;
			Byte[] mWriteData = new Byte[4096];
			while(true)
			{
				mSize = unCompressStream.Read(mWriteData, 0, mWriteData.Length);
				if (mSize > 0)
				{
					outStream.Write(mWriteData, 0, mSize);
				}
				else
				{
					break;
				}
			}
			unCompressStream.Close();
			//modify by zendy//这个设置非常重要,否则会导致后续Sink在处理该stream时失败,,在原来的源码中就是因为没有这个处理导致程序运行失败
			outStream.Seek(0,SeekOrigin.Begin);
			return outStream;
		}
	}
}
