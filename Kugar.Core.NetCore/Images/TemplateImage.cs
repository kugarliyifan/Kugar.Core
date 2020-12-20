using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;

namespace Kugar.Core.Images
{

    public class TemplateImageFactory:IDisposable
    {
        private bool _disposing = false;
        private ConcurrentDictionary<string, TemplateImage> _cacheTemplates = new ConcurrentDictionary<string, TemplateImage>();

        
        public TemplateImage GetOrAdd(string templatePath)
        {
            if (_disposing)
            {
                throw new ObjectDisposedException(nameof(TemplateImageFactory));
            }
            
            return _cacheTemplates.GetOrAdd(templatePath, x => new TemplateImage(templatePath));
        }

        public void Remove(string templatePath)
        {
            if (_disposing)
            {
                throw new ObjectDisposedException(nameof(TemplateImageFactory));
            }
            
            if (_cacheTemplates.TryRemove(templatePath, out var tmp))
            {
                tmp.Dispose();
            }
        }
        
        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
            
            foreach (var image in _cacheTemplates)
            {
                image.Value.Dispose();
            }
            
            _cacheTemplates.Clear();
            
        }
    }
    
    /// <summary>
    /// 使用一个bitmap作为模板,后在模板上叠加图片或文字并导出为新的图片,该操作不会影响作为模板的图片
    /// </summary>
    public class TemplateImage:IDisposable
    {
        private Bitmap _templateSource = null;

        public TemplateImage(Bitmap templateSource)
        {
            _templateSource = templateSource;
        }

        public TemplateImage(string templatePath)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(nameof(templatePath));
            }

            using var file = File.OpenRead(templatePath);
            
            _templateSource = (Bitmap) Bitmap.FromStream(file);
            
        }

        public SmoothingMode SmoothingMode { set; get; } = SmoothingMode.AntiAlias;

        public TextRenderingHint TextRenderingHint { set; get; } = TextRenderingHint.AntiAlias;

        public CompositingQuality CompositingQuality { set; get; } = CompositingQuality.HighQuality;

        /// <summary>
        /// 根据传入的数据,套入模板图片,生成新的图片
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Bitmap Generate(TemplateItem[] settings)
        {
            var newImg = _templateSource.Clone(new Rectangle(0, 0, _templateSource.Width, _templateSource.Height),
                PixelFormat.Format32bppArgb);

            using var g = Graphics.FromImage(newImg);
            
            g.SmoothingMode = SmoothingMode;
            g.TextRenderingHint = TextRenderingHint;
            g.CompositingQuality = CompositingQuality;
            
            foreach (var item in settings)
            {
                var location = item.Location;
                
                if (item is ImageTemplateItem img)
                {
                    if (item.Horizontal== HorizontalPosition.Center || item.Vertical== VerticalPosition.Middle)
                    {
                        location = new Point(item.Location.X,item.Location.Y);
                        
                        if (item.Horizontal== HorizontalPosition.Center)
                        {
                            var newx = newImg.Width / 2 - img.Size.Width / 2;
                            location.X = newx;
                        }

                        if (item.Vertical== VerticalPosition.Middle)
                        {
                            var newy= newImg.Height / 2 - img.Size.Height / 2;
                            location.Y = newy;
                        }
                    }
                    
                    g.DrawImage(img.Image,new Rectangle(location,img.Size),new Rectangle(0,0,img.Image.Width,img.Image.Height),GraphicsUnit.Pixel);
                }
                else if (item is StringTemplateItem str)
                {
                    if (item.Horizontal== HorizontalPosition.Center || item.Vertical== VerticalPosition.Middle)
                    {
                        location = new Point(item.Location.X,item.Location.Y);

                        SizeF size;

                        if (str.MaxWidth>0)
                        {
                            size = g.MeasureString(str.Value, str.Font,str.MaxWidth);
                        }
                        else
                        {
                            size = g.MeasureString(str.Value, str.Font);
                        }
                        
                        if (item.Horizontal== HorizontalPosition.Center)
                        {
                            var newx = newImg.Width / 2 - (int)(size.Width / 2);
                            location.X = newx;
                        }

                        if (item.Vertical== VerticalPosition.Middle)
                        {
                            var newy= newImg.Height / 2 - (int)(size.Height / 2);
                            location.Y = newy;
                        }
                    }
                    
                    g.DrawString(str.Value, str.Font,new SolidBrush(str.Color), location);
                }
            }

            return newImg;
        }
                
        public void Dispose()
        {
            _templateSource.Dispose();
        }
    }

    public enum HorizontalPosition
    {
        Custom=-1,
        
        Center=1,
    }

    public enum VerticalPosition
    {
        Custom=-1,
        
        Middle=1
    }
    
    public class TemplateItem 
    {
        public HorizontalPosition  Horizontal { set; get; }
        
        public VerticalPosition Vertical { set; get; }
     
        public Point Location { set; get; }
        
    }
    
    /// <summary>
    /// 传入一个图片
    /// </summary>
    public class ImageTemplateItem:TemplateItem
    {
        public Bitmap Image { set; get; }
        
        public Size Size { set; get; }
    }

    public class StringTemplateItem : TemplateItem
    {
        public string Value { set; get; }
        
        public Font Font { set; get; }
        
        public Color Color { set; get; }= Color.Black;

        /// <summary>
        /// 文本输出的最大宽度,如果为0,则自动,,如果非0,则只用最大宽度,并自动根据最大宽度修改计算字符串所需高度
        /// </summary>
        public int MaxWidth { set; get; } = 0;


    }
}
