using System;
using System.Drawing;
using System.IO;
using Kugar.Core.BaseStruct;
using Kugar.Core.Images;
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
            return new SuccessResultReturn<(string str1, int int2)>(("stttt", 9));
        }

        [TestMethod]

        public void TestTemplateImage()
        {
            var homeShareTemplate =
                new TemplateImage(
                    "F:\\Projects\\StockForecast\\StockForecast\\StockForecast.Web\\uploads\\share\\homeshare.png");

            var t = homeShareTemplate.Generate(new TemplateItemBase[]
            {
                new StringTemplateItem() //日期
                {
                    Location = new Point(136, 191),
                    Font = new Font("Songti SC", 27, FontStyle.Bold, GraphicsUnit.Pixel),
                    Color = Color.FromArgb(0x8e, 0x1a, 0x22),
                    Value = DateTime.Now.ToString("yyyy.MM.dd")
                },
                new StringTemplateItem() //农历
                {
                    Location = new Point(276, 287),
                    MaxWidth = 15,
                    Font = new Font("Songti SC", 16, FontStyle.Bold, GraphicsUnit.Pixel),
                    Color = Color.FromArgb(0x8e, 0x1a, 0x22),
                    Value = "九月初六"
                },
                new StringTemplateItem() //星期
                {
                    Location = new Point(300, 293),
                    MaxWidth = 15,
                    Font = new Font("Songti SC", 12, FontStyle.Bold, GraphicsUnit.Pixel),
                    Color = Color.FromArgb(0x8e, 0x1a, 0x22),
                    Value = "星期一"
                },
                new ImageTemplateItem() //图片
                {
                    Image = (Bitmap) Bitmap.FromFile(
                        "F:\\Projects\\StockForecast\\StockForecast\\StockForecast.Web\\uploads\\weather\\202012\\202012130334171296129.png"),
                    Location = new Point(122, 229),
                    Size = new Size(132, 133)
                },
                new StringTemplateItem()
                {
                    Location = new Point(129, 378),
                    MaxWidth = 125,
                    Font = new Font("Kaiti SC", 14, FontStyle.Bold, GraphicsUnit.Pixel),
                    Color = Color.FromArgb(0x17, 0x14, 0x0e),
                    Value = "sdflsdjfslfjsldfjslfjsldf"
                },
                new StringTemplateItem() //宜
                {
                    Location = new Point(82, 415),
                    Color = Color.FromArgb(0x8f, 0x1A, 0x22),
                    Font = new Font("Songti SC", 20, FontStyle.Bold, GraphicsUnit.Pixel),
                    MaxWidth = 14,
                    Value = "满仓"
                },
                new StringTemplateItem() //忌
                {
                    Location = new Point(274, 415),
                    Color = Color.FromArgb(0x8f, 0x1A, 0x22),
                    Font = new Font("Songti SC", 20, FontStyle.Bold, GraphicsUnit.Pixel),
                    MaxWidth = 14,
                    Value = "不购入"
                },
                new QrCodeTemplateItem() //二维码
                {
                    Location = new Point(229, 542),
                    Size = new Size(73, 72),
                    QrCode = "http://ssssss.com/sdfsdfsdfs/sss"
                }
            });


        }

        [TestMethod]
        public void TestUrlModifier()
        {
            var url1 = "http://localhost:6750/product/List?categoryid=1";
            var m = url1.ToUrlModifier();


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
