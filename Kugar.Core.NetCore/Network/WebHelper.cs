using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Kugar.Core.Network
{
    public static class WebHelper
    {
        private static string[] _normalUA = new string[]
                                            {
                                                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36",
                                                "Mozilla/5.0 (compatible; WOW64; MSIE 10.0; Windows NT 6.2)",
                                                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
                                                "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)",
                                                "Mozilla/5.0 (Linux; Android 4.1.2; Nexus 7 Build/JZ054K) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166 Safari/535.19",
                                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0"
                                            };

        static WebHelper()
        {

            DefaultUserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
            DefaultAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8,image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/vnd.ms-excel, application/msword, */*";
        }

        public static HttpWebGetter Create(string url)
        {
            return new HttpWebGetter(url);
        }

        public static WebDownloadFileInfo GetFile(string url)
        {
            return GetFile(new Uri(url));
        }

        public static WebDownloadFileInfo GetFile(Uri url)
        {
            var client=new HttpClient();
            
            var resp=client.GetAsync(url).Result;

            var contentType = resp.Headers.GetValues("Content-Type").FirstOrDefault();

            var item = new WebDownloadFileInfo();

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                var extLst = contentType.Split('/');
                if (extLst.Length == 2)
                {
                    item.FileExt = extLst[1];
                }
            }

            var data = resp.Content.ReadAsByteArrayAsync().Result;

            if (data.Length > 0)
            {
                item.FileSize = (int)data.Length;
            }


            return item;
        }

        public static string DefaultUserAgent { set; get; } = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.1453.94 Safari/537.36";

        public static string DefaultAccept { set; get; }

        public class DefaultHttpClientHandler : HttpClientHandler
        {
            public DefaultHttpClientHandler() => this.ServerCertificateCustomValidationCallback+=delegate { return true; };
        }

        /// <summary>
        /// 用于获取HttpWeb的数据的包装类
        /// </summary>
        public class HttpWebGetter
        {
            //private Lazy<Dictionary<string,string>> _headers=new Lazy<Dictionary<string, string>>(()=>new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase));
            //private HttpClientHandler _handler = new HttpClientHandler();
            //private HttpClient _client = null;
            private HttpRequestMessage _requestMsg = null;
            private JObject _jsonContent = null;
            private string _stringContent = null;
            private Encoding _encoding = System.Text.Encoding.UTF8;
            private Dictionary<string, object> _values = null;
            private ContentTypeEnum _contentType = ContentTypeEnum.FormUrlencoded;
            private static IHttpClientFactory _httpClientFactory=null;
            private bool _hasChangeContentType = false;
            private Lazy<Dictionary<string, string>>_cookies =new Lazy<Dictionary<string, string>>();
            

            private static ServiceProvider _service = null;
            //private static HttpClient

            #region 固定字段


            private static string[] _normalUA = new string[]
            {
            "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36",
            "Mozilla/5.0 (compatible; WOW64; MSIE 10.0; Windows NT 6.2)",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)",
            "Mozilla/5.0 (Linux; Android 4.1.2; Nexus 7 Build/JZ054K) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166 Safari/535.19",
            "	Mozilla/5.0 (Windows NT 6.2; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0"
            };

            private static string _defaultUA =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.1453.94 Safari/537.36";



            #endregion



            static HttpWebGetter()
            {
                DefaultUserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
                DefaultAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8,image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/vnd.ms-excel, application/msword, */*";

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddHttpClient("defaulthttpgetter")
                    .ConfigurePrimaryHttpMessageHandler(x => new DefaultHttpClientHandler());

                _service =serviceCollection.BuildServiceProvider();
                _httpClientFactory =  _service.GetRequiredService<IHttpClientFactory>();
             
            }

            public static string DefaultAccept { get; set; }

            public static string DefaultUserAgent { get; set; }

            public HttpWebGetter(string url)
            {


                //var request = (HttpWebRequest)HttpWebRequest.Create(url);
                //request.Method = "get";
                //request.Credentials = CredentialCache.DefaultCredentials;
                //request.Accept = DefaultAccept;
                //request.UserAgent = DefaultUserAgent;
                //request.ContentType = "application/x-www-form-urlencoded";
                ////request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
                //request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.3");
                //request.Headers.Add("Accept-Encoding", "gzip,deflate");
                //request.Timeout = timeout;
                //request.ReadWriteTimeout = timeout;
                _requestMsg=new HttpRequestMessage(HttpMethod.Get, url);
                _requestMsg.Method=HttpMethod.Get;
                _requestMsg.Headers.AcceptCharset.ParseAdd("utf-8;q=0.7,*;q=0.3");
                _requestMsg.Headers.UserAgent.ParseAdd(DefaultUserAgent);
                _requestMsg.Headers.AcceptEncoding.ParseAdd("gzip,deflate");
                _requestMsg.Headers.Accept.ParseAdd("*/*,text/html,application/xhtml+xml,application/xml,application/json ;q=0.9, */*;q=0.8");
                
                //_handler.AllowAutoRedirect = true;
                //_handler.Credentials = CredentialCache.DefaultCredentials;
                
            }

            public HttpWebGetter SetCookie(string name, string value)
            {
                //_request.CookieContainer = _request.CookieContainer ?? new CookieContainer();

                _cookies.Value.AddOrUpdate(name, value);

                //_request.CookieContainer.Add(new Cookie(name,value, path,domain));

                return this;
            }

            public HttpWebGetter SetCookie(Cookie cookie)
            {
                //_request.CookieContainer = _request.CookieContainer ?? new CookieContainer();

                //_request.CookieContainer.Add(cookie);

                SetCookie(cookie.Name, cookie.Value);

                return this;
            }

            public HttpWebGetter SetCookie(IEnumerable<Cookie> cookies)
            {
                //_request.CookieContainer = _request.CookieContainer ?? new CookieContainer();

                foreach (var cookie in cookies)
                {
                    SetCookie(cookie.Name, cookie.Value);
                }

                return this;
            }

            /// <summary>
            /// 添加一个Request的Header信息
            /// </summary>
            /// <param name="headerName"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public HttpWebGetter AddHeader(string headerName, string value)
            {

                if (headerName.CompareTo("Accept", true))
                {
                    _requestMsg.Headers.Accept.ParseAdd(value);
                }
                else if (headerName.CompareTo("ContentType", true) || headerName.CompareTo("Content-Type", true))
                {
                    _requestMsg.Headers.Add("Content-Type",value);
                    //_request.ContentType = value;
                }
                else if (headerName.CompareTo("user-agent", true))
                {
                    _requestMsg.Headers.UserAgent.ParseAdd(value);
                    //_request.UserAgent = value;
                }
                else if (headerName.CompareTo("host", true))
                {
                    _requestMsg.Headers.Host = value;
                }
                else if (headerName.CompareTo("referer", true))
                {
                    _requestMsg.Headers.Referrer=new Uri(value);
                   // _request.Referer = value;
                }
                else
                {
                    _requestMsg.Headers.TryAddWithoutValidation(headerName, value);
                }

                return this;
            }

            /// <summary>
            /// 设置上传数据为json,并在提交时,自动提交该数据
            /// </summary>
            /// <param name="json"></param>
            /// <returns></returns>
            public HttpWebGetter SetJson(JObject json)
            {
                _jsonContent = json;
                _values = null;

                return this;
            }

            public HttpWebGetter SetParamters(Dictionary<string, object> values, bool autoUrlEncoding = true)
            {
                _values = values;
                _jsonContent = null;

                if (_values!=null && autoUrlEncoding)
                {
                    var keys = _values.Keys.ToArrayEx();
                    foreach (var key in keys)
                    {
                        var v = _values[key];

                        if (!(v is FormFile))
                        {
                            _values[key] = HttpUtility.UrlEncode(v.ToStringEx());
                        }
                    }
                }

                return this;
            }
            
            /// <summary>
            /// 设置一个指定名称和值的参数,使用该方式,则在提交数据的时候,会使用键值对的方式提交
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public HttpWebGetter SetParamter(string name, string value,bool autoUrlEncoding=false)
            {
                if (_values == null)
                {
                    _values = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
                }

                //if (autoEncode)
                //{
                //    value = HttpUtility.UrlEncode(value);
                //}

                _values.Add(name, autoUrlEncoding?HttpUtility.UrlEncode(value):value);

                _jsonContent = null;

                return this;
            }

            public HttpWebGetter SetParamter(string name, FormFile file)
            {
                if (_values == null)
                {
                    _values = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
                }

                _values.Add(name, file);

                _jsonContent = null;

                return this;
            }

            /// <summary>
            /// 直接设置字符串数据内容
            /// </summary>
            /// <param name="content"></param>
            /// <returns></returns>
            public HttpWebGetter SetContent(string content)
            {
                _values = null;
                _jsonContent = null;
                _stringContent = content;

                return this;
            }

            /// <summary>
            /// 设置头部授权信息,一般用于设置Bearer类型token授权
            /// </summary>
            /// <param name="scheme"></param>
            /// <param name="token"></param>
            /// <returns></returns>
            public HttpWebGetter SetAuthentication(string scheme, string token)
            {
                _requestMsg.Headers.Authorization=new AuthenticationHeaderValue(scheme,token);

                return this;
            }

            //public HttpWebGetter SetProxy(WebProxy proxy)
            //{
                
            //}

            /// <summary>
            /// 设置Encoding属性值
            /// </summary>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public HttpWebGetter Encoding(Encoding encoding)
            {
                _encoding = encoding;
                //_request.TransferEncoding = encoding.EncodingName;

                return this;
            }

            /// <summary>
            /// 数据接收的类型
            /// </summary>
            /// <param name="accept"></param>
            /// <returns></returns>
            public HttpWebGetter Accept(string accept)
            {
                _requestMsg.Headers.Accept.ParseAdd(accept);

                return this;
            }

            /// <summary>
            /// 设置UserAgent属性值
            /// </summary>
            /// <param name="userAgent"></param>
            /// <returns></returns>
            public HttpWebGetter UserAgent(string userAgent)
            {
                _requestMsg.Headers.UserAgent.ParseAdd(userAgent);

                return this;
            }

            //public HttpWebGetter ContentType(string contentType)
            //{
            //    _contentType = contentType;
            //    _hasChangeContentType = true;
            //    return this;
            //}

            /// <summary>
            /// 提交数据的内容类型
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public HttpWebGetter ContentType(ContentTypeEnum type)
            {
                //_contentType = getContentType(type);
                _contentType = type;
                //ContentType(getContentType(type));

                _hasChangeContentType = true;

                return this;
            }

            /// <summary>
            /// 使用Post提交数据,并将返回数据作为string返回
            /// </summary>
            /// <returns></returns>
            public async Task<string> Post_StringAsync(CancellationToken? cancellationToken=null)
            {
                _requestMsg.Method= HttpMethod.Post;

                var data = await getResponseData(cancellationToken);

                var content = System.Text.Encoding.UTF8.GetString(data);

                return content;
            }

            /// <summary>
            /// 使用Get提交数据,并将返回数据作为string返回
            /// </summary>
            /// <returns></returns>
            public async Task<string> Get_StringAsync(CancellationToken? cancellationToken = null)
            {
                _requestMsg.Method = HttpMethod.Get;

                var data =await getResponseData(cancellationToken);

                return System.Text.Encoding.UTF8.GetString(data);
            }
            
            /// <summary>
            /// 使用Post提交数据,并将数据作为Json格式返回
            /// </summary>
            /// <returns></returns>
            public async Task<JObject> Post_JsonAsync(CancellationToken? cancellationToken = null)
            {
                //_requestMsg.Headers.Add("Content-Type", getContentType(ContentTypeEnum.Json));


                var str =await  Post_StringAsync(cancellationToken);

                return JObject.Parse(str);
            }

            /// <summary>
            /// 使用Get提交数据,并将数据作为Json格式返回
            /// </summary>
            /// <returns></returns>
            public async Task<JObject> Get_JsonAsync(CancellationToken? cancellationToken = null)
            {
                //_requestMsg.Headers.Add("Content-Type", getContentType(ContentTypeEnum.Json));
                
                var str =await Get_StringAsync(cancellationToken);

                return JObject.Parse(str);
            }

            public async Task<byte[]> Get_BinaryAsync(CancellationToken? cancellationToken = null)
            {
                _requestMsg.Method=HttpMethod.Get;
                
                var resp = await getResponseData(cancellationToken);

                return resp;
            }
            
            public async Task<byte[]> Post_BinaryAsync(CancellationToken? cancellationToken = null)
            {
                _requestMsg.Method=HttpMethod.Post;

                var resp = await getResponseData(cancellationToken);

                return resp;
            }

            public async Task<XmlDocument> Get_XmlAsync(CancellationToken? cancellationToken = null)
            {
                var str = await Get_StringAsync(cancellationToken);

                var xml=new XmlDocument();
                
                xml.LoadXml(str);

                return xml;
            }

            public async Task<XmlDocument> Post_XmlAsync(CancellationToken? cancellationToken = null)
            {
                var str = await Post_StringAsync(cancellationToken);

                var xml = new XmlDocument();

                xml.LoadXml(str);

                return xml;
            }

            //public Stream Get_File()
            //{

            //}

            //public Stream Post_File()
            //{

            //}

            private string getContentType(ContentTypeEnum value)
            {
                var s = "";

                switch (value)
                {
                    case ContentTypeEnum.Text:
                        s = "text/plain";
                        break;
                    case ContentTypeEnum.Html:
                        s = "text/html";
                        break;
                    case ContentTypeEnum.Xml:
                        s = "text/xml";
                        break;
                    case ContentTypeEnum.Json:
                        s = "application/json";
                        break;
                    case ContentTypeEnum.Stream:
                        s = "application/octet-stream";
                        break;
                    case ContentTypeEnum.FormUrlencoded:
                        s = "application/x-www-form-urlencoded";
                        break;
                    case ContentTypeEnum.FormData:
                        s = "multipart/form-data";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                return s;
            }

            private async Task<byte[]> getResponseData(CancellationToken? cancellationToken)
            {
                if (cancellationToken==null)
                {
                    cancellationToken=new CancellationToken();
                }

                //var client = new HttpClient(_handler, true);
                var client = _httpClientFactory.CreateClient("defaulthttpgetter");
                client.BaseAddress = _requestMsg.RequestUri;
           
                var contentType = _contentType;

                if (_requestMsg.Method== HttpMethod.Post)
                {
                    if (_jsonContent != null)
                    {
                        if (!_hasChangeContentType)
                        {
                            contentType = ContentTypeEnum.Json;
                        }

                        var content = new StringContent(_jsonContent.ToStringEx(Formatting.None), _encoding, getContentType(contentType));

                        _requestMsg.Content = content;
                    }
                    else if (_values.HasData())
                    {
                        var hasFile = _values.Any(x => x.Value is FormFile);

                        if (!_hasChangeContentType)
                        {
                            if (hasFile)
                            {
                                contentType = ContentTypeEnum.FormData;
                            }
                            else
                            {
                                contentType = ContentTypeEnum.FormUrlencoded;
                            }
                        }

                        if (!hasFile && contentType!=ContentTypeEnum.FormData)
                        {
                            var content=new FormUrlEncodedContent(_values.Select(x=>new KeyValuePair<string, string>(x.Key,x.Value.ToStringEx())));

                            _requestMsg.Content = content;
                
                        }
                        else
                        {
                            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                            var content=new MultipartFormDataContent(boundary);

                            foreach (var value in _values)
                            {
                                if (value.Value is FormFile)
                                {
                                    var file = (FormFile) value.Value;

                                    if (!file.FileData.HasData())
                                    {
                                        continue;
                                    }
                            
                                    content.Add(new ByteArrayContent(file.FileData),value.Key,file.FilePath);
                                }
                                else
                                {
                                    content.Add(new StringContent((string)value.Value),value.Key);
                                }
                            }
                        }
                    }
                    else
                    {
                        var content = new StringContent(_stringContent, _encoding, getContentType(contentType));

                        _requestMsg.Content = content;
                    }                   
                }



                if (_cookies.IsValueCreated && _cookies.Value.HasData())
                {
                    var cookie = _cookies.Value.Select(x => $"{x.Key}={x.Value};").JoinToString(' ');

                    _requestMsg.Headers.Add("Cookie", cookie);
                }
                
                HttpResponseMessage resp = null;

                try
                {
                    
                    //if (cancellationToken.HasValue)
                    //{
                        resp = (await client.SendAsync(_requestMsg, cancellationToken.Value)).EnsureSuccessStatusCode();
                    //}

                    var data =await resp.Content.ReadAsByteArrayAsync();


                    if (resp.Content.Headers.ContentEncoding.Any(x=>x.Contains("gzip")))
                    {
                        using (GZipStream stream = new GZipStream(new ByteStream(data), CompressionMode.Decompress))
                        {
                            data = stream.ReadAllBytes();
                        }
                    }
                    else if (resp.Content.Headers.ContentEncoding.Any(x => x.Contains("deflate")))
                    {
                        using (DeflateStream stream = new DeflateStream(new ByteStream(data), CompressionMode.Decompress))
                        {
                            data = stream.ReadAllBytes();
                        }
                    }
              
                    return data;
                }
                catch (Exception e)
                {
                    var content = await resp.Content.ReadAsStringAsync();

                    LoggerManager.Default.Debug("读取错误:" + content);
                    
                    throw new HttpWebGetterException(_requestMsg.RequestUri.ToStringEx(),e,content);
                }
                
            }
            
            private static readonly Regex reg_charset = new Regex(@"charset\b\s*=\s*(?<charset>[^""]*)");

            public class HttpWebGetterException : Exception
            {
                public HttpWebGetterException(string url, Exception ex, string reponseText)
                {
                    Url = url;
                    Error = ex;
                    ResponseText = reponseText;
                }

                public string Url { get; private set; }

                public Exception Error { get; private set; }

                public string ResponseText { private set; get; }
            }


        }

        public class FormFile
        {
            public string Name { get; set; }

            public string ContentType { get; set; }

            public string FilePath { get; set; }

            public byte[] FileData { get; set; }
        }

        public enum ContentTypeEnum
        {
            Text,

            Html,

            Xml,

            Json,

            Stream,

            FormUrlencoded,

            FormData

        }

        public class WebDownloadFileInfo
        {
            public string FileExt { set; get; }
            public int FileSize { set; get; }
            public byte[] Data { set; get; }
        }
    }
}

