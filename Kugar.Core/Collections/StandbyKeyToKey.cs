using System.Collections;
using System.Collections.Generic;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     用于在关联数据的时候,当出现原有版本叫法与现有控件关联字段名称不同时,可用该类进行转换
    /// </summary>
    public class StandbyKeyToKey<T> : Dictionary<T, StandbyList<T>>
    {
        public T GetMatchKey(T key, IEnumerable<T> lst)
        {
            return GetMatchKey(key, lst, key);
        }

        /// <summary>
        ///     在关联列表中,搜索已经设定的备用列表
        /// </summary>
        /// <param name="key">要读取的key名</param>
        /// <param name="lst">当前版本中,所有key的列表</param>
        /// <returns>返回现有版本中,对应设key值</returns>
        public T GetMatchKey(T key, IEnumerable<T> lst,T defaultvalue)
        {
            if (base.Count <= 0 || !base.ContainsKey(key))
            {
                return defaultvalue;
            }

            var temp = base[key];

            return temp.GetMatchKey(lst,defaultvalue);
        }


    }

    /// <summary>
    ///     备用key列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StandbyList<T> : ReadOnlyCollectionBase
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="key">指定的备选key的列表</param>
        public StandbyList(params T[] key)
            : base()
        {
            if (key != null && key.Length > 0)
            {
                base.InnerList.AddRange(key);
            }
        }

        /// <summary>
        ///     判断是否包含指定的key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainKey(T key)
        {
            if (base.InnerList.Count <= 0)
            {
                return false;
            }

            lock (base.InnerList)
            {
                for (int i = 0; i < base.InnerList.Count; i++)
                {
                    if (base.InnerList[i].Equals(key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     判断在指定keyList列表中,是否有与当前类中存放的备用key相同的值
        /// </summary>
        /// <param name="keyList">输入列表</param>
        /// <returns>返回匹配结果,如果不存在,则返回default(T)的值</returns>
        public T GetMatchKey(IEnumerable<T> keyList)
        {
            return GetMatchKey(keyList, default(T));
        }

        /// <summary>
        ///     判断在指定keyList列表中,是否有与当前类中存放的备用key相同的值
        /// </summary>
        /// <param name="keyList">输入列表</param>
        /// <param name="defaultValue">不存在时返回的默认值</param>
        /// <returns>返回匹配结果</returns>
        public T GetMatchKey(IEnumerable<T> keyList, T defaultValue)
        {

            if (keyList == null)
            {
                return defaultValue;
            }

            lock (keyList)
            {
                foreach (var v in keyList)
                {
                    if (this.ContainKey(v))
                    {
                        return v;
                    }
                }
            }

            return defaultValue;
        }
    }
}
