using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    /// <summary>
    /// 地址位置相关操作
    /// </summary>
    public class LocationExt
    {
        
        #region 计算距离

        private const double EARTH_RADIUS = 6378.137; //地球半径

        private static double rad(double d)
        {

            return d * Math.PI / 180.0;

        }



        public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
        {

            double radLat1 = rad(lat1);

            double radLat2 = rad(lat2);

            double a = radLat1 - radLat2;

            double b = rad(lng1) - rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +

             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));

            s = s * EARTH_RADIUS;

            s = Math.Round(s * 10000) / 10000;

            return s;

        }

        #endregion
    }
}
