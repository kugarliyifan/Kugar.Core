using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Network.RSS
{
    public class RssReader
    {
        public static RssItem[] ReadRssItem(string rssUrl)
        {
            //使用一个字符串rssURL作为它的参数。这个字符串包含了RSS的URL。它使用rssURL的值建立了一个WebRequest项
            var myRequest = System.Net.WebRequest.Create(rssUrl);

            //请求的响应将会被放到一个WebResponse对象里
            var myResponse = myRequest.GetResponse();

            //这个WebResponse对象被用来建立一个流来取出XML的值
            var rssStream = myResponse.GetResponseStream();

            if (rssStream == null || !rssStream.CanRead)
            {
                return null;
            }

            //使用一个XmlDocument对象来存储流中的XML内容。XmlDocument对象用来调入XML的内容
            var rssDoc = new System.Xml.XmlDocument();
            rssDoc.Load(rssStream);

            //个项应该在rss/channel/里。使用XPath表达，一个项节点列表可以如下方式创建
            var rssItems = rssDoc.SelectNodes("rss/channel/item");

            if (rssItems == null)
            {
                return null;
            }

            var resLst = new List<RssItem>(rssItems.Count);

            for (int i = 0; i < rssItems.Count; i++)
            {
                var item = new RssItem();

                System.Xml.XmlNode rssDetail;

                rssDetail = rssItems[i].SelectSingleNode("title");
                item.Title = rssDetail != null ? rssDetail.InnerText : "";

                rssDetail = rssItems[i].SelectSingleNode("link");
                item.Link = rssDetail != null ? rssDetail.InnerText : "";

                rssDetail = rssItems[i].SelectSingleNode("description");
                item.Description = rssDetail != null ? rssDetail.InnerText : "";

                resLst.Add(item);
            }

            return resLst.ToArray();
        }
    }

    public class RssItem
    {
        public string Title { set; get; }
        public string Link { set; get; }
        public string Description { set; get; }
    }
}
