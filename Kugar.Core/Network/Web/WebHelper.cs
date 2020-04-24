using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Record.Chart;

namespace Kugar.Core.Network
{
    /// <summary>
    /// Description of RequestEx.
    /// </summary>
    public static class WebHelper
    {
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

        static WebHelper()
        {
            DefaultUserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
            DefaultAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8,image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/vnd.ms-excel, application/msword, */*";
        }

        public static string DefaultUserAgent
        {
            set { _defaultUA = value; }
            get { return _defaultUA; }
        }

        public static string DefaultAccept { set; get; }

        public static HttpWebGetter Create(string url)
        {
            return new HttpWebGetter(url);
        }

        /// <summary>
        /// 	获取指定Uri返回的文本数据
        /// </summary>
        /// <param name="uri">远程地址</param>
        /// <param name="encoding">文本编码</param>
        /// <returns></returns>
        [Obsolete("建议使用Create函数创建访问对象")]
        public static string GetUriData(string uri, string userAgent = null, System.Text.Encoding encoding = null, CookieContainer cookie = null, bool randomUA = false, int timeout = 30000)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "get";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout;


            //if (headers.HasData())
            //{
            //    foreach (var header in headers)
            //    {
            //        if (header.Key == "Accept")
            //        {
            //            request.Accept = header.Value;
            //        }
            //        else if (header.Key == "ContentType" || header.Key == "Content-Type")
            //        {
            //            request.ContentType = header.Value;
            //        }
            //        else
            //        {
            //            request.Headers.Add(header.Key, header.Value);
            //        }

            //    }
            //}


            //using (var requestStream = request.GetRequestStream())
            //{
            //    //var bytes = Encoding.UTF8.GetBytes(postDataStr.Trim());
            //    requestStream.Write(postDataStr, 0, postDataStr.Length);
            //    requestStream.Close();
            //}

            try
            {
                var data = GetResponseData((HttpWebResponse)request.GetResponse(), encoding, true, true);

                return data;
            }
            catch (Exception)
            {
                if (true)
                {
                    throw;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

#if NET45

        public static async Task<string> GetUriDataAsync(string uri, string userAgent = null, System.Text.Encoding encoding = null, CookieContainer cookie = null, bool randomUA = false)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "get";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.Timeout = 15000;
            request.ReadWriteTimeout = 15000;

            var ua = userAgent;

            if (string.IsNullOrEmpty(ua))
            {
                if (randomUA)
                {
                    ua = _normalUA[RandomEx.Next(_normalUA.Length)];
                }
                else
                {
                    ua = _defaultUA;
                }
            }

            request.UserAgent = ua;

            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }


            //request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");

            HttpWebResponse resp = null;
            System.IO.Stream respStream = null;
            try
            {
                resp = (HttpWebResponse)request.GetResponse();

                return await GetResponseDataAsync(resp, encoding);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问:{uri}出错,出错提示:{ex.Message}");

                return string.Empty;
            }
            finally
            {
                //    if (respStream != null)
                //    {
                //        respStream.Close();
                //        respStream.Dispose();
                //    }

                //    if (resp != null)
                //    {
                //        resp.Close();
                //    }

            }
        }

#endif

        public static byte[] GetUriData_Binary(string uri, string userAgent = "", bool isThrowException = false)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "get";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");


            //using (var requestStream = request.GetRequestStream())
            //{
            //    var bytes = Encoding.UTF8.GetBytes(postDataStr.Trim());
            //    requestStream.Write(bytes, 0, bytes.Length);
            //    requestStream.Close();
            //}

            try
            {
                var data = GetResponseData_Binary((HttpWebResponse)request.GetResponse(), true, isThrowException);

                return data;
            }
            catch
            {
                return null;
            }
        }


        public static string GetUriDataByPost(string uri, Dictionary<string, string> postDatas,
             string userAgent = "", System.Text.Encoding encoding = null, bool isThrowException = false, Dictionary<string, string> headers = null, int timeout = 30000)
        {
            var postDataStr = "";

            if (postDatas.HasData())
            {
                var sb = new StringBuilder();

                foreach (var data in postDatas)
                {
                    sb.AppendFormat("{0}={1}&", data.Key, WebUtility.HtmlEncode(data.Value));
                }

                postDataStr = sb.ToString();
            }


            return GetUriDataByPost(uri, postDataStr.Trim('&'), userAgent, encoding, isThrowException, headers, timeout);

            //var request = (HttpWebRequest) HttpWebRequest.Create(uri);
            //request.Method = "post";
            //request.Credentials = CredentialCache.DefaultCredentials;
            //request.Accept = DefaultAccept;
            //request.UserAgent = DefaultUserAgent;
            //request.ContentType = "application/x-www-form-urlencoded";
            ////request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            //request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            //request.Headers.Add("Accept-Encoding", "gzip,deflate");

            //StringBuilder dataStr = new StringBuilder();

            //foreach (var data in postDatas)
            //{
            //    dataStr.AppendFormat("{0}={1}&",data.Key, WebUtility.HtmlEncode(data.Value));
            //}

            //using (var requestStream = request.GetRequestStream())
            //{
            //    var bytes = Encoding.UTF8.GetBytes(dataStr.ToString().Trim());
            //    requestStream.Write(bytes, 0, bytes.Length);
            //    requestStream.Close();
            //}

            //HttpWebResponse resp = null;
            //Stream respStream = null;
            //try
            //{
            //    resp = (HttpWebResponse) request.GetResponse();

            //    using (respStream = resp.GetResponseStream())
            //    {
            //        if (encoding == null)
            //        {
            //            if (!string.IsNullOrWhiteSpace(resp.ContentEncoding))
            //            {
            //                encoding = Encoding.GetEncoding(resp.ContentEncoding);
            //            }
            //            else if (!string.IsNullOrWhiteSpace(resp.CharacterSet))
            //            {
            //                encoding = Encoding.GetEncoding(resp.CharacterSet);
            //            }
            //            else
            //            {
            //                var reader = new StreamReader(respStream, Encoding.ASCII);

            //                string html = reader.ReadToEnd();

            //                if (reg_charset.IsMatch(html))
            //                {
            //                    encoding = Encoding.GetEncoding(reg_charset.Match(html).Groups["charset"].Value);
            //                }
            //            }
            //        }

            //        if (encoding == null)
            //        {
            //            encoding = Encoding.Default;
            //        }

            //        if (resp.ContentEncoding.Contains("gzip"))
            //        {
            //            return getGzipStr(respStream, encoding);
            //        }
            //        else if (resp.ContentEncoding.Contains("deflate"))
            //        {
            //            return getDeflateStr(respStream, encoding);
            //        }
            //        else
            //        {
            //            return getNormal(respStream, encoding);
            //        }

            //    }
            //}
            //catch (Exception)
            //{
            //    return string.Empty;
            //}
            //finally
            //{
            //    if (respStream != null)
            //    {
            //        respStream.Close();
            //        respStream.Dispose();
            //    }

            //    if (resp != null)
            //    {
            //        resp.Close();
            //    }
            //}
        }

        //public static string GetUriDataByGet(string uri, Dictionary<string, string> getDatas, string userAgent = "",
        //    System.Text.Encoding encoding = null)
        //{

        //}

        //public static string GetUriDataByGet(string uri, string getData, string userAgent = "",
        //    System.Text.Encoding encoding = null)
        //{

        //}

        public static string GetUriDataByPost(string uri, string postDataStr, string userAgent = "", Encoding encoding = null, bool isThrowException = false, Dictionary<string, string> headers = null, int timeout = 30000)
        {
            return GetUriDataByPost(uri, Encoding.UTF8.GetBytes(postDataStr.Trim()), userAgent, encoding, isThrowException, headers, timeout);
        }

        public static string GetUriDataByPost(string uri, byte[] postDataStr, string userAgent = "",
            Encoding encoding = null, bool isThrowException = false, Dictionary<string, string> headers = null, int timeout = 30000)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "post";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout;


            if (headers.HasData())
            {
                foreach (var header in headers)
                {
                    if (header.Key == "Accept")
                    {
                        request.Accept = header.Value;
                    }
                    else if (header.Key == "ContentType" || header.Key == "Content-Type")
                    {
                        request.ContentType = header.Value;
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                }
            }


            using (var requestStream = request.GetRequestStream())
            {
                //var bytes = Encoding.UTF8.GetBytes(postDataStr.Trim());
                requestStream.Write(postDataStr, 0, postDataStr.Length);
                requestStream.Close();
            }

            try
            {
                var data = GetResponseData((HttpWebResponse)request.GetResponse(), encoding, true, isThrowException);

                return data;
            }
            catch (Exception)
            {
                if (isThrowException)
                {
                    throw;
                }
                else
                {
                    return string.Empty;
                }
            }

        }


        public static JObject GetUriDataByPostJson(string uri, JObject json, string userAgent = "",
            Encoding encoding = null, bool isThrowException = false, Dictionary<string, string> headers = null, int timeout = 30000)
        {
            headers = headers ?? new Dictionary<string, string>();

            headers.AddOrUpdate("Content-Type", "application/json");
            headers.AddOrUpdate("Accept", "application/json");

            byte[] data = null;

            if (json.HasData())
            {
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }

                data = encoding.GetBytes(json.ToStringEx(Formatting.None));
            }

            var ret = GetUriDataByPost(uri, data, userAgent, encoding, isThrowException, headers, timeout);

            if (string.IsNullOrEmpty(ret))
            {
                return null;
            }
            else
            {
                return JObject.Parse(ret);
            }
        }


        public static byte[] GetUriDataByPost_Binary(string uri, string postDataStr, string userAgent = "", bool isThrowException = false)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "post";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.UserAgent = DefaultUserAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");


            using (var requestStream = request.GetRequestStream())
            {
                var bytes = Encoding.UTF8.GetBytes(postDataStr.Trim());
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            try
            {
                var data = GetResponseData_Binary((HttpWebResponse)request.GetResponse(), true, isThrowException);

                return data;
            }
            catch
            {
                return null;
            }
        }

        public static WebDownloadFileInfo GetFile(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "get";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
            request.Headers.Add("Accept-Charset", "GBK,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");

            HttpWebResponse resp = null;
            System.IO.Stream respStream = null;
            try
            {
                resp = (HttpWebResponse)request.GetResponse();

                if ((int)resp.StatusCode != 200)
                {
                    return null;
                }

                var item = new WebDownloadFileInfo();

                if (!string.IsNullOrWhiteSpace(resp.ContentType))
                {
                    var extLst = resp.ContentType.Split('/');
                    if (extLst.Length == 2)
                    {
                        item.FileExt = extLst[1];
                    }
                }

                if (resp.ContentLength > 0)
                {
                    item.FileSize = (int)resp.ContentLength;
                }

                using (respStream = resp.GetResponseStream())
                {
                    item.Data = respStream.ReadAllBytes();
                }

                return item;

            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Close();
                    respStream.Dispose();
                }

                if (resp != null)
                {
                    resp.Close();
                    resp.Dispose();
                }


            }
        }

        public static WebDownloadFileInfo GetFile(string uri)
        {
            return GetFile(uri.ToUri());
        }

        public static string GetResponseData(HttpWebResponse response, Encoding encoding, bool isAutoClosed = true, bool isThrowException = false)
        {
            Stream respStream = null;
            try
            {
                //resp = (HttpWebResponse)request.GetResponse();

                using (respStream = response.GetResponseStream())
                {
                    var bytes = respStream.ReadAllBytes();

                    if (respStream == null)
                    {
                        return string.Empty;
                    }

                    if (encoding == null)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(response.CharacterSet))
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                            else if (!string.IsNullOrWhiteSpace(response.CharacterSet))
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                            else
                            {
                                //var reader = new StreamReader(respStream, Encoding.ASCII);

                                string html = Encoding.ASCII.GetString(bytes);

                                if (reg_charset.IsMatch(html))
                                {
                                    encoding = Encoding.GetEncoding(reg_charset.Match(html).Groups["charset"].Value);
                                }
                            }
                        }
                        catch (System.Exception)
                        {

                            throw;
                        }

                    }

                    if (encoding == null)
                    {
                        encoding = Encoding.Default;
                    }

                    var responseContentEncoding = response.ContentEncoding;

                    if (responseContentEncoding.Contains("gzip", true))
                    {
                        return getGzipStr(bytes, encoding);
                    }
                    else if (responseContentEncoding.Contains("deflate", true))
                    {
                        return getDeflateStr(bytes, encoding);
                    }
                    else
                    {
                        return getNormal(bytes, encoding);
                    }

                }
            }
            catch (Exception)
            {
                if (isThrowException)
                {
                    throw;
                }
                else
                {
                    return string.Empty;
                }

            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Close();
                    respStream.Dispose();
                }

                if (isAutoClosed)
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }

            }
        }

#if NET45

        public static async Task<string> GetResponseDataAsync(HttpWebResponse response, Encoding encoding, bool isAutoClosed = true, bool isThrowException = false)
        {
            Stream respStream = null;
            try
            {
                //resp = (HttpWebResponse)request.GetResponse();

                using (respStream = response.GetResponseStream())
                {
                    if (respStream == null)
                    {
                        return string.Empty;
                    }

                    if (encoding == null)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(response.CharacterSet))
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                            else if (!string.IsNullOrWhiteSpace(response.CharacterSet))
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                            else
                            {
                                var reader = new StreamReader(respStream, Encoding.ASCII);

                                //string html = reader.ReadToEnd();

                                //if (reg_charset.IsMatch(html))
                                //{
                                //    encoding = Encoding.GetEncoding(reg_charset.Match(html).Groups["charset"].Value);
                                //}
                            }
                        }
                        catch (System.Exception)
                        {

                            throw;
                        }

                    }

                    if (encoding == null)
                    {
                        encoding = Encoding.Default;
                    }

                    var responseContentEncoding = response.ContentEncoding;

                    if (responseContentEncoding.Contains("gzip", true))
                    {
                        return await getGzipStrAsync(respStream, encoding);
                    }
                    else if (responseContentEncoding.Contains("deflate", true))
                    {
                        return await getGzipStrAsync(respStream, encoding);
                    }
                    else
                    {
                        return await getGzipStrAsync(respStream, encoding);
                    }

                }
            }
            catch (Exception)
            {
                if (isThrowException)
                {
                    throw;
                }
                else
                {
                    return string.Empty;
                }

            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Close();
                    respStream.Dispose();
                }

                if (isAutoClosed)
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }

            }
        }

#endif
        public static byte[] GetResponseData_Binary(HttpWebResponse response, bool isAutoClosed = true, bool isThrowException = false)
        {
            Stream respStream = null;
            try
            {
                //resp = (HttpWebResponse)request.GetResponse();

                using (respStream = response.GetResponseStream())
                {
                    if (respStream == null)
                    {
                        return null;
                    }

                    var count = (int)response.ContentLength;

                    var data = new byte[count];

                    var readCount = 0;

                    var currentPos = -1;

                    BinaryReader br = new BinaryReader(respStream);
                    while (currentPos != count - 1)
                    {
                        readCount = br.Read(data, currentPos + 1, count - (currentPos + 1));
                        //readCount = respStream.Read(data, currentPos + 1, count);
                        if (readCount <= 0)
                        {
                            break;
                        }
                        currentPos += readCount;
                    }


                    br.Close();
                    br.Dispose();

                    return data;
                }
            }
            catch (Exception)
            {
                if (isThrowException)
                {
                    throw;
                }
                else
                {
                    return null;
                }

            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Close();
                    respStream.Dispose();
                }

                if (isAutoClosed)
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }

            }
        }

        public class WebDownloadFileInfo
        {
            public string FileExt { set; get; }
            public int FileSize { set; get; }
            public byte[] Data { set; get; }
        }

        private static string getGzipStr(byte[] buff, Encoding encoding)
        {
            using (var stream = new ByteStream(buff))
            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            {
                var data = responeReader.ReadToEnd();

                return data;
            }
        }

        private static string getGzipStr(Stream srcStream, System.Text.Encoding encoding)
        {

            using (GZipStream zipStream = new GZipStream(srcStream, CompressionMode.Decompress))
            using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            {
                var data = responeReader.ReadToEnd();

                return data;
            }
        }

        private static string getDeflateStr(byte[] buff, System.Text.Encoding encoding)
        {
            return getDeflateStr(new ByteStream(buff), encoding);

            //using (DeflateStream zipStream = new DeflateStream(srcStream, CompressionMode.Decompress))
            //using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            //{
            //    var data = responeReader.ReadToEnd();

            //    return data;
            //}
        }

        private static string getDeflateStr(Stream srcStream, System.Text.Encoding encoding)
        {
            using (DeflateStream zipStream = new DeflateStream(srcStream, CompressionMode.Decompress))
            using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            {
                var data = responeReader.ReadToEnd();

                return data;
            }
        }

        private static string getNormal(Stream srcStream, System.Text.Encoding encoding)
        {
            var data = srcStream.ReadToEnd(encoding);

            return data;
        }

        private static string getNormal(byte[] buff, System.Text.Encoding encoding)
        {
            return encoding.GetString(buff);

            //var data = srcStream.ReadToEnd(encoding);

            //return data;
        }

        private static readonly Regex reg_charset = new Regex(@"charset\b\s*=\s*(?<charset>[^""]*)");


#if NET45
        private static async Task<string> getGzipStrAsync(byte[] buff, Encoding encoding)
        {
            using (var stream = new ByteStream(buff))
            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            {
                var data = await responeReader.ReadToEndAsync();

                return data;
            }
        }

        private static async Task<string> getGzipStrAsync(Stream srcStream, System.Text.Encoding encoding)
        {
            using (var zipStream = new GZipStream(srcStream, CompressionMode.Decompress))
            using (var responeReader = new StreamReader(zipStream, encoding))
            {
                var data = await responeReader.ReadToEndAsync();

                return data;
            }
        }

        private static async Task<string> getDeflateStrAsync(byte[] buff, Encoding encoding)
        {
            using (var stream = new ByteStream(buff))
            using (var zipStream = new DeflateStream(stream, CompressionMode.Decompress))
            using (var responeReader = new StreamReader(zipStream, encoding))
            {
                var data = await responeReader.ReadToEndAsync();

                return data;
            }
        }

        private static async Task<string> getDeflateStrAsync(Stream srcStream, System.Text.Encoding encoding)
        {
            using (DeflateStream zipStream = new DeflateStream(srcStream, CompressionMode.Decompress))
            using (StreamReader responeReader = new StreamReader(zipStream, encoding))
            {
                var data = await responeReader.ReadToEndAsync();

                return data;
            }
        }

        private static async Task<string> getNormalAsync(byte[] buff, System.Text.Encoding encoding)
        {
            return encoding.GetString(buff);
        }

        private static async Task<string> getNormalAsync(Stream srcStream, System.Text.Encoding encoding)
        {
            using (var sr = new StreamReader(srcStream))
            {
                var data = await sr.ReadToEndAsync();

                return data;
            }


        }


#endif


        private static string getEncodingData(HttpWebResponse response)
        {
            StreamReader reader = null;
            try
            {
                if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                else
                    reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);

                string html = reader.ReadToEnd();

                if (reg_charset.IsMatch(html))
                {
                    return reg_charset.Match(html).Groups["charset"].Value;
                }
                else if (response.CharacterSet != string.Empty)
                {
                    return response.CharacterSet;
                }
                else
                {
                    return Encoding.Default.BodyName;
                }


            }
            catch
            {
                return Encoding.Default.BodyName;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (reader != null)
                    reader.Close();
            }
        }


    }
    public class FormFile
    {
        public string FileName { set; get; }

        public string ContentType { get; set; }

        public string FilePath { get; set; }

        public Stream Stream { get; set; }
    }

    /// <summary>
    /// 用于获取HttpWeb的数据的包装类
    /// </summary>
    public class HttpWebGetter
    {
        //private Lazy<Dictionary<string,string>> _headers=new Lazy<Dictionary<string, string>>(()=>new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase));
        private HttpWebRequest _request = null;
        //private WebClient _client = null;
        private JObject _jsonContent = null;
        private Encoding _encoding = System.Text.Encoding.UTF8;
        private Dictionary<string, object> _values = null;
        private string _contentType = "";
        private bool _hasChangeContentType = false;
        private Dictionary<string,string> _cookies=new Dictionary<string, string>();
        private CookieCollector _collector = null;
        
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
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";



        #endregion



        static HttpWebGetter()
        {
            DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36";
            DefaultAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
          

        }

        public static string DefaultAccept { get; set; }

        public static string DefaultUserAgent { get; set; }

        public HttpWebGetter(string url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "get";
            //request.Credentials = CredentialCache.DefaultCredentials;
            request.Accept = DefaultAccept;
            request.UserAgent = DefaultUserAgent;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.CachePolicy=new System.Net.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);	
            request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Credentials=new NetworkCredential();
            request.ServicePoint.Expect100Continue = true;
            request.Proxy = null ;
            //request.Timeout = timeout;
            //request.ReadWriteTimeout = timeout;

            _request = request;
        }

        public HttpWebGetter SetCookie(string name, string value,string path="/",string domain="")
        {
            //_request.CookieContainer = _request.CookieContainer ?? new CookieContainer();

            _cookies.AddOrUpdate(name, value);

            //_request.CookieContainer.Add(new Cookie(name,value, path,domain));

            return this;
        }

        public HttpWebGetter SetCookieCollector(CookieCollector collector)
        {
            _collector = collector;
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

        public HttpWebGetter SetCookie(IEnumerable<KeyValuePair<string,string>> cookies)
        {
            foreach (var pair in cookies)
            {
                _cookies.Add(pair.Key,pair.Value);
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
                _request.Accept = value;
            }
            else if (headerName.CompareTo("ContentType", true) || headerName.CompareTo("Content-Type", true))
            {
                _request.ContentType = value;
            }
            else if (headerName.CompareTo("ContentLength", true))
            {
                _request.ContentLength = value.ToInt();
            }
            else if (headerName.CompareTo("user-agent", true))
            {
                _request.UserAgent = value;
            }
            else if (headerName.CompareTo("referer", true))
            {
                _request.Referer = value;
            }
            else if (headerName.CompareTo("host", true))
            {
                _request.Host = value;
            }
            else if (headerName.CompareTo("Referer", true))
            {
                _request.Referer = value;
            }
            else
            {
                _request.Headers.Add(headerName, value);
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

        public HttpWebGetter SetParamters(Dictionary<string, object> values)
        {
            _values = values;
            _jsonContent = null;

            return this;
        }

        /// <summary>
        /// 设置一个指定名称和值的参数,使用该方式,则在提交数据的时候,会使用键值对的方式提交
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        /// <param name="autoEncode">对字符串进行html编码</param>
        /// <returns></returns>
        public HttpWebGetter SetParamter(string name, string value)
        {
            return SetParamter(name, value, true);
        }

        public HttpWebGetter SetParamter(string name, int value)
        {
            return SetParamter(name, value.ToString(),false);
        }

        public HttpWebGetter SetParamter(string name, string value, bool autoEncode)
        {
            if (_values == null)
            {
                _values = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            }

            if (autoEncode)
            {
                value = HttpUtility.UrlEncode(value);
            }

            _values.Add(name, value);

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

        public HttpWebGetter Accept(string accept)
        {
            _request.Accept = accept;

            return this;
        }

        /// <summary>
        /// 设置UserAgent属性值
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public HttpWebGetter UserAgent(string userAgent)
        {
            _request.UserAgent = userAgent;

            return this;
        }

        public HttpWebGetter ContentType(string contentType)
        {
            _contentType = contentType;
            _request.ContentType = _contentType;
            _hasChangeContentType = true;
            return this;
        }

        public HttpWebGetter ContentType(ContentTypeEnum type)
        {
            return ContentType(getContentType(type));

            return this;
        }

        /// <summary>
        /// 使用Post提交数据,并将返回数据作为string返回
        /// </summary>
        /// <returns></returns>
        public string Post_String()
        {
            _request.Method = "POST";

            var data = postData_String();

            return data;
        }

        /// <summary>
        /// 使用Get提交数据,并将返回数据作为string返回
        /// </summary>
        /// <returns></returns>
        public string Get_String()
        {
            _request.Method = "GET";

            var data = getData_String();

            return data;
        }

        /// <summary>
        /// 使用Post提交数据,并将数据作为Json格式返回
        /// </summary>
        /// <returns></returns>
        public JObject Post_Json()
        {
            if (!_hasChangeContentType)
            {
                _request.ContentType = getContentType(ContentTypeEnum.Json);
            }
            
            var str = Post_String();

            return JObject.Parse(str);
        }

        /// <summary>
        /// 使用Get提交数据,并将数据作为Json格式返回
        /// </summary>
        /// <returns></returns>
        public JObject Get_Json()
        {
            if (!_hasChangeContentType)
            {
                _request.ContentType = getContentType(ContentTypeEnum.Json);
            }
            
            var str = Get_String();

            return JObject.Parse(str);
        }

        public byte[] Get_Binary()
        {
            _request.Method = "GET";

            var data = getData_Binary();

            return data;
        }

        public byte[] Post_Binary()
        {
            _request.Method = "POST";

            var data = postData_Binary();

            return data;
        }
        
        //public Stream Get_File()
        //{

        //}

        //public Stream Post_File()
        //{

        //}

        private byte[] postData_Binary()
        {
            buildPostRequest();

            //_request.TransferEncoding = _encoding;
    

            try
            {
                using (var response = (HttpWebResponse)_request.GetResponse())
                {
                    if (_request.CookieContainer != null)
                    {
                        _request.CookieContainer.Add(response.Cookies);
                    }
                    
                    if (_collector != null && response.Cookies.HasData())
                    {
                        _collector.Update(response.Cookies);
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException("无效授权",WebExceptionStatus.ProtocolError);
                    }

                    byte[] data = null;
                    var ret = getResponseData_Binary(response);

                    if (ret.ReturnData != null && ret.ReturnData.Length>0)
                    {
                        data = ret.ReturnData;

                        var responseContentEncoding = response.ContentEncoding;

                        if (responseContentEncoding.Contains("gzip", true))
                        {
                            using (var stream = new ByteStream(data))
                            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                data = zipStream.ReadAllBytes();
                            }
                        }
                        else if (responseContentEncoding.Contains("deflate", true))
                        {
                            using (var stream = new ByteStream(data))
                            using (DeflateStream zipStream = new DeflateStream(stream, CompressionMode.Decompress))
                            {
                                data = zipStream.ReadAllBytes();
                            }
                        }
                    }

                    if (!ret.IsSuccess)
                    {
                        if (data != null)
                        {
                            //var encoding = System.Text.Encoding.GetEncoding(response.ContentEncoding);

                            var error = new HttpWebGetterException(_request.RequestUri.ToStringEx(), ret.Error,
                                _encoding.GetString(data));

                            throw error;
                        }
                        else
                        {
                            throw ret.Error;
                        }
                    }

                    return data;
                }
            }
            catch (WebException ex)
            {
                var s = getResponseData_Binary((HttpWebResponse)ex.Response);

                var error = new HttpWebGetterException(_request.RequestUri.ToStringEx(), ex,
                    _encoding.GetString(s.ReturnData));

                throw error;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] getData_Binary()
        {
           

            if (_cookies.HasData())
            {
                //var cookie = _cookies.Select(x => $"{x.Key}={x.Value};").JoinToString("");

                //_request.Headers.Add("Cookie", cookie);
                _request.CookieContainer = new CookieContainer();
                if (_collector != null)
                {
                    _collector.Update(_cookies);
                }

                foreach (var cookie1 in _cookies)
                {
                    _request.CookieContainer.Add(new Cookie(cookie1.Key, cookie1.Value, "/", _request.RequestUri.Host));
                }

            }

            

            try
            {
                using (var response = (HttpWebResponse)_request.GetResponse())
                {
                    if (_request.CookieContainer != null)
                    {
                        _request.CookieContainer.Add(response.Cookies);
                    }

                    byte[] data = null;
                    var ret = getResponseData_Binary(response);

                    if (_collector!=null && response.Cookies.HasData())
                    {
                        _collector.Update(response.Cookies);
                    }

                    if (ret.ReturnData != null)
                    {
                        data = ret.ReturnData;

                        var responseContentEncoding = response.ContentEncoding.ToStringEx();

                        if (responseContentEncoding.Contains("gzip", true))
                        {
                            using (var stream = new ByteStream(data))
                            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                data = zipStream.ReadAllBytes();
                            }
                        }
                        else if (responseContentEncoding.Contains("deflate", true))
                        {
                            using (var stream = new ByteStream(data))
                            using (DeflateStream zipStream = new DeflateStream(stream, CompressionMode.Decompress))
                            {
                                data = zipStream.ReadAllBytes();
                            }
                        }
                    }

                    if (!ret.IsSuccess)
                    {
                        if (data != null)
                        {
                            //var encoding = System.Text.Encoding.GetEncoding(response.ContentEncoding);

                            var error = new HttpWebGetterException(_request.RequestUri.ToStringEx(), ret.Error, _encoding.GetString(data));

                            throw error;
                        }
                        else
                        {
                            throw ret.Error;
                        }
                    }

                    return data;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string getData_String()
        {
            var s = getData_Binary();

            var str = _encoding.GetString(s);

            return str;
        }

        private string postData_String()
        {
            var s = postData_Binary();

            if (s.HasData())
            {
                return _encoding.GetString(s);
            }
            else
            {
                return "";
            }
        }

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

        private ResultReturn<byte[]> getResponseData_Binary(HttpWebResponse response)
        {
            Stream respStream = null;
            Exception error = null;
            try
            {
                respStream = response.GetResponseStream();
            }
            catch (WebException ex)
            {
                respStream = ex.Response.GetResponseStream();
                error = ex;
            }
            catch (Exception e)
            {
                return new FailResultReturn<byte[]>(e);
            }

            if (respStream == null)
            {
                return null;
            }

            try
            {
                using (respStream)
                {
                    var readCount = 0;

                    //var currentPos = -1;

                    byte[] data = null;

                    using (Stream stream = new MemoryStream())
                    using (BinaryReader br = new BinaryReader(respStream))
                    {
                        var buffer = new byte[2048];

                        do
                        {
                            readCount = br.Read(buffer, 0, buffer.Length);

                            if (readCount <= 0)
                            {
                                break;
                            }
                            else
                            {
                                stream.Write(buffer, 0, readCount);
                            }

                            //currentPos += readCount;
                        } while (readCount > 0);

                        stream.Position = 0;

                        data = stream.ReadAllBytes();
                    }

                    //br.Close();
                    //br.Dispose();

                    return new ResultReturn<byte[]>(error == null, data, error: error);
                }
            }
            catch (Exception ex)
            {
                return new FailResultReturn<byte[]>(ex);
            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Close();
                    respStream.Dispose();
                }
            }
        }

        private void buildPostRequest()
        {
            //_request.CookieContainer = new CookieContainer();

            if (_cookies.HasData())
            {
                _request.CookieContainer = new CookieContainer();
                if (_collector != null)
                {
                    _collector.Update(_cookies);
                }

                foreach (var cookie1 in _cookies)
                {
                    _request.CookieContainer.Add(new Cookie(cookie1.Key, cookie1.Value, "/", _request.RequestUri.Host));
                }
            }
            
            

            //using ()
            {
                if (_jsonContent != null)
                {
                    using (var requestStream = _request.GetRequestStream())
                    {
                        var tmp = _jsonContent.ToStringEx();

                        var b = _encoding.GetBytes(tmp);

                        _request.ContentType = string.IsNullOrWhiteSpace(_contentType)
                            ? getContentType(ContentTypeEnum.Json)
                            : _contentType;

                        requestStream.Write(b, 0, b.Length);
                    }
                    
                }
                else if (_values.HasData())
                {
                    if (!_values.Any(x => x.Value is FormFile) && !_request.ContentType.Contains("form-data",true))
                    {
                        var sb=new StringBuilder();

                        foreach (var value in _values)
                        {
                            sb.Append($"{value.Key}={value.Value.ToStringEx()}&");
                        }

                        sb.Remove(sb.Length - 1, 1);

                        var requestStr = sb.ToString();

                        _request.ContentType = string.IsNullOrWhiteSpace(_contentType)
                            ? getContentType(ContentTypeEnum.FormUrlencoded)
                            : _contentType;

                        var data = _encoding.GetBytes(requestStr);

                        using (var requestStream = _request.GetRequestStream())
                        {
                            requestStream.Write(data,0,data.Length);
                        }
                        

                        //_request.ContentLength = data.Length;
                    }
                    else
                    {
                        var temp = new MultipartForm(_request);

                        foreach (var pair in _values)
                        {
                            if (!(pair.Value is FormFile))
                            {
                                temp.SetField(pair.Key, pair.Value.ToStringEx());

                            }
                        }

                        var file1 = _values.FirstOrDefault(x => x.Value is FormFile).Value as FormFile;
                        temp.FileContentType = file1?.ContentType;
                        //temp.SetFilename("request.log");
                        temp.SendFile(file1?.FilePath);



                        //temp.GetResponse();

                        //buildPostFileRequest();

                        return;


                        var beginBoundary = "--WebChrome8c405ee4e38917c";
                        var contentBoundary= "--" + beginBoundary;
                        var endingBoundary= contentBoundary + "--\r\n";

                        _request.ContentType = string.IsNullOrWhiteSpace(_contentType)
                            ? getContentType(ContentTypeEnum.FormData)
                            : _contentType;

                        //string boundary = "---------------------------WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
                        //byte[] boundaryBytes = _encoding.GetBytes("\r\n" + boundary + "\r\n");

                        if (_values.Values.Any(x => x is FormFile))
                        {
                            _request.ContentType = getContentType(ContentTypeEnum.FormData) + "; boundary=" + beginBoundary;
                            _request.Headers.Add("Cache-Control", "no-cache");
                        }

                        //var contentLength = 0;

                        var stringFields =
                            buildPostStringField(contentBoundary, _values.Where(x => !(x.Value is FormFile)));

                        //var fileBlockLength =
                        //    calcFileLength(contentBoundary, _values.Where(x => x.Value is FormFile));

                        //_request.ContentLength = System.Text.Encoding.UTF8.GetByteCount(stringFields) + 0 /* fileBlockLength*/ ;

                        //using (var requestStream = _request.GetRequestStream())
                        using (var stream =new MemoryStream())
                        {
              
                            byte[] stringFieldsBytes = _encoding.GetBytes(stringFields);
                            stream.Write(stringFieldsBytes, 0, stringFieldsBytes.Length);

                            foreach (var pair in _values)
                            {
                                if (pair.Value is FormFile)
                                {
                                    FormFile file = pair.Value as FormFile;
                                    string header = contentBoundary + "\r\n Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.FileName + "\"\r\nContent-Type: " + file.ContentType + "\r\n";
                                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(header);
                                    stream.Write(bytes, 0, bytes.Length);
                                    byte[] buffer = new byte[32768];
                                    int bytesRead;

                                    //contentLength += bytes.Length;

                                    if (file.Stream == null)
                                    {
                                        // upload from file
                                        var fileStream = File.OpenRead(file.FilePath);
                                        using (fileStream)
                                        {
                                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                                stream.Write(buffer, 0, bytesRead);
                                            fileStream.Close();
                                        }

                                        //contentLength += (int)fileStream.Length;
                                    }
                                    else
                                    {
                                        // upload from given stream
                                        file.Stream.Position = 0;
                                        while ((bytesRead = file.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                            stream.Write(buffer, 0, bytesRead);

                                        //contentLength += (int)file.Stream.Length;
                                    }
                                    
                                }
                                else
                                {
                                    //string data = contentBoundary + "\r\n Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n\r\n" + pair.Value + "\r\n"; ;
                                    //byte[] bytes = _encoding.GetBytes(data);
                                    //requestStream.Write(bytes, 0, bytes.Length);
                                    
                                }
                            }

                            _request.ContentLength = stream.Length;
                            stream.Seek(0, SeekOrigin.Begin);

                            using (var reqStream=_request.GetRequestStream())
                            {
                                stream.CopyTo(reqStream);
                            }

                        }

                        

                        //byte[] trailer = _encoding.GetBytes("\r\n--" + boundary + "--\r\n");

                        //requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                        //_request.ContentLength = contentLength;
                    }


                }

                //requestStream.Close();
            }
        }

