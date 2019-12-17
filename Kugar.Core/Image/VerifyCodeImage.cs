using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kugar.Core.ImageExt
{
    /// <summary>
    ///     生成指定文字的校验码
    /// </summary>
    public static class VerifyCodeImage
    {
        private static Random rnd=new Random();

        //字体列表，用于验证码
        private static readonly string[] font ={ "Times New Roman", "MS Mincho"};

        private static readonly Color[] defaultRandomColor=new []{ Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange,Color.Brown, Color.DarkBlue };

        //默认的验证码的字符集，去掉了一些容易混淆的字符
        public static char[] defaultCharacter ={ '2', '3', '4', '5', '6', '8', '9', 'A', 'B', 'C', 'D', 'E','F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };

        //缓冲画笔,避免每次都重新新建一个画笔造成内存碎片
        private static Dictionary<Color,Pen> cachePens=new Dictionary<Color, Pen>();

        private static Dictionary<Color,SolidBrush> cacheSolidBrush=new Dictionary<Color, SolidBrush>();


        /// <summary>
        ///     生成验证码图像用的随机字符串,使用默认的字符集,字符串长度为4
        /// </summary>
        /// <returns></returns>
        public static string GetRandomVerifyCodeString()
        {
            return GetRandomVerifyCodeString(defaultCharacter, 4);
        }

        /// <summary>
        ///     生成验证码图像用的随机字符串
        /// </summary>
        /// <param name="charList">使用的字符集列表</param>
        /// <param name="charLength">生成字符串的长度</param>
        /// <returns></returns>
        public static string GetRandomVerifyCodeString(char[] charList,int charLength)
        {
            if (charLength<1)
            {
                throw new ArgumentOutOfRangeException("charLength");
            }

            if (charList == null || charList.Length<1)
            {
                throw new ArgumentOutOfRangeException("charList");
            }

            var charAr = new char[charLength];


            //生成验证码字符串
            for (int i = 0; i < charLength; i++)
            {
                charAr[i] = charList[rnd.Next(charList.Length)];
            }

            return new string(charAr);
        }

        /// <summary>
        ///     生成指定的验证码图像,指定文字大小,并使用默认的颜色列表
        /// </summary>
        /// <param name="codeStr">要生成的图像</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <param name="fontSize">生成的图像高度</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(string codeStr, int img_width, int img_heigh,int fontSize)
        {
            return GetImgeVerifyCode(codeStr, img_width, img_heigh, defaultRandomColor, fontSize);
        }

        /// <summary>
        ///     生成指定字号的验证码图像,并使用默认的颜色列表
        /// </summary>
        /// <param name="verifyCodeLength">要生成的文字图像的字体个数</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <param name="fontSize">用于生成的字体大小</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(int verifyCodeLength, int img_width, int img_heigh, int fontSize)
        {
            var codeStr = GetRandomVerifyCodeString(defaultCharacter, verifyCodeLength);

            return GetImgeVerifyCode(codeStr, img_width, img_heigh, defaultRandomColor, fontSize);
        }

        /// <summary>
        ///     生成指定的验证码图像,并使用默认的颜色列表,并根据指定的图像宽度与高度,计算合适的字体大小
        /// </summary>
        /// <param name="codeStr">要生成的图像</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(string codeStr, int img_width, int img_heigh)
        {
            return GetImgeVerifyCode(codeStr, img_width, img_heigh, defaultRandomColor, 0);
        }

        /// <summary>
        ///     生成指定的验证码图像,并根据指定的图像宽度与高度,计算合适的字体大小
        /// </summary>
        /// <param name="codeStr">要生成的图像</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <param name="codeColor">用于随机绘制的颜色</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(string codeStr, int img_width, int img_heigh, Color codeColor)
        {
            return GetImgeVerifyCode(codeStr, img_width, img_heigh, new[] { codeColor }, 16);
        }

        /// <summary>
        ///     生成指定的验证码图像,默认文字为16
        /// </summary>
        /// <param name="codeStr">要生成的图像</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <param name="codeColor">用于随机绘制的颜色列表,必须大于2</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(string codeStr, int img_width, int img_heigh, Color[] codeColor)
        {
            return GetImgeVerifyCode(codeStr, img_width, img_heigh, codeColor, 16);
        }

        /// <summary>
        ///     生成指定的验证码图像
        /// </summary>
        /// <param name="codeStr">要生成的图像</param>
        /// <param name="img_width">生成的图像宽度</param>
        /// <param name="img_heigh">生成的图像高度</param>
        /// <param name="codeColor">用于随机绘制的颜色列表,必须大于1</param>
        /// <param name="fontSize">验证码文字的大小,必须大于1</param>
        /// <returns>返回生成的图像</returns>
        public static Image GetImgeVerifyCode(string codeStr, int img_width, int img_heigh, Color[] codeColor, int fontSize)
        {
            if (string.IsNullOrEmpty(codeStr))
            {
                throw new ArgumentOutOfRangeException("codeStr");
            }

            if (codeColor==null || codeColor.Length<1)
            {
                throw new ArgumentOutOfRangeException("codeColor");
            }

            if (img_width<=0)
            {
                throw new ArgumentOutOfRangeException("img_width");
            }

            if (img_heigh <= 0)
            {
                throw new ArgumentOutOfRangeException("img_heigh");
            }

            Bitmap bmp = new Bitmap(img_width, img_heigh);

            Graphics g = Graphics.FromImage(bmp);

            string fnt = font[rnd.Next(font.Length)];

            if (fontSize==0)
            {
                var newFontSize =g.GetFixFontSize(codeStr, fnt, img_width, img_heigh, 20);

                if (newFontSize<=0)
                {
                    throw new ArgumentOutOfRangeException("img_width");
                }

                fontSize =(int) newFontSize;
            }

            if (fontSize < 0)
            {
                throw new ArgumentOutOfRangeException("fontSize");
            }

            g.Clear(Color.White);

            //画噪线
            for (int i = 0; i < 5; i++)
            {
                int x1 = rnd.Next(img_width);
                int y1 = rnd.Next(img_heigh);
                int x2 = rnd.Next(img_width);
                int y2 = rnd.Next(img_heigh);
                Color clr = GetRandomColor(codeColor);
                g.DrawLine(GetCachePen(clr), x1, y1, x2, y2);
            }

            var currentWidth = 2f;

            Font ft = new Font(fnt, fontSize,FontStyle.Bold);
            //g.DrawString(codeStr, ft, GetCacheBrush(clr), currentWidth, (float)6);
            float stry =(float)img_heigh*0.1f;
            //画验证码字符串
            for (int i = 0; i < codeStr.Length; i++)
            {
                Color clr = GetRandomColor(codeColor);
                g.DrawString(codeStr[i].ToString(), ft, GetCacheBrush(clr), currentWidth, stry);
                var width = g.MeasureString(codeStr[i].ToString(), ft).Width;
                currentWidth += width ;
            }

            //画噪点
            for (int i = 0; i < 50; i++)
            {
                int x = rnd.Next(bmp.Width);
                int y = rnd.Next(bmp.Height);
                Color clr = GetRandomColor(codeColor);
                bmp.SetPixel(x, y, clr);
            }

            g.Dispose();

            return bmp;
        }



        ///从缓存中读取Pen
        private static Pen GetCachePen(Color color)
        {
            if (cachePens.ContainsKey(color))
            {
                return cachePens[color];
            }
            else
            {
                var pen = new Pen(color);

                cachePens.Add(color,pen);

                return pen;
            }
        }

        ///从缓存中读取Pen
        private static Brush GetCacheBrush(Color color)
        {
            if (cacheSolidBrush.ContainsKey(color))
            {
                return cacheSolidBrush[color];
            }
            else
            {
                var brush = new SolidBrush(color);

                cacheSolidBrush.Add(color, brush);

                return brush;
            }
        }

        //从颜色列表中读取随机颜色
        private static Color GetRandomColor(Color[] colorList)
        {
            if (colorList.Length==1)
            {
                return colorList[0];
            }

            Color clr = colorList[rnd.Next(colorList.Length)];

            return clr;
        }
    }
}
