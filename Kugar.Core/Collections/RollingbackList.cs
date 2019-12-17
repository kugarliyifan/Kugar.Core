using System;
using System.ComponentModel;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     一个类似于缓冲区的列表
    ///     功能是：预先指定列表大小，使用Add函数插入数据，在数据的数量满了之后，会将最前的数据删除
    ///     适用：绘制实时曲线的时候，数据点的数据存储
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RollingbackList<T> : BindingList<T>
    {
        private int _maxCount = 0;

        public RollingbackList(int maxCount)
        {
            if (maxCount <= 1)
            {
                throw new ArgumentOutOfRangeException(@"maxCount",@"最大容量不能小于或等于1");
            }

            _maxCount = maxCount;
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (base.Count > _maxCount)
                {
                    lock (this)
                    {
                        var temp = base[0];

                        base.RemoveAt(0);

                        if (temp is IDisposable)
                        {
                            (temp as IDisposable).Dispose();
                        }
                    }
                }
            }

            base.OnListChanged(e);
        }

        public int MaxLength { get { return _maxCount; } }

        public T FirstNode
        {
            get
            {
                return base[0];
            }
        }

        public T LastNode
        {
            get
            {
                return base[base.Count - 1];
            }
        }

        public T[] ToArray()
        {
            var lst = new T[this.Count];

            lock (this)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    lst[i] = this[i];
                }
            }


            return lst;
        }

    }
}
