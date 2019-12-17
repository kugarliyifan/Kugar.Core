using System.Net;
using System.Web;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.ExtMethod
{
    public static class IPEndPointAbout
    {
        public static IPEndPoint ToIPEndPoint(this object str)
        {
            return ToIPEndPoint(str, new IPEndPoint(IPAddress.Loopback, 1986));
        }

        public static IPEndPoint ToIPEndPoint(this object str, IPEndPoint defaultvalue)
        {
            if (str == null)
            {
                return defaultvalue;
            }

            if (str.ToString().IsNullOrEmpty())
            {
                return defaultvalue;
            }

            return str.ToString().ToIPEndPoint(defaultvalue);

            //var str1 = str.ToString().Split(':');
            //if (str1.Length != 2)
            //{
            //    return defaultvalue;
            //}

            //return new IPEndPoint(str1[0].ToIPAddress(), str1[1].ToInt(1986));

        }

        //IP地址的粘贴
        public static IPAddress ToIPAddress(this object str)
        {
            return ToIPAddress(str, IPAddress.Loopback);
        }

        public static IPAddress ToIPAddress(this object str, IPAddress defaultvalue)
        {
            if (str is IPAddress)
            {
                return (IPAddress) str;
            }

            if (str == null)
            {
                return defaultvalue;
            }

            //IPAddress ia = null;

            //if (str.ToString().IsNullOrEmpty() || !IPAddress.TryParse(str.ToString(), out ia))
            //{
            //    return defaultvalue;
            //}

            return str.ToString().ToIPAddress(defaultvalue);

        }


        public static IPEndPoint ToIPEndPoint(this string str)
        {
            return ToIPEndPoint(str, new IPEndPoint(IPAddress.Loopback, 1986));
        }

        public static IPEndPoint ToIPEndPoint(this string str, IPEndPoint defaultvalue)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultvalue;
            }

            var str1 = str.Split(':');
            if (str1.Length != 2)
            {
                return defaultvalue;
            }

            return new IPEndPoint(str1[0].ToIPAddress(), ObjectAbout.ToInt(str1[1], 1986));

        }

        //IP地址的粘贴
        public static IPAddress ToIPAddress(this string str)
        {
            return ToIPAddress(str, IPAddress.Loopback);
        }

        public static IPAddress ToIPAddress(this string str, IPAddress defaultvalue)
        {
            IPAddress ia = null;

            if (string.IsNullOrEmpty(str) || !IPAddress.TryParse(str, out ia))
            {
                return defaultvalue;
            }

            return ia;
        }

    }
}
