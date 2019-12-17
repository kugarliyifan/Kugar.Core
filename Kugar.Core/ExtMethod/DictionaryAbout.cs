using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kugar.Core.ExtMethod
{

    public static class ConcurrentDictionaryExtMethod
    {
        public static bool TryRemove<TKey, TValue>(this System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> src, TKey key)
        {
            TValue old;

            return src.TryRemove(key, out old);

        }
    }

    public static class DictionaryAbout
    {
        //public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> i, TKey key)
        //{
        //    return TryGetValue(i, key, default(TValue));
        //}

        //public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> i, TKey key, TValue defalutvalue)
        //{
        //    TValue tv;

        //    if (i.TryGetValue(key, out tv))
        //    {
        //        return tv;
        //    }

        //    return defalutvalue;
        //}

        /// <summary>
        ///     添加或修改指定key对应的值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }

            return dic;
        }



        //IDictionary接口
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> i, TKey key)
        {
            return TryGetValue(i, key, default(TValue));
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> i, TKey key, TValue defalutvalue)
        {
            if (key == null)
            {
                return defalutvalue;
            }

            TValue tv;

            if (i.TryGetValue(key, out tv))
            {
                return tv;
            }

            return defalutvalue;
        }

        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dic,TKey key,TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key,value);
            }

            return dic;
        }


        /// <summary>
        ///     字典转存为字符串,关键字与值之间使用 = 号相连,项目之间使用 '|' 号相连
        /// </summary>
        /// <param name="src">原字典</param>
        /// <returns></returns>
        public static string ToStringEx<TKey, TValue>(this IDictionary<TKey, TValue> src)
        {
            return ToStringEx(src, '=', '|');
        }

        public static string ToStringEx<TKey, TValue>(this IDictionary<TKey, TValue> src, char linesplite)
        {
            return ToStringEx(src, '=', linesplite);
        }

        public static string ToStringEx<TKey, TValue>(this IDictionary<TKey, TValue> src, char valuesplite, char linesplite)
        {
            if (src == null || src.Count <= 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(256);

            foreach (var o in src)
            {
                sb.Append(o.Key.ToString() + valuesplite + o.Value.ToString() + linesplite);
            }

            return sb.ToString();

        }


        public static string ToStringEx<TKey, TValue>(this Dictionary<TKey, TValue> src)
        {
            return ToStringEx(src, '=', '|');
        }

        public static string ToStringEx<TKey, TValue>(this Dictionary<TKey, TValue> src, char linesplite)
        {
            return ToStringEx(src, '=', linesplite);
        }

        public static string ToStringEx<TKey, TValue>(this Dictionary<TKey, TValue> src, char valuesplite, char linesplite)
        {
            if (src == null || src.Count <= 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(256);

            foreach (var o in src)
            {
                sb.Append(o.Key.ToString() + valuesplite + o.Value.ToString() + linesplite);
            }

            return sb.ToString();

        }


        //字典粘贴
        public static IDictionary<string, string> ToDictionary(this string src)
        {
            return ToDictionary(src, '=', '|', null);
        }

        public static IDictionary<string, string> ToDictionary(this string src, IDictionary<string, string> defaultvalue)
        {
            return ToDictionary(src, '=', '|', defaultvalue);
        }

        public static IDictionary<string, string> ToDictionary(this string src, char linesplite, IDictionary<string, string> defaultvalue)
        {
            return ToDictionary(src, '=', linesplite, defaultvalue);
        }

        /// <summary>
        ///     字符串转字典
        /// </summary>
        /// <param name="srcstr">原字符串</param>
        /// <param name="linesplite">不同项目之间的间隔字符</param>
        /// <param name="valuesplite">关键字与值之间的间隔</param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this string srcstr, char valuesplite, char linesplite, IDictionary<string, string> defaultvalue)
        {
            if (string.IsNullOrEmpty(srcstr))
            {
                return defaultvalue;
            }

            var strary = srcstr.Split(linesplite);

            if (strary.Length <= 0)
            {
                return defaultvalue;
            }


            var temp = new Dictionary<string, string>(10);


            foreach (var s in strary)
            {
                try
                {
                    var sindex = s.IndexOf(valuesplite);

                    var name = s.Substring(0, sindex);
                    var value = s.Substring(sindex + 1);

                    
                    temp.Add(name, value);
                    
                }
                catch (Exception)
                {

                    continue;
                }

            }

            return temp;

        }

        public static IDictionary<TKey, TValue> AddRange<T, TKey, TValue>(this IDictionary<TKey, TValue> descDic,
                                                                        IEnumerable<T> src, 
                                                                        Func<T, TKey> keyFunc, 
                                                                        Func<T, TValue> valueFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException("valueFunc");
            }

            var tempDic = new Dictionary<TKey, TValue>();

            foreach (var item in src)
            {
                tempDic.Add(keyFunc(item), valueFunc(item));
            }

            return tempDic;
        }

        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> descDic,
                                                                        IEnumerable<TValue> src,
                                                                        Func<TValue, TKey> keyFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            var tempDic = new Dictionary<TKey, TValue>();

            foreach (var item in src)
            {
                tempDic.Add(keyFunc(item),item);
            }

            return tempDic;
        }


        public static IDictionary<TKey, TValue> AddOrUpdateRange<T, TKey, TValue>(this IDictionary<TKey, TValue> descDic,
                                                                        IEnumerable<T> src,
                                                                        Func<T, TKey> keyFunc,
                                                                        Func<T, TValue> valueFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException("valueFunc");
            }

            var tempDic = new Dictionary<TKey, TValue>();

            foreach (var item in src)
            {
                tempDic.AddOrUpdate(keyFunc(item), valueFunc(item));
            }

            return tempDic;
        }

        public static IDictionary<TKey, TValue> AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> descDic,
                                                                IEnumerable<TValue> src,
                                                                Func<TValue, TKey> keyFunc)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            if (keyFunc == null)
            {
                throw new ArgumentNullException("keyFunc");
            }

            var tempDic = new Dictionary<TKey, TValue>();

            foreach (var item in src)
            {
                tempDic.AddOrUpdate(keyFunc(item),item);
            }

            return tempDic;
        }

        public static bool UpdateValue<TKey,TValue>(this IDictionary<TKey,TValue> srcDic,TKey key,TValue value)
        {
            Monitor.Enter(srcDic);

            if (srcDic.ContainsKey(key))
            {

                srcDic[key] = value;
                Monitor.Exit(srcDic);
                return true;
            }
            else
            {
                Monitor.Exit(srcDic);
                return false;
            }
        }

        public static bool UpdateValue<TKey, TValue>(this IDictionary<TKey, TValue> srcDic, TKey key, Action<TValue> updateAction)
        {
            Monitor.Enter(srcDic);

            if (srcDic.ContainsKey(key))
            {
                var v = srcDic[key];

                updateAction(v);

                //srcDic[key] = value;
                Monitor.Exit(srcDic);
                return true;
            }
            else
            {
                Monitor.Exit(srcDic);
                return false;
            }
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> Remove<TKey, TValue>(this IDictionary<TKey, TValue> srcDic, Predicate<KeyValuePair<TKey, TValue>> checker)
        {
            if (srcDic==null)
            {
                throw new ArgumentNullException("srcDic");
            }

            if (checker==null)
            {
                throw new ArgumentNullException("checker");
            }

            if (srcDic.Count<=0)
            {
                return null;
            }

            List<KeyValuePair<TKey, TValue>> removingLst = new List<KeyValuePair<TKey, TValue>>((srcDic.Count/2).ToBetween(1,10));

            foreach (var pair in srcDic)
            {
                if(checker(pair))
                {
                   removingLst.Add(pair); 
                }
            }

            if (removingLst.Count>0)
            {
                foreach (var key in removingLst)
                {
                    srcDic.Remove(key);
                }
            }

            return removingLst;

        }
    }

}
