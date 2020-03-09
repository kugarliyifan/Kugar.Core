using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
namespace Kugar.Core.Network
{
    public class UrlModifier
    {
        private string _anchor = string.Empty;
        private string _hostAndPath = string.Empty;
        private List<(string key, string value)> _queryKeys = new List<(string key, string value)>(5);


        public UrlModifier(string url) //直接传入string,不用uri,省的相对路径下,Uri类在读取Query属性的之后报错
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            var endIndex = url.Length - 1;

            //从后往前扫描锚点#
            for (int i = url.Length - 1; i >= 0; i--)
            {
                if (url[i] == '#')
                {
                    _anchor = url.Substring(i + 1);
                    endIndex = i - 1;
                    break;
                }

                //防止出现无#字符的情况
                if (url[i] == '=' || url[i] == '&' || url[i] == ' ' || url[i] == '?' || url[i] == '/')
                {
                    endIndex = url.Length - 1;
                    break;
                }
            }

            //截取域名段,包含协议,端口号等
            var hostEndIndex = endIndex;
            for (int i = 0; i < endIndex; i++)
            {
                if (url[i] == '?')
                {
                    hostEndIndex = i - 1;
                }
            }

            if (hostEndIndex > 0)
            {
                if (url[hostEndIndex]=='?')  //处理 http://xxxx.com/ss/ss? 的情况
                {
                    _hostAndPath = url.Substring(0, hostEndIndex);
                }
                else
                {
                    _hostAndPath = url.Substring(0, hostEndIndex + 1);
                }
                
                hostEndIndex++;
            }

            if (hostEndIndex > 0 && hostEndIndex < endIndex)
            {
                //排除掉使用=号或者&或者空格结尾的字符,减少后续判断的麻烦,计算出实际的结束index
                for (int i = endIndex; i >= hostEndIndex; i--)
                {
                    var c = url[i];
                    if (c != '=' && c != '&' && c != ' ')
                    {
                        endIndex = i;
                        break;
                    }
                }

                var keyword = "";
                var value = "";
                var startIndex = 1;
                char lastKeyword = '\0';

                for (int i = hostEndIndex; i < endIndex; i++)
                {
                    var c = url[i];
                    //排除掉使用 = 号或者 & 或者 ? 或者空格开头的字符,减少后续判断的麻烦,计算出实际起始index
                    if (c != '=' && c != '&' && c != ' ' && c != '?')
                    {
                        startIndex = i;
                        break;
                    }
                }

                if (startIndex >= endIndex) //如果没字符了,整个都是特殊字符,则直接返回
                {
                    return;
                }

                for (int i = startIndex; i <= endIndex; i++)
                {
                    var c = url[i];

                    if (c == '=')
                    {
                        if (lastKeyword == '=') //处理 ?s==0 
                        {
                            startIndex = i + 1;
                            continue;
                        }

                        keyword = (string)url.Substring(startIndex, i - startIndex);
                        lastKeyword = c;
                        startIndex = i + 1;
                        //startIndex++;
                    }

                    if (c == '&')
                    {
                        if (url[i - 1] == '&') //处理 ?s=0&& 的情况
                        {
                            startIndex = i + 1;
                            continue;
                        }

                        if (lastKeyword == '=' || lastKeyword == '\0') // 处理 ?ss=0 的情况
                        {
                            value = url.Substring(startIndex, i - startIndex);
                        }
                        // 处理 ?ddd& 或者 ?p=0&ddd 这种情况,切分出来的,算key
                        else if (lastKeyword == '&' || lastKeyword == '\0')
                        {
                            keyword = url.Substring(startIndex, i - startIndex);
                            value = string.Empty;
                        }

                        lastKeyword = '\0';
                        startIndex = i + 1;

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            AddQuery(keyword, value); //添加入列表
                        }

                    }
                }

