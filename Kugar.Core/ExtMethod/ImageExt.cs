using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


using System.Linq;
using System.Net;
using System.Text;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.ExtMethod
{
    /// <summary>
    ///     图像的常用操作
    /// </summary>
    public static class ImageExt
    {
        /// <summary>
        ///     将位图对象按照指定的格式,保存为Byte数组
        /// </summary>
        /// <param name="img">位图对象</param>
        /// <param name="format">保存格式</param>
        /// <returns></returns>
        public static byte[] SaveToBytes(this Image img, ImageFormat format)
        {
            if (img == null)
            {
                throw new ArgumentNullException("img");
            }

            if (format == null)
            {
                format = ImageFormat.Jpeg;
            }

            using (var ms = new System.IO.MemoryStream(1024))
            {
                img.Save(ms, format);

                return ms.ToArray();
            }
        }

        /// <summary>
        ///      将位图对象按照指定的格式,保存为Byte数组,并输出到指定的缓冲区
        /// </summary>
        /// <param name="img">位图对象</param>
        /// <param name="format">位图格式</param>
        /// <param name="buffer">自定义缓冲区</param>
        /// <param name="offset">缓冲区起始位置的偏移量</param>
        /// <param name="count">缓冲区可用的大小</param>
        /// <returns></returns>
        public static int SaveToBytes(this Image img, ImageFormat format, byte[] buffer, int offset, int count)
        {
            if (img == null)
            {
                throw new ArgumentNullException("img");
            }

            if (format == null)
            {
                format = ImageFormat.Jpeg;
            }

            using (var ms = new BaseStruct.ByteStream(buffer, offset, count))
            {
                img.Save(ms, format);

                return (int)(ms.Position - offset);
            }
        }

        /// <summary>
        ///     从输入的Byte[]数据中,加载位图
        /// </summary>
        /// <param name="bytes">数据数组</param>
        /// <param name="offset">数组起始地址偏移量</param>
        /// <param name="count">数据可用数量</param>
        /// <returns></returns>
        public static Image BuildImage(byte[] bytes, int offset, int count)
        {
            using (var stream = new Bitmap(new ByteStream(bytes, offset, count)))
            {
                return new Bitmap(stream);
            }
            //            var bitmap = new Bitmap(new ByteStream(bytes, offset, count));
            //
            //            return bitmap;
        }

        /// <summary>
        ///     从指定的网址中读取位图图片
        /// </summary>
        /// <param name="uri">图片的目标网址</param>
        /// <returns></returns>
        public static Image BuildImageFromUri(string uri)
        {
            return BuildImageFromUri(new Uri(uri));
        }

        /// <summary>
        ///     从指定的网址中读取位图图片
        /// </summary>
        /// <param name="uri">图片的目标网址</param>
        /// <returns></returns>
        public static Image BuildImageFromUri(Uri uri)
        {
            var data = Core.Network.WebHelper.GetFile(uri);

            if (data != null && data.Data != null)
            {
                return BuildImage(data.Data, 0, data.Data.Length);
            }

            return null;
            //        	
            //        	
            //            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(uri);
            //            var response = request.GetResponse();
            //            try
            //            {
            //                using (var stream =response.GetResponseStream())
            //                {
            //                    if (stream != null)
            //                    {
            //                        var bitmap = new Bitmap(stream);
            //
            //                        return bitmap;
            //                    }
            //                    else
            //                    {
            //                        return null;
            //                    }
            //                }
            //            }
            //            catch (Exception)
            //            {
            //                return null;
            //            }
            //            finally
            //            {
            //                response.Close();
            //            }
        }

        public static void MakeThumbnail(string srcImagePath, string thumbnailPath, int width, int height, ThumbnailType mode = ThumbnailType.HW, ImageFormat imageFormat = null)
        {
            if (imageFormat == null)
            {
                imageFormat = ImageFormat.Jpeg;
            }

            using (var bitmap = Image.FromFile(srcImagePath))
            {
                using (var target = MakeThumbnail(bitmap, width, height, mode))
                {
                    target.Save(thumbnailPath, imageFormat);
                }
            }
        }

        public static Image MakeThumbnail(this Image srcImage, int width, int height, ThumbnailType mode)
        {
            //System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;

            int ow = srcImage.Width;
            int oh = srcImage.Height;

            if (srcImage.Width == width && srcImage.Height == height)
            {
                return srcImage;
            }

            switch (mode)
            {
                case ThumbnailType.HW:
                    {
                        if (srcImage.Width > width || srcImage.Height > height)
                        {
                            // scale down the smaller dimension  
                            if (width * srcImage.Height < height * srcImage.Width)
                            {
                                towidth = width;
                                toheight = (int)Math.Round((decimal)srcImage.Height * width / srcImage.Width);
                            }
                            else
                            {
                                toheight = height;
                                towidth = (int)Math.Round((decimal)srcImage.Width * height / srcImage.Height);
                            }
                        }
                        else
                        {
                            towidth = srcImage.Width;
                            toheight = srcImage.Height;
                        }

                        x = (int)Math.Round((width - towidth) / 2.0);
                        y = (int)Math.Round((height - toheight) / 2.0);
                    }   
                    break;
                case ThumbnailType.W:
                    toheight = srcImage.Height * width / srcImage.Width;
                    break;
                case ThumbnailType.H:
                    towidth = srcImage.Width * height / srcImage.Height;
                    break;
                case ThumbnailType.Cut:
                    if ((double)srcImage.Width / (double)srcImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = srcImage.Height;
                        ow = srcImage.Height * towidth / toheight;
                        y = 0;
                        x = (srcImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = srcImage.Width;
                        oh = srcImage.Width * height / towidth;
                        x = 0;
                        y = (srcImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片 
            Image bitmap = new Bitmap(towidth, toheight);

            //新建一个画板 
            Graphics g = Graphics.FromImage(bitmap);

            //设置高质量插值法 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(srcImage, new Rectangle(0, 0, towidth, toheight),
                new Rectangle(x, y, ow, oh),
                GraphicsUnit.Pixel);

            try
            {
                return bitmap;

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                srcImage.Dispose();
                g.Dispose();
            }
        }

    }

    /// <summary>
    /// 缩略图缩放类型
    /// </summary>
    public enum ThumbnailType
    {
        /// <summary>
        /// 指定高宽缩放（不变形）
        /// </summary>
        HW = 0,

        /// <summary>
        /// //指定宽，高按比例    
        /// </summary>
        W = 1,

        /// <summary>
        /// //指定高，宽按比例 
        /// </summary>
        H = 2,

        /// <summary>
        /// //指定高宽裁减（不变形）    
        /// </summary>
        Cut = 3
    }
}
