using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kugar.Core.ImageExt
{
    public static class ImageExt
    {
        public static float GetFixFontSize( this Graphics graphics,string text,string fontName,float width,float height,int maxFontSize)
        {
            var newFontSize = maxFontSize;
            var currentFont = new Font(fontName, newFontSize);
            SizeF currentFontSize = graphics.MeasureString(text, currentFont);
            while (currentFontSize.Width>width || currentFontSize.Height>height)
            {
                newFontSize -= 1;

                if (newFontSize<1)
                {
                    break;
                }


                currentFontSize = graphics.MeasureString(text, new Font(fontName, newFontSize));


            }

            return newFontSize;
        }
    }
}
