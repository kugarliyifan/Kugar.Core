using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using QRCoder;

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
        private Stream _sourceStream = null;
        private FileSystemWatcher _wather = null;

        public TemplateImage(Bitmap templateSource)
        {
            _templateSource = templateSource;
        }

        /// <summary>
        /// 模板图片的构造函数
        /// </summary>
        /// <param name="templatePath">模板图片文件绝对路径</param>
        /// <param name="isWatchFileModify">是否自动监控文件,当文件有变动时,自动重新加载模板文件
        /// </param>
        public TemplateImage(string templatePath,bool isWatchFileModify=true)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(nameof(templatePath));
            }

            using var file = File.OpenRead(templatePath);

            var data = file.ReadAllBytes();

            var s1 = new ByteStream(data);
            _sourceStream = s1;
            _templateSource = (Bitmap) Bitmap.FromStream(s1);

            if (isWatchFileModify)
            {
                _wather = new FileSystemWatcher(templatePath);
                _wather.EnableRaisingEvents = true;
                _wather.Changed += wather_changed;

            }
        }

        private void wather_changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType== WatcherChangeTypes.Created )
            {
                using var file = File.OpenRead(e.FullPath);

                var data = file.ReadAllBytes();

                var oldValue = _sourceStream;
                var templateSource = _templateSource;
                var s1 = new ByteStream(data);
                var newTemplateSource = (Bitmap) Bitmap.FromStream(s1);
                
                _sourceStream = s1;
                _templateSource = newTemplateSource;
                
                oldValue.Close();
                oldValue.Dispose();
                templateSource.Dispose();
            }
        }
        

        public SmoothingMode SmoothingMode { set; get; } = SmoothingMode.AntiAlias;

        public TextRenderingHint TextRenderingHint { set; get; } = TextRenderingHint.AntiAlias;

        public CompositingQuality CompositingQuality { set; get; } = CompositingQuality.HighQuality;

        /// <summary>
        /// 根据传入的数据,套入模板图片,生成新的图片
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Bitmap Generate(TemplateItemBase[] settings)
        {
            var newImg = (Bitmap)_templateSource.Clone();

            var g1 = Graphics.FromImage(_templateSource);
            
            try
            {
                using (var g = Graphics.FromImage(newImg))
                {
                    g.SmoothingMode = SmoothingMode;
                    g.TextRenderingHint = TextRenderingHint;
                    g.CompositingQuality = CompositingQuality;
            
                    foreach (var item in settings)
                    {
                        item.Draw(g, newImg.Size);
                    }

                    return newImg;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
                
        public void Dispose()
        {
            _templateSource.Dispose();
            
            _sourceStream?.Close();
            _sourceStream?.Dispose();
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
    
    public abstract class TemplateItemBase
    {
        /// <summary>
        /// 水平方向对其方式,默认为Custom,使用Location定位
        /// </summary>
        public HorizontalPosition Horizontal { set; get; } = HorizontalPosition.Custom;

        /// <summary>
        /// 垂直方向对其方式,默认为Custom,使用Location定位
        /// </summary>
        public VerticalPosition Vertical { set; get; } = VerticalPosition.Custom;
     
        /// <summary>
        /// 输出项定位
        /// </summary>
        public Point Location { set; get; }

        public abstract void Draw(Graphics graphics,Size newBitmapSize);

    }
    
    /// <summary>
    /// 传入一个图片
    /// </summary>
    public class ImageTemplateItem:TemplateItemBase
    {
        /// <summary>
        /// 图片信息
        /// </summary>
        public Bitmap Image { set; get; }
        
        /// <summary>
        /// 图片输出到模板图的时候的大小
        /// </summary>
        public Size Size { set; get; }
        
        public override void Draw(Graphics graphics,Size newBitmapSize)
        {
            var location = this.Location;
            
            if (this.Horizontal== HorizontalPosition.Center || this.Vertical== VerticalPosition.Middle)
            {
                location = new Point(this.Location.X,this.Location.Y);
                        
                if (this.Horizontal== HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - this.Size.Width / 2;
                    
                    location.X = newx;
                }

                if (this.Vertical== VerticalPosition.Middle)
                {
                    var newy= newBitmapSize.Height / 2 - this.Size.Height / 2;
                    location.Y = newy;
                }
            }
                    
            graphics.DrawImage(Image,new Rectangle(location,this.Size),new Rectangle(0,0,this.Image.Width,this.Image.Height),GraphicsUnit.Pixel);
            
        }
    }

    /// <summary>
    /// 二维码项
    /// </summary>
    public class QrCodeTemplateItem : TemplateItemBase
    {
        /// <summary>
        /// 二维码内实际存储的字符数据
        /// </summary>
        public string QrCode { set; get; }
        
        /// <summary>
        /// 二维码中心的icon图标
        /// </summary>
        public Bitmap Icon { set; get; }
        
        /// <summary>
        /// 二维码尺寸
        /// </summary>
        public Size Size { set; get; }

        /// <summary>
        /// 容错级别,默认为M
        /// </summary>
        public QRCodeGenerator.ECCLevel ECCLevel { set; get; } = QRCodeGenerator.ECCLevel.M;
        
        public override void Draw(Graphics graphics,Size newBitmapSize)
        {
            var location = this.Location;
            
            if (this.Horizontal== HorizontalPosition.Center || this.Vertical== VerticalPosition.Middle)
            {
                location = new Point(this.Location.X,this.Location.Y);
                        
                if (this.Horizontal== HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - this.Size.Width / 2;
                    
                    location.X = newx;
                }

                if (this.Vertical== VerticalPosition.Middle)
                {
                    var newy= newBitmapSize.Height / 2 - this.Size.Height / 2;
                    location.Y = newy;
                }
            }
            
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(QrCode,ECCLevel))
            using (QRCode qrCode = new QRCode(qrCodeData))
            using (Bitmap qrCodeImage = qrCode.GetGraphic(20,Color.Black,Color.White,Icon))
            {
                graphics.DrawImage(qrCodeImage,new Rectangle(location,this.Size),new Rectangle(0,0,qrCodeImage.Width,qrCodeImage.Height),GraphicsUnit.Pixel);
                
            }
        }
    }

    /// <summary>
    /// 普通字符串项
    /// </summary>
    public class StringTemplateItem : TemplateItemBase
    {
        /// <summary>
        /// 文本字符串值
        /// </summary>
        public string Value { set; get; }
        
        /// <summary>
        /// 字体信息
        /// </summary>
        public Font Font { set; get; }
        
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color Color { set; get; }= Color.Black;

        /// <summary>
        /// 文本输出的最大宽度,如果为0,则自动,,如果非0,则只用最大宽度,并自动根据最大宽度修改计算字符串所需高度
        /// </summary>
        public int MaxWidth { set; get; } = 0;

        /// <summary>
        /// 字符串输出参数
        /// </summary>
        /// <example>
        /// 如纵向输出:
        /// new StringFormat(StringFormatFlags.DirectionVertical)
        /// 
        /// </example>
        public StringFormat StringFormat { set; get; }

        public override void Draw(Graphics graphics,Size newBitmapSize)
        {
            var location = this.Location;
            SizeF size=default(Size);
            if (this.Horizontal== HorizontalPosition.Center || this.Vertical== VerticalPosition.Middle)
            {
                location = new Point(this.Location.X,this.Location.Y);
                
                if (this.MaxWidth>0)
                {
                    size = graphics.MeasureString(this.Value, this.Font,this.MaxWidth);
                }
                else
                {
                    size = graphics.MeasureString(this.Value, this.Font);
                }
                        
                if (this.Horizontal== HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - (int)(size.Width / 2);
                    location.X = newx;
                }

                if (this.Vertical== VerticalPosition.Middle)
                {
                    var newy= newBitmapSize.Height / 2 - (int)(size.Height / 2);
                    location.Y = newy;
                }
            }
            else if(MaxWidth>0)
            {
                size = graphics.MeasureString(this.Value, this.Font,this.MaxWidth);
            }

            if (MaxWidth>0)
            {
                graphics.DrawString(this.Value, this.Font,new SolidBrush(this.Color), new RectangleF(location,size),StringFormat);
            }
            else
            {
                graphics.DrawString(this.Value, this.Font,new SolidBrush(this.Color), location,StringFormat);
            }
            
            
        }
    }
}