        private void buildPostFileRequest()
        {
            string boundary = "7d930d1a850658";

            _request.ContentType = getContentType(ContentTypeEnum.FormData) + "; boundary=" + boundary;
            _request.Headers.Add("Cache-Control", "no-cache");
            //_request.ContentType = "multipart/form-data; boundary=" + boundary;

            var beginBoundary = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            
            var endBoundary = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

            const string filePartHeader =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                "Content-Type: application/octet-stream\r\n\r\n";

            var pair = _values.FirstOrDefault(x => x.Value is FormFile);
            var file = (FormFile) pair.Value;

            
        
            var memStream = new MemoryStream();
            memStream.Write(beginBoundary, 0, beginBoundary.Length);

            var stringKeyHeader = "\r\n--" + boundary +
                                  "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                  "\r\n\r\n{1}\r\n";

            foreach (var pair1 in _values.Where(x=>!(x.Value is FormFile)))
            {
                var data = string.Format(stringKeyHeader, pair1.Key, pair1.Value.ToStringEx());
                var b = System.Text.Encoding.UTF8.GetBytes(data);
                memStream.Write(b,0,b.Length);
            }

            var fileHeader = string.Format(filePartHeader, pair.Key, file.FileName);
            var fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(fileHeader);

            memStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            var autoDispose = false;

            if (file.Stream==null)
            {
                autoDispose = true;
                var stream = File.Open(file.FilePath, FileMode.Open);
                file.Stream = stream;
            }
            
            file.Stream.CopyTo(memStream,2048);

            if (autoDispose)
            {
                file.Stream.Dispose();
            }

            memStream.Write(endBoundary, 0, endBoundary.Length);


            _request.ContentLength = memStream.Length;
            

            memStream.Seek(0l, SeekOrigin.Begin);

            using (memStream)
            using(var requestStream=_request.GetRequestStream())
            {
                memStream.CopyTo(requestStream,2048);
            }
            
        }

