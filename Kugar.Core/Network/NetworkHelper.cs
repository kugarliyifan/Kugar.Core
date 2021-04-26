using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Net;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Network
{
    public static class NetworkHelper
    {
        //private static System.Net.NetworkInformation.Ping pingServer=new System.Net.NetworkInformation.Ping();


        private static Uri ip138Uri = new Uri("http://www.ip138.com/ips138.asp");

        /// <summary>
        ///     获取当前计算机的外网IP地址
        /// </summary>
        /// <returns>返回外网的ip地址,,,当网络未能访问外网时返回null</returns>
        public static IPAddress GetWanIPAddress()
        {
            //if (!IsConnectedToInternet())
            //{
            //    return null;
            //}
            string all = "";
                
            WebRequest wr = WebRequest.Create(ip138Uri);
            var ip = String.Empty;
            try
            {
                wr.Timeout = 5000; //设置10秒超时
                
                using (var response = wr.GetResponse())
                {
                    if (response.ContentLength <= 0)
                    {
                        return null;
                    }
                    using (var s = wr.GetResponse().GetResponseStream())
                    {
                        if (s == null)
                        {
                            return null;
                        }

                        var sr = new StreamReader(s, Encoding.Default);

                        all = sr.ReadToEnd(); //读取网站的数据

                        s.Close();
                    }

                    response.Close();
                }

                if (string.IsNullOrEmpty(all))
                {
                    return null;
                }

                var bodyIndex = all.IndexOf("<body>",StringComparison.CurrentCultureIgnoreCase);

                var i = all.IndexOf("[", bodyIndex) + 1;

                var i2 = all.IndexOf("]", i + 1);

                var tempip = all.Substring(i, i2-i);



                ip = tempip.Replace("]", "").Replace(" ", "");

                if (ip.IsNullOrEmpty())
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            IPAddress wanIP;

            if (IPAddress.TryParse(ip, out wanIP))
            {
                return wanIP;
            }
            else
            {
                return null;
            }

        }

        /// <summary>获取可用的IP地址</summary>
        /// <returns></returns>
        public static IEnumerable<IPAddress> GetIPs()
        {
            var dic = new Dictionary<UnicastIPAddressInformation, Int32>();
            foreach (var item in GetActiveInterfaces())
            {
                if (item != null && item.UnicastAddresses.Count > 0)
                {
                    var gw = item.GatewayAddresses.Count;
                    foreach (var elm in item.UnicastAddresses)
                    {
                        try
                        {
                            if (elm.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred) continue;
                        }
                        catch { }

                        dic.Add(elm, gw);
                    }
                }
            }

            // 带网关的接口地址很重要，优先返回
            // Linux下不支持PrefixOrigin
            var ips = dic.OrderByDescending(e => e.Value)
                //.ThenByDescending(e => e.Key.PrefixOrigin == PrefixOrigin.Dhcp || e.Key.PrefixOrigin == PrefixOrigin.Manual)
                .Select(e => e.Key.Address).ToList();

            return ips;
        }

        public static IEnumerable<IPInterfaceProperties> GetActiveInterfaces()
        {
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus != OperationalStatus.Up) continue;

                var ip = item.GetIPProperties();
                if (ip != null) yield return ip;
            }
        }

        //#if NET45
        //        /// <summary>
        //        ///     判断当前系统是否已连接互联网
        //        /// </summary>
        //        /// <returns></returns>
        //        public static bool IsConnectedToInternet()
        //        {
        //            int desc;
        //            return Api.InternetGetConnectedState(out desc, 0);
        //        }      
        //  #endif


        //public static IPAddress[] GetLanIPAddress()
        //{
        //    var s = NetworkInterface.GetAllNetworkInterfaces().SelectMany(x=>x.GetIPProperties().);

        //    s.Where(x=>x.GetIPProperties().UnicastAddresses[0].Address)
        //}

        //public static int BeginGetMTU(IPAddress testUrl, WaitCallback callback)
        //{
        //    using (System.Net.NetworkInformation.Ping PingSender = new System.Net.NetworkInformation.Ping())
        //    {
        //        PingOptions Options = new PingOptions();
        //        Options.DontFragment = true;

        //        var isSuccess = false;

        //        var mtuLst = new List<int>();

        //        PingSender.PingCompleted += (s, e) =>
        //                                    {
        //                                        if (e.Reply.Status == IPStatus.Success)
        //                                        {
        //                                            mtuLst.Add((int)e.UserState);
        //                                        }
        //                                    };

        //        for (int i = 1400; i < 1500; i++)
        //        {
        //            var buff = new byte[i];
        //            PingSender.SendAsync(testUrl, 2000, buff, Options, i);

        //        }


        //        if (Reply.Status == IPStatus.Success)
        //            return true;
        //        return false;
        //    }
        //}
    }
}
