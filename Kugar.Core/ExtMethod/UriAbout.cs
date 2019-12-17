using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Kugar.Core.ExtMethod
{
    public static class UriAbout
    {
        public static IPAddress GetHostIPAddress(this Uri srcUri)
        {
            if (srcUri == null)
            {
                return null;
            }
            else
            {
                var remoteUri = srcUri.Host;

                if (srcUri.IsLoopback)
                {
                    return IPAddress.Parse("127.0.0.1");
                }
                

                //if (srcUri.Port>0)
                //{
                //    remoteUri += (":" + srcUri.Port);
                //}

                var ip = Dns.GetHostAddresses(remoteUri);

                return ip == null || ip.Length <= 0 ? null : ip[0];
            }
        }

        public static Uri ToUri(this string srcUriStr)
        {
            return ToUri(srcUriStr, UriKind.RelativeOrAbsolute, null);
        }

        public static Uri ToUri(this string srcUriStr, Uri defaultValue)
        {
            return ToUri(srcUriStr, UriKind.RelativeOrAbsolute, defaultValue);
        }

        public static Uri ToUri(this string srcUriStr, UriKind uriKind)
        {
            return ToUri(srcUriStr, uriKind, null);
        }

        public static Uri ToUri(this string srcUriStr, UriKind uriKind, Uri defaultValue)
        {
            if (srcUriStr.IsNullOrEmpty())
            {
                return defaultValue;
            }

            Uri remoteUri;

            if (Uri.TryCreate(srcUriStr, uriKind, out remoteUri))
            {
                return remoteUri;
            }
            else
            {
                return defaultValue;
            }
        }

        public static Dictionary<string, string> GetQueryInfo(this Uri srcUri)
        {
            if (srcUri == null)
            {
                return null;
            }

            var tempDic = new Dictionary<string, string>(3, StringComparer.CurrentCultureIgnoreCase);

            if (srcUri.Query.IsNullOrEmpty())
            {
                return tempDic;
            }

            var queryString = srcUri.Query.TrimStart('?');

            var item = queryString.Split('&');

            foreach (var s in item)
            {
                var slist = s.Split('=');
                if (slist.Length == 2)
                {
                    if (slist[1].IsNullOrEmpty())
                    {
                        continue;
                    }

                    var v = Uri.EscapeUriString(slist[1]);

                    //var v = HttpHelper.Decode(slist[1], Encoding.UTF8);
                    if (tempDic.ContainsKey(slist[0]))
                    {
                        tempDic[slist[0]] = v;
                    }
                    else
                    {
                        tempDic.Add(slist[0], v);
                    }

                }
                else
                {
                    tempDic.Add(s, string.Empty);
                }
            }

            return tempDic;
        }

        public static string GetTopLevelDomain(this Uri domain)
        {
            string str = domain.Host;
            if (str.IndexOf(".", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                string[] strArr = str.Split(':')[0].Split('.');
                if (strArr[strArr.Length - 1].IsNumeric())
                {
                    return str;
                }
                else
                {
                    string domainRules = "|com.cn|net.cn|org.cn|gov.cn|com.hk|公司|中国|网络|com|net|org|int|edu|gov|mil|arpa|Asia|biz|info|name|pro|coop|aero|museum|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cf|cg|ch|ci|ck|cl|cm|cn|co|cq|cr|cu|cv|cx|cy|cz|de|dj|dk|dm|do|dz|ec|ee|eg|eh|es|et|ev|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gh|gi|gl|gm|gn|gp|gr|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|in|io|iq|ir|is|it|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|ml|mm|mn|mo|mp|mq|mr|ms|mt|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nt|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|pt|pw|py|qa|re|ro|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sk|sl|sm|sn|so|sr|st|su|sy|sz|tc|td|tf|tg|th|tj|tk|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|va|vc|ve|vg|vn|vu|wf|ws|ye|yu|za|zm|zr|zw|";
                    string tempDomain;
                    if (strArr.Length >= 4)
                    {
                        tempDomain = strArr[strArr.Length - 3] + "." + strArr[strArr.Length - 2] + "." + strArr[strArr.Length - 1];
                        if (domainRules.IndexOf("|" + tempDomain + "|", StringComparison.CurrentCultureIgnoreCase) > 0)
                        {
                            return strArr[strArr.Length - 4] + "." + tempDomain;
                        }
                    }
                    if (strArr.Length >= 3)
                    {
                        tempDomain = strArr[strArr.Length - 2] + "." + strArr[strArr.Length - 1];
                        if (domainRules.IndexOf("|" + tempDomain + "|", StringComparison.CurrentCultureIgnoreCase) > 0)
                        {
                            return strArr[strArr.Length - 3] + "." + tempDomain;
                        }
                    }
                    if (strArr.Length >= 2)
                    {
                        tempDomain = strArr[strArr.Length - 1];
                        if (domainRules.IndexOf("|" + tempDomain + "|", StringComparison.CurrentCultureIgnoreCase) > 0)
                        {
                            return strArr[strArr.Length - 2] + "." + tempDomain;
                        }
                    }
                }
            }
            return str;
        }


        private static long aBegin =10;// (IPAddress.Parse("10.0.0.0")).Address;
        private static long aEnd =4294967050;// (IPAddress.Parse("10.255.255.255")).Address;
        private static long bBegin =4268;// (IPAddress.Parse("172.16.0.0")).Address;
        private static long bEnd =4294909868;// (IPAddress.Parse("172.31.255.255")).Address;
        private static long cBegin =43200;// (IPAddress.Parse("192.168.0.0")).Address;
        private static long cEnd =4294944960;// (IPAddress.Parse("192.168.255.255")).Address;

        public static bool IsLocalNetworkAddress(this string ipAddress)
        {
            IPAddress ipad;

            if (IPAddress.TryParse(ipAddress, out ipad))
            {
                return IsLocalNetworkAddress(ipad);
            }
            else
            {
                return false;
            }
        }

        public static bool IsLocalNetworkAddress(this IPAddress srcAddress)
        {
            var ad = srcAddress.Address;

            if (IPAddress.Loopback.Equals(srcAddress) || ad.IsBetween(aBegin, aEnd) || ad.IsBetween(bBegin, bEnd) || ad.IsBetween(cBegin, cEnd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var ub = new UriBuilder(uri);
            
            // decodes urlencoded pairs from uri.Query to HttpValueCollection
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            if (httpValueCollection.AllKeys.Contains(name))
            {
                httpValueCollection[name] = value;
            }
            else
            {
                httpValueCollection.Add(name, value);
            }

            

            // this code block is taken from httpValueCollection.ToString() method
            // and modified so it encodes strings with HttpUtility.UrlEncode
            if (httpValueCollection.Count == 0)
                ub.Query = String.Empty;
            else
            {
                var sb = new StringBuilder();

                for (int i = 0; i < httpValueCollection.Count; i++)
                {
                    string text = httpValueCollection.GetKey(i);
                    {
                        text = HttpUtility.UrlEncode(text);

                        string val = (text != null) ? (text + "=") : string.Empty;
                        string[] vals = httpValueCollection.GetValues(i);

                        if (sb.Length > 0)
                            sb.Append('&');

                        if (vals == null || vals.Length == 0)
                            sb.Append(val);
                        else
                        {
                            if (vals.Length == 1)
                            {
                                sb.Append(val);
                                sb.Append(HttpUtility.UrlEncode(vals[0]));
                            }
                            else
                            {
                                for (int j = 0; j < vals.Length; j++)
                                {
                                    if (j > 0)
                                        sb.Append('&');

                                    sb.Append(val);
                                    sb.Append(HttpUtility.UrlEncode(vals[j]));
                                }
                            }
                        }
                    }
                }

                ub.Query = sb.ToString();
            }

            return ub.Uri;
        }

        /// <summary>
        /// 删除指定值
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Uri RemoveQuery(this Uri uri, string name)
        {
            var ub = new UriBuilder(uri);

            // decodes urlencoded pairs from uri.Query to HttpValueCollection
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);

            // this code block is taken from httpValueCollection.ToString() method
            // and modified so it encodes strings with HttpUtility.UrlEncode
            if (httpValueCollection.Count == 0)
                ub.Query = String.Empty;
            else
            {
                var sb = new StringBuilder();

                for (int i = 0; i < httpValueCollection.Count; i++)
                {
                    string text = httpValueCollection.GetKey(i);
                    {
                        text = HttpUtility.UrlEncode(text);

                        string val = (text != null) ? (text + "=") : string.Empty;
                        string[] vals = httpValueCollection.GetValues(i);

                        if (sb.Length > 0)
                            sb.Append('&');

                        if (vals == null || vals.Length == 0)
                            sb.Append(val);
                        else
                        {
                            if (vals.Length == 1)
                            {
                                sb.Append(val);
                                sb.Append(HttpUtility.UrlEncode(vals[0]));
                            }
                            else
                            {
                                for (int j = 0; j < vals.Length; j++)
                                {
                                    if (j > 0)
                                        sb.Append('&');

                                    sb.Append(val);
                                    sb.Append(HttpUtility.UrlEncode(vals[j]));
                                }
                            }
                        }
                    }
                }

                ub.Query = sb.ToString();
            }

            return ub.Uri;
        }
    }

}