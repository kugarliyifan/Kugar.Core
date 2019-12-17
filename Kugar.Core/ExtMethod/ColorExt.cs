using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class ColorExt
    {
        /// <summary>
        /// Color转html颜色分组
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToHtmlColor(this Color color)
        {
            return "#" + color.R.ToString("x2") + color.G.ToString("x2") + color.B.ToString("x2");
        }
    }
}
