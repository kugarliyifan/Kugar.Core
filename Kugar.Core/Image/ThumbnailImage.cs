using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class ThumbnailImage
    {
        /// <summary>
        ///     生成指定图像的缩略图(等比缩放),默认生成的质量为65%
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="width">缩略图的宽度</param>
        /// <param name="hegiht">缩略图的高度</param>
        /// <returns>返回生成之后的图像</returns>
        public static Image GetThumbnailImage(this Image img, int width, int hegiht)
        {
            return GetThumbnailImage(img, width, hegiht, 65);
        }

        /// <summary>
        ///     生成指定图像的缩略图(等比缩放)
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="width">缩略图的宽度</param>
        /// <param name="hegiht">缩略图的高度</param>
        /// <param name="quality">生成的图像的质量,取值范围为1~100,取值越小,生成图像的质量越差</param>
        /// <returns>返回生成之后的图像</returns>
        public static Image GetThumbnailImage(this Image img, int width, int hegiht, int quality)
        {
            var c_width = 0;
            var c_height = 0;

            if (img.Width > 0 && img.Height > 0)
            {
                if (img.Width / img.Height >= width / hegiht)
                {
                    if (img.Width > width)
                    {
                        c_width = width;
                        c_height = (img.Height * width) / img.Width;
                    }
                    else
                    {
                        c_width = img.Width;
                        c_height = img.Height;
                    }
                }
                else
                {
                    if (img.Height > hegiht)
                    {
                        c_height = hegiht;
                        c_width = (img.Width * hegiht) / img.Height;
                    }
                    else
                    {
                        c_width = img.Width;
                        c_height = img.Height;
                    }
                }
            }

            var encParams = new EncoderParameters(1);
            //  建议 65%品质的JPEG格式图片     
            encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            ImageCodecInfo codeInfo = GetEncoder(ImageFormat.Jpeg);
            //ImageCodecInfo[] codeInfos = ImageCodecInfo.GetImageEncoders();

            //foreach (ImageCodecInfo info in codeInfos)
            //{
            //    if (info.MimeType.Equals("image/jpeg "))
            //    {
            //        codeInfo = info;
            //        break;
            //    }
            //}

            Image retValue;

            using (var m_b = new Bitmap(c_width, c_height))
            using (var g2 = Graphics.FromImage(m_b))
            using (var ms = new MemoryStream())
            {
                g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g2.DrawImage(img, new Rectangle(0, 0, c_width, c_height),
                                                new Rectangle(0, 0, img.Width, img.Height),
                                                GraphicsUnit.Pixel);               
 
                if (codeInfo == null)
                {
                    m_b.Save(ms, ImageFormat.Jpeg);
                }
                else
                {
                    m_b.Save(ms, codeInfo, encParams);
                }

                m_b.Dispose();

                retValue = Image.FromStream(ms);

            }

            return retValue;
        }

        public static void GetThumbnailImage(string srcImgPath,string newImgPath, int width, int height, int quality)
        {
            if (!File.Exists(srcImgPath))
            {
                throw new FileNotFoundException("参数srcImgPath指向的路径,查找不到指定名称的问题");
            }

            Bitmap srcImg = null;

            try
            {
                srcImg = new Bitmap(srcImgPath);
            }
            catch (Exception)
            {
                throw new FileLoadException("无法加载指定图像文件");
            }

            var newImg = GetThumbnailImage(srcImg, width, height, quality);


            newImg.Save(newImgPath);
        }


        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var info in codecs)
            {
                if (info.FormatID == format.Guid)
                {
                    return info;
                }
            }

            return null;
        }

    }
}