                //如果还有剩余的字符,则处理完剩下的字符串
                if (startIndex <= endIndex)
                {
                    if (lastKeyword == '=' && !string.IsNullOrEmpty(keyword)) //处理 ?d=value 的情况
                    {
                        value = url.Substring(startIndex, endIndex - startIndex);
                    }
                    else if ((lastKeyword == '=' && string.IsNullOrEmpty(keyword)) ||
                             lastKeyword == '&' ||
                             lastKeyword == '\0')
                    {
                        keyword = url.Substring(startIndex, endIndex + 1 - startIndex);
                        value = string.Empty;
                    }

                    AddQuery(keyword, value);
                }
            }

        }

        /// <summary>
        /// 添加一对参数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlModifier AddQuery(string key, string value)
        {
            //如果存在相同的Key的,则直接附加到原有值的后面
            var index = _queryKeys.IndexOf(x => x.key.CompareTo(key, true));

            if (index > 0)
            {
                var orgValue = _queryKeys[index].value;
                _queryKeys[index] = (key, $"{orgValue},{value}");
            }
            else
            {
                _queryKeys.Add((key, value));
            }

            return this;
        }

        /// <summary>
        /// 添加一对参数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlModifier AddQuery(string key, object value)
        {
            //如果存在相同的Key的,则直接附加到原有值的后面
            var index = _queryKeys.IndexOf(x => x.key.CompareTo(key, true));

            if (index > 0)
            {
                var orgValue = _queryKeys[index].value;
                _queryKeys[index] = (key, $"{orgValue},{value}");
            }
            else
            {
                _queryKeys.Add((key, value.ToStringEx()));
            }

            return this;
        }

        /// <summary>
        /// 添加一个keyvalue
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public UrlModifier AddQuery((string key, string value) pair)
        {
            return AddQuery(pair.key, pair.value);
        }

        /// <summary>
        /// 删除指定key的项目
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public UrlModifier RemoveQuery(params string[] keys)
        {
            if (keys.HasData())
            {
                _queryKeys.Remove(x => keys.Contains(x.key, StringComparer.InvariantCultureIgnoreCase));
            }

            return this;
        }

        /// <summary>
        /// 删除指定key的项目中的某个值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public UrlModifier RemoveQuery(string key,string value)
        {
            var index = _queryKeys.IndexOf(x => x.key.CompareTo(key, true));

            if (index > 0)
            {
                var orgValue = _queryKeys[index].value;

                var newValues = orgValue.Split(',').Remove(x => x.CompareTo(value, true));

                _queryKeys[index] = (key, newValues.JoinToString(','));
            }
            

            return this;
        }

        /// <summary>
        /// 替换指定key的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlModifier ReplaceQuery(string key, string value)
        {
            var index = _queryKeys.IndexOf(x => x.key == key);

            if (index < 0)
            {
                _queryKeys.Add((key, value));
            }
            else
            {
                _queryKeys[index] = (key, value);
            }

            return this;
        }

        /// <summary>
        /// 替换指定key的数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlModifier ReplaceQuery(string key, object value)
        {
            var index = _queryKeys.IndexOf(x => x.key == key);

            if (index < 0)
            {
                _queryKeys.Add((key, value.ToString()));
            }
            else
            {
                _queryKeys[index] = (key, value.ToString());
            }

            return this;
        }

        /// <summary>
        /// 设置锚点值
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public UrlModifier SetAnchor(string anchor)
        {
            _anchor = anchor;
            return this;
        }

    public UrlModifier Clone()
    {
        var item=new UrlModifier("") ;

        item._queryKeys.AddRange(this._queryKeys);
        item._hostAndPath = this._hostAndPath;
        item._anchor = this._anchor;

        return item;
    }

        public override string ToString()
        {
            var sb = new StringBuilder(60);

            if (!string.IsNullOrEmpty(_hostAndPath))
            {
                sb.Append(_hostAndPath);
            }

            if (_queryKeys != null && _queryKeys.Count > 0)
            {
                sb.Append('?');

                foreach (var item in _queryKeys)
                {
                    sb.Append(item.key);

                    if (!string.IsNullOrEmpty(item.value))
                    {
                        sb.Append('=');
                        sb.Append(item.value);

                    }
                    sb.Append('&');
                }

                if (sb[sb.Length - 1] == '&')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

            }

            if (!string.IsNullOrEmpty(_anchor))
            {
                sb.Append('#');
                sb.Append(_anchor);
            }

            return sb.ToString();
        }

        public static implicit operator string(UrlModifier modifier)
        {
            return modifier.ToStringEx();
        }

        public static implicit operator UrlModifier(string srcStr)
        {
            return new UrlModifier(srcStr);
        }

        public static implicit operator Uri(UrlModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            var result = modifier.ToStringEx();

            if (result.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase) ||
                result.StartsWith("https:", StringComparison.InvariantCultureIgnoreCase) ||
                result.StartsWith("ftp:", StringComparison.InvariantCultureIgnoreCase) ||
                result.StartsWith("file:", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                return new Uri(result, UriKind.Absolute);
            }
            else
            {
                return new Uri(result, UriKind.Relative);
            }
        }

        public static implicit operator UrlModifier(Uri uri)
        {
            if (uri == null)
            {
                return new UrlModifier(string.Empty);
            }

            return new UrlModifier(uri.ToStringEx());
        }
    }

    public static class UrlModifierExt
    {
        public static UrlModifier ToUrlModifier(this string url)
        {
            return new UrlModifier(url);
        }

        public static UrlModifier ToUrlModifier(this Uri uri)
        {
            return new UrlModifier(uri.ToStringEx());
        }
    }
}
