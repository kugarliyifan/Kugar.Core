using System;
using System.Collections.Generic;
using System.Text;

#if NET2

namespace System.ServiceModel.Channels
{
    public abstract class BufferManager
    {
        protected BufferManager()
        {
        }

        public abstract void Clear();

        public static BufferManager CreateBufferManager(
          long maxBufferPoolSize, int maxBufferSize)
        {
            return new DefaultBufferManager(maxBufferPoolSize, maxBufferSize);
        }

        public abstract void ReturnBuffer(byte[] buffer);

        public abstract byte[] TakeBuffer(int bufferSize);

#if DEBUG_BUFFER
    internal abstract void DumpStats ();
#endif

        class DefaultBufferManager : BufferManager
        {
            const int log_min = 5;   // Anything smaller than 1 << log_cut goes into the first bucket
            long max_pool_size;
            int max_size;
            List<byte[]>[] buffers = new List<byte[]>[32 - log_min];
            private object lockerObj=new object();


#if DEBUG_BUFFER
      internal override void DumpStats ()
      {
        Console.WriteLine ("- hit={0} miss={1}-", hits, miss);
        for (int i = 0; i < buffers.Length; i++){
          if (buffers [i] == null)
            continue;
          
          Console.Write ("Slot {0} - {1} [", i, buffers [i].Count);
          byte [][] arr = buffers [i].ToArray ();
          
          for (int j = 0; j < Math.Min (3, arr.Length); j++)
            Console.Write ("{0} ", arr [j].Length);
          Console.WriteLine ("]");
        }
      }
#endif

            static int log2(uint n)
            {
                int pos = 0;
                if (n >= 1 << 16)
                {
                    n >>= 16;
                    pos += 16;
                }
                if (n >= 1 << 8)
                {
                    n >>= 8;
                    pos += 8;
                }
                if (n >= 1 << 4)
                {
                    n >>= 4;
                    pos += 4;
                }
                if (n >= 1 << 2)
                {
                    n >>= 2;
                    pos += 2;
                }
                if (n >= 1 << 1)
                    pos += 1;

                return ((n == 0) ? (-1) : pos);
            }

            public DefaultBufferManager(long maxBufferPoolSize, int maxBufferSize)
            {
                this.max_pool_size = maxBufferPoolSize;
                this.max_size = maxBufferSize;
            }

            public override void Clear()
            {
                foreach (var stack in buffers)
                {
                    if (stack == null)
                        continue;
                    stack.Clear();
                }
                Array.Clear(buffers, 0, buffers.Length);
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                if (buffer == null)
                    return;

                uint size = (uint)buffer.Length;
                int l2 = log2(size);
                if (l2 > log_min)
                    l2 -= log_min;

                List<byte[]> returned = buffers[l2];

                lock (lockerObj)
                {
                    if (returned == null)
                        returned = buffers[l2] = new List<byte[]>();

                    returned.Add(buffer);                    
                }


            }

            int hits, miss;

            public override byte[] TakeBuffer(int bufferSize)
            {
                if (bufferSize < 0 || (max_size >= 0 && bufferSize > max_size))
                    throw new ArgumentOutOfRangeException();

                int l2 = log2((uint)bufferSize);
                if (l2 > log_min)
                    l2 -= log_min;

                List<byte[]> returned = buffers[l2];
                if (returned == null || returned.Count == 0)
                    return new byte[bufferSize];
                
                byte[] geterBuff = null;
                lock(lockerObj)
                {
                    
                    foreach (var e in returned)
                    {
                        if (e.Length >= bufferSize)
                        {
                            hits++;

                            geterBuff = e;
                            break;

                            //returned.Remove(e);
                            //return e;
                        }
                    }

                    returned.Remove(geterBuff);
                }

                if (geterBuff!=null)
                {
                    
                    return geterBuff;
                }
                else
                {
                    return new byte[bufferSize];
                }
            }
        }
    }
}
#endif
