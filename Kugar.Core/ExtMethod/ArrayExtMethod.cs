using System;

namespace Kugar.Core.ExtMethod
{
    public static class ArrayExtMethod
    {
        /// <summary>
        ///    数据项移动 <br/>
        ///   将数组中的某个区块从sourceIndex移动到desIndex开始的索引
        /// </summary>
        /// <param name="array">源数组</param>
        /// <param name="sourceIndex">要移动的区块的起始索引</param>
        /// <param name="moveCount">移动的总数量</param>
        /// <param name="desIndex">移动的目的索引</param>
        /// <returns></returns>
        
        public static int MoveItems(this Array array, int sourceIndex, int moveCount, int desIndex)
        {
            if (array==null || sourceIndex < 0 || desIndex < 0 || sourceIndex > array.Length || desIndex > array.Length || sourceIndex + moveCount > array.Length || desIndex + moveCount > array.Length || sourceIndex == desIndex)
            {
                throw new ArgumentOutOfRangeException("array");
            }

            var moveCountPerOne = 0; //每次移动的区块大小

            moveCountPerOne = Math.Min(Math.Abs(sourceIndex - desIndex), moveCount);

            if (moveCountPerOne<=0)
            {
                return -1;
            }

            //如果一次性可复制完的,则直接调用Copy复制
            if (moveCountPerOne >= moveCount)
            {
                lock (array)
                {
                    Array.Copy(array,sourceIndex,array,desIndex,moveCountPerOne);
                }

                return desIndex;
            }

            var needCopyCount = moveCount;  //总共需要复制的数据量
            var tempsrcIndex = sourceIndex;   //临时的起始位置指针
            var tempdesIndex = desIndex;        //临时的结束位置指针
            var posDirection = 1;                       //指针的移动方向,1为向前,-1为向后

            if (sourceIndex < desIndex)   //如果sourceIndex比desIndex小.则从后向前复制
            {
                tempsrcIndex = sourceIndex + moveCount - moveCountPerOne;   
                tempdesIndex = desIndex + moveCount - moveCountPerOne;
                posDirection = -1;  //指针向后移动
            }

            lock (array)
            {
                while (needCopyCount > 0)
                {
                    Array.Copy(array, tempsrcIndex, array, tempdesIndex, moveCountPerOne);

                    needCopyCount -= moveCountPerOne;
                    tempsrcIndex += (posDirection * moveCountPerOne);
                    tempdesIndex += (posDirection * moveCountPerOne);
                }
            }

            return desIndex;
        }
    }

    public static class ArraySegmentExtMethod
    {
        /// <summary>
        ///     数组片段是否为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool HasData<T>(this ArraySegment<T> data)
        {
            if (data.Array==null)
            {
                return false;
            }

            if (data.Array.IsInEnableRange(data.Offset,data.Count))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}