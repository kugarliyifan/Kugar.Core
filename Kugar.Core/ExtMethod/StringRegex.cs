using System.Text.RegularExpressions;
namespace Kugar.Core.ExtMethod
{
    public static class StringRegex
    {
        /// <summary>
        ///     是否符合IP地址的规则
        /// </summary>
        /// <param name="inputstr">输入的源字符串</param>
        /// <returns>如果符合，返回true，否则返回false</returns>
        public static bool IsMatchIPAddress(this string inputstr)
        {
            return GeneralRegex.IsMatchIPAddress(inputstr);
        }

        /// <summary>
        ///     是否符合e-mail的规则
        /// </summary>
        /// <param name="inputstr">输入的源字符串</param>
        /// <returns>如果符合，返回true，否则返回false</returns>
        public static bool IsMatchEMail(this string inputstr)
        {
            return GeneralRegex.IsMatchEMail(inputstr);
        }

        /// <summary>
        ///     是否符合手机号码的规则
        /// </summary>
        /// <param name="inputstr">输入的源字符串</param>
        /// <returns>如果符合，返回true，否则返回false</returns>
        public static bool IsMatchPhoneNumber(this string inputstr)
        {
            return GeneralRegex.IsMobilePhone(inputstr);
        }

        /// <summary>
        ///     是否符合Url的规则
        /// </summary>
        /// <param name="inputstr">输入的源字符串</param>
        /// <returns>如果符合，返回true，否则返回false</returns>
        public static bool IsMatchUrl(this string inputstr)
        {
            return GeneralRegex.IsMatchUrl(inputstr);
        }
    }

    public  class GeneralRegex
    {
        public const string emailRegex = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public const string ipaddressRegex = @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";
        public const string phoneNumRegex = @"\d{3}-\d{8}|\d{4}-\d{7}";
        public const string urlRegex = @"^(?=[^&])(?:(?<scheme>[^:/?#]+):)?(?://(?<authority>[^/?#]*))?(?<path>[^?#]*)(?:\?(?<query>[^#]*))?(?:#(?<fragment>.*))?";
        private static Regex _mobileRegex = new Regex(@"(1[3,4,5,6,7,8,9][0-9])\d{8}$");

        /// <summary>  
        /// 判断输入的字符串是否是一个合法的手机号  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsMobilePhone(string input)
        {
            //Regex regex = new Regex();
            return _mobileRegex.IsMatch(input);

        } 

        public static bool IsMatchIPAddress(string inputstr)
        {
            return checkRegex(inputstr, ipaddressRegex);
        }

        public static bool IsMatchEMail(string inputstr)
        {
            return checkRegex(inputstr, emailRegex);
        }


        public static bool IsMatchUrl(string inputstr)
        {
            return checkRegex(inputstr, urlRegex);
        }



        protected static bool checkRegex(string inputstr,string parent)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(inputstr, parent);
        }




    }
}