        private string buildPostStringField(string contentBoundary,IEnumerable<KeyValuePair<string, object>> values)
        {
            var sb=new StringBuilder(); 

            foreach (var pair in values)
            {
                sb.Append(contentBoundary + "\r\n Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n" +
                          pair.Value + "\r\n");
            }

            return sb.ToStringEx();
        }

        private int calcFileLength(string contentBoundary, IEnumerable<KeyValuePair<string, object>> values)
        {
            //var sb=new StringBuilder();
            var length = 0;

            foreach (var pair in values)
            {
                var file = pair.Value as FormFile;

                //头部数据
                //contentBoundary.Length + "\r\n Content-Disposition: form-data; name=\"".Length + pair.Key.Length +
                //    "\"; filename=\"".Length + file.FileName.Length + "\"\r\nContent-Type: ".Length + file.ContentType.Length +
                //    "\r\n\r\n".Length;

                string header = contentBoundary + "\r\n Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.FileName + "\"\r\nContent-Type: " + file.ContentType + "\r\n";
                //byte[] bytes = ;

                //计算头部描述的长度
                length += System.Text.Encoding.UTF8.GetByteCount(header);

                //计算文件长度
                if (file.Stream==null)
                {
                    length += (int) new System.IO.FileInfo(file.FilePath).Length;
                }
                else
                {
                    length += (int)file.Stream.Length;
                }

                //文件数据尾部
                //  "\r\n"  + contentBoundary + "--\r\n"



                length += System.Text.Encoding.UTF8.GetByteCount("\r\n" + contentBoundary + "--\r\n");
            }

            return length;
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


        /// <summary>
        /// Allow the transfer of data files using the W3C's specification
        /// for HTTP multipart form data. Microsoft's version has a bug
        /// where it does not format the ending boundary correctly.
        /// Original version written by Gregory Prentice : gregoryp@norvanco.com
        /// See: http://www.c-sharpcorner.com/UploadFile/gregoryprentice/DotNetBugs12062005230632PM/DotNetBugs.aspx
        /// </summary>
        public class MultipartForm
        {
            private HttpWebRequest _request = null;

            /// <summary>
            /// Holds any form fields and values that you
            /// wish to transfer with your data.
            /// </summary>
            private Hashtable coFormFields;

            /// <summary>
            /// Used mainly to avoid passing parameters to other routines.
            /// Could have been local to sendFile().
            /// </summary>
            protected HttpWebRequest coRequest;

            /// <summary>
            /// Used if we are testing and want to output the raw
            /// request, minus http headers, out to a file.
            /// </summary>
            private Stream coFileStream;

            /// <summary>
            /// Difined to build the form field data that is being
            /// passed along with the request.
            /// </summary>
            private static string CONTENT_DISP = "Content-Disposition: form-data; name=";

            /// <summary>
            /// Allows you to specify the specific version of HTTP to use for uploads.
            /// The dot NET stuff currently does not allow you to remove the continue-100 header
            /// from 1.1 and 1.0 currently has a bug in it where it adds the continue-100. MS
            /// has sent a patch to remove the continue-100 in HTTP 1.0.
            /// </summary>
            public Version TransferHttpVersion { get; set; }

            /// <summary>
            /// Used to change the content type of the file being sent.
            /// Currently defaults to: text/xml. Other options are
            /// text/plain or binary
            /// </summary>
            public string FileContentType { get; set; }

            /// <summary>
            /// Initialize our class for use to send data files.
            /// </summary>
            /// <param name="url">The web address of the recipient of the data transfer.</param>
            public MultipartForm(HttpWebRequest request)
            {
                URL = request.RequestUri.ToStringEx();
                coFormFields = new Hashtable();
                ResponseText = new StringBuilder();
                BufferSize = 1024 * 10;
                BeginBoundary = "----8c405ee4e38917c";
                TransferHttpVersion = HttpVersion.Version11;
                FileContentType = "text/xml";

                _request = request;
            }
            //---------- BEGIN PROPERTIES SECTION ----------
            private string _BeginBoundary;
            /// <summary>
            /// The string that defines the begining boundary of
            /// our multipart transfer as defined in the w3c specs.
            /// This method also sets the Content and Ending
            /// boundaries as defined by the w3c specs.
            /// </summary>
            public string BeginBoundary
            {
                get { return _BeginBoundary; }
                set
                {
                    _BeginBoundary = value;
                    ContentBoundary = "--" + BeginBoundary;
                    EndingBoundary = ContentBoundary + "--";
                }
            }

            /// <summary>
            /// The string that defines the content boundary of
            /// our multipart transfer as defined in the w3c specs.
            /// </summary>
            protected string ContentBoundary { get; set; }

            /// <summary>
            /// The string that defines the ending boundary of
            /// our multipart transfer as defined in the w3c specs.
            /// </summary>
            protected string EndingBoundary { get; set; }

            /// <summary>
            /// The data returned to us after the transfer is completed.
            /// </summary>
            public StringBuilder ResponseText { get; set; }

            /// <summary>
            /// The web address of the recipient of the transfer.
            /// </summary>
            public string URL { get; set; }

            /// <summary>
            /// Allows us to determine the size of the buffer used
            /// to send a piece of the file at a time out the IO
            /// stream. Defaults to 1024 * 10.
            /// </summary>
            public int BufferSize { get; set; }

            //---------- END PROPERTIES SECTION ----------

            /// <summary>
            /// Used to signal we want the output to go to a
            /// text file verses being transfered to a URL.
            /// </summary>
            /// <param name="path"></param>
            public void SetFilename(string path)
            {
                coFileStream = new System.IO.FileStream(path, FileMode.CreateNew, FileAccess.Write);
            }
            /// <summary>
            /// Allows you to add some additional field data to be
            /// sent along with the transfer. This is usually used
            /// for things like userid and password to validate the
            /// transfer.
            /// </summary>
            /// <param name="key">The form field name</param>
            /// <param name="str">The form field value</param>
            public void SetField(string key, string str)
            {
                coFormFields[key] = str;
            }
            /// <summary>
            /// Determines if we have a file stream set, and returns either
            /// the HttpWebRequest stream of the file.
            /// </summary>
            /// <returns></returns>
            public virtual Stream GetStream()
            {
                Stream stream;
                if (null == coFileStream)
                    stream = coRequest.GetRequestStream();
                else
                    stream = coFileStream;

                //stream =new MemoryStream();

                return stream;
            }
            /// <summary>
            /// Here we actually make the request to the web server and
            /// retrieve it's response into a text buffer.
            /// </summary>
            public virtual void GetResponse()
            {
                

                if (null == coFileStream)
                {
                    Stream stream;
                    WebResponse response;
                    try
                    {
                        response = coRequest.GetResponse();
                    }
                    catch (WebException web)
                    {
                        response = web.Response;
                    }
                    if (null != response)
                    {
                        stream = response.GetResponseStream();
                        StreamReader sr = new StreamReader(stream);
                        string str;
                        ResponseText.Length = 0;
                        while ((str = sr.ReadLine()) != null)
                            ResponseText.Append(str);
                        response.Close();
                    }
                    else
                        throw new Exception("MultipartForm: Error retrieving server response");
                }
            }
            /// <summary>
            /// Transmits a file to the web server stated in the
            /// URL property. You may call this several times and it
            /// will use the values previously set for fields and URL.
            /// </summary>
            /// <param name="filename">The full path of file being transfered.</param>
            public void SendFile(string filename)
            {
                // The live of this object is only good during
                // this function. Used mainly to avoid passing
                // around parameters to other functions.
                coRequest =_request;
                // Set use HTTP 1.0 or 1.1.
                coRequest.ProtocolVersion = TransferHttpVersion;
                coRequest.Method = "POST";
                coRequest.ContentType = "multipart/form-data; boundary=" + BeginBoundary;
                coRequest.Headers.Add("Cache-Control", "no-cache");
                coRequest.KeepAlive = true;
                string strFields = GetFormfields();

                string strFileHdr = "";
                string strFileTlr = "";
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    strFileHdr = GetFileheader(filename);
                    strFileTlr = GetFiletrailer();
                    FileInfo info = new FileInfo(filename);
                    coRequest.ContentLength = System.Text.Encoding.UTF8.GetByteCount(strFields) +
                                              System.Text.Encoding.UTF8.GetByteCount(strFileHdr) +
                                              System.Text.Encoding.UTF8.GetByteCount(strFileTlr) +
                                              info.Length;
                }
                else
                {
                    coRequest.ContentLength = System.Text.Encoding.UTF8.GetByteCount(strFields);
                }
                
                System.IO.Stream io;
                io = GetStream();
                WriteString(io, strFields);

                if (!string.IsNullOrWhiteSpace(filename))
                {
                    WriteString(io, strFileHdr);
                    this.WriteFile(io, filename);
                    WriteString(io, strFileTlr);
                }
                


                //io.Seek(0, SeekOrigin.Begin);
                //var str = io.ReadToEnd(System.Text.Encoding.UTF8);

                //GetResponse();
                io.Close();
                // End the life time of this request object.
                coRequest = null;
            }
            /// <summary>
            /// Mainly used to turn the string into a byte buffer and then
            /// write it to our IO stream.
            /// </summary>
            /// <param name="stream">The io stream for output.</param>
            /// <param name="str">The data to write.</param>
            public void WriteString(Stream stream, string str)
            {
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(str);
                stream.Write(postData, 0, postData.Length);
            }
            /// <summary>
            /// Builds the proper format of the multipart data that
            /// contains the form fields and their respective values.
            /// </summary>
            /// <returns>The data to send in the multipart upload.</returns>
            public string GetFormfields()
            {
                string str = "";
                IDictionaryEnumerator myEnumerator = coFormFields.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    str += ContentBoundary + "\r\n" +
                      CONTENT_DISP + '"' + myEnumerator.Key + "\"\r\n\r\n" +
                      myEnumerator.Value + "\r\n";
                }
                return str;
            }
            /// <summary>
            /// Returns the proper content information for the
            /// file we are sending.
            /// </summary>
            /// <remarks>
            /// Hits Patel reported a bug when used with ActiveFile.
            /// Added semicolon after sendfile to resolve that issue.
            /// Tested for compatibility with IIS 5.0 and Java.
            /// </remarks>
            /// <param name="filename"></param>
            /// <returns></returns>
            public string GetFileheader(string filename)
            {
                if (string.IsNullOrWhiteSpace(FileContentType))
                {
                    FileContentType=MimeTypeMap.GetMimeType(Path.GetExtension(filename));
                }

                return ContentBoundary + "\r\n" +
                  CONTENT_DISP +
                  "\"file\"; filename=\"" +
                  Path.GetFileName(filename) + "\"\r\n" +
                  "Content-type: " + FileContentType + "\r\n\r\n";
            }
            /// <summary>
            /// Creates the proper ending boundary for the multipart upload.
            /// </summary>
            /// <returns>The ending boundary.</returns>
            public string GetFiletrailer()
            {
                return "\r\n" + EndingBoundary;
            }
            /// <summary>
            /// Reads in the file a chunck at a time then sends it to the
            /// output stream.
            /// </summary>
            /// <param name="stream">The io stream to write the file to.</param>
            /// <param name="filename">The name of the file to transfer.</param>
            public void WriteFile(Stream stream, string filename)
            {
                using (FileStream readIn = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    readIn.Seek(0, SeekOrigin.Begin); // move to the start of the file
                    byte[] fileData = new byte[BufferSize];
                    int bytes;
                    while ((bytes = readIn.Read(fileData, 0, BufferSize)) > 0)
                    {
                        // read the file data and send a chunk at a time
                        stream.Write(fileData, 0, bytes);
                    }
                }
            }
        }
    

    }

    /// <summary>
    /// 用于采集和更新Cookie,用于在HttpWebGetter中设置和采集Response返回的Cookie
    /// </summary>
    public class CookieCollector:IEnumerable<KeyValuePair<string,string>>
    {
        private Dictionary<string, string> _values=new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        public CookieCollector()
        {

        }

        public CookieCollector(IEnumerable<Cookie> initValues)
        {
            if (initValues!=null)
            {
                Update(initValues);
            }
        }

        public void Update(IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var value in values)
            {
                Update(value.Key, value.Value);
            }
        }

        public void Update(CookieCollection cookies)
        {
            foreach (Cookie cookie in cookies)
            {
                Update(cookie);
            }
        }

        public void Update(IEnumerable<Cookie> newCookies)
        {
            if (newCookies!=null)
            {
                foreach (var cookie in newCookies)
                {
                    Update(cookie);
                }
            }
        }

        public void Update(Cookie cookie)
        {
            if (cookie.Expired)
            {
                _values.Remove(cookie.Name);
            }
            else
            {
                _values.AddOrUpdate(cookie.Name, cookie.Value);
            }
        }

        public void Update(string name, string value)
        {
            _values.AddOrUpdate(name, value);
        }

        public void Remove(string name)
        {
            _values.Remove(name);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
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


    public static class CookieExt
    {
        public static Cookie[] GetAllCookies(this CookieContainer cookieJar)
        {
            var lst = new List<Cookie>(cookieJar.Count);

            try
            {
                Hashtable table = (Hashtable)cookieJar
                    .GetType().InvokeMember("m_domainTable",
                    BindingFlags.NonPublic |
                    BindingFlags.GetField |
                    BindingFlags.Instance,
                    null,
                    cookieJar,
                    new object[] { });

                

                foreach (var key in table.Keys)
                {
                    // Look for http cookies.
                    if (cookieJar.GetCookies(
                        new Uri(string.Format("http://{0}/", key))).Count > 0)
                    {
                        //Console.WriteLine(cookieJar.Count + " HTTP COOKIES FOUND:");
                        //Console.WriteLine("----------------------------------");
                        foreach (Cookie cookie in cookieJar.GetCookies(
                            new Uri(string.Format("http://{0}/", key))))
                        {
                            lst.Add(cookie);
                            //Console.WriteLine(
                            //    "Name = {0} ; Value = {1} ; Domain = {2}",
                            //    cookie.Name, cookie.Value, cookie.Domain);
                        }
                    }

                    // Look for https cookies
                    if (cookieJar.GetCookies(
                        new Uri(string.Format("https://{0}/", key))).Count > 0)
                    {
                        //Console.WriteLine(cookieJar.Count + " HTTPS COOKIES FOUND:");
                        //Console.WriteLine("----------------------------------");
                        foreach (Cookie cookie in cookieJar.GetCookies(
                            new Uri(string.Format("https://{0}/", key))))
                        {
                            lst.Add(cookie);
                            //Console.WriteLine(
                            //    "Name = {0} ; Value = {1} ; Domain = {2}",
                            //    cookie.Name, cookie.Value, cookie.Domain);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }

            return lst.ToArray();
        }
    }


}
