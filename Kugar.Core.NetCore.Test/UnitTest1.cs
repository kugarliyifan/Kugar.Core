using Kugar.Core.BaseStruct;
using Kugar.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kugar.Core.NetCore.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var ret = gets();
        }

        private ResultReturn<(string str1, int int2)> gets()
        {
            return new SuccessResultReturn<(string str1, int int2)>(("stttt",9));
        }

        [TestMethod]
    public void TestUrlModifier()
    {
        var url1 = "http://localhost:6750/product/List?categoryid=1";
        var m=url1.ToUrlModifier();


        var url = "https://www.xxx.com/doc/base/infrastructure.html?";
        var newUrl = url.ToUrlModifier().SetAnchor("topoop").RemoveQuery("keyword").ReplaceQuery("pppp", "iii")
            .AddQuery("o1", "ooo");
        var newUrlStr = newUrl.ToString();

        var test5 = "https://www.xxx.com/doc/base/infrastructure.html?=&o==ppp&&pppp=iii&o1=ooo&test2=&u#topoop"; //出现连续两个=号及连续的&号
        var newUrl5 = test5.ToUrlModifier();
        var newStr5 = newUrl5.ToString();

        var test6 = "/doc/base/infrastructure.html?=&o==ppp&&pppp=iii&o1=ooo&test2=&u#topoop"; //出现连续两个=号及连续的&号
        var newUrl6 = test6.ToUrlModifier();
        var newStr6 = newUrl6.ToString();
    }
    }
}
