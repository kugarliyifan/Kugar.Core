using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{
    /// <summary>
    ///     一个简单的字符串模板处理类,用于替换字符串中指定格式的数据<br/>
    ///    指定的格式为: {关键字名称,'格式化字符串','默认值'}  <br/>
    ///                    注:格式化字符串与默认值为可选,,但顺序不能改变,例如,如果不想设置格式化字符串,但想设置默认值,则格式为:{关键字名称,'','默认值'} //逗号与两个单引号不能忽略<br/>
    ///                        默认值段为可忽略,即格式可以为 {关键字名称,'格式化字符串'}<br/>
    ///                        如果两个都想忽略,则格式为 {关键字名称}<br/>
    ///                        关键字中,不允许出现 逗号,单引号
    /// </summary>
    public class StringFormatTemplateEngine
    {
        private static Regex regex = new Regex(@"\{(.*?)\}");

        /// <summary>
        ///     替换字符串中指定的数据,数据来源于getValueFunc指定的函数
        /// </summary>
        /// <param name="srcStr">源字符串</param>
        /// <param name="getValueFunc">读取数据用的函数</param>
        /// <param name="state">传给函数的自定义参数,如不需要,可以为空</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatString(string srcStr, FormatterGetValueFunc getValueFunc,object state)
        {
            if (getValueFunc==null)
            {
                return srcStr;
            }

            var mc = regex.Matches(srcStr);

            if (mc.Count>0)
            {
                var tempDic = new Dictionary<string, string>();
                string keyName;
                string formatStr;
                string defaultValue;
                
                foreach (Match m in mc)
                {
                    if (m.Groups.Count<=1 || tempDic.ContainsKey(m.Groups[0].Value))
                    {
                        continue;
                    }

                    string realValue = string.Empty;

                    try
                    {
                        filterKeyInfo(m.Groups[1].Value, out keyName, out formatStr, out defaultValue);

                        var v = getValueFunc(keyName, state);

                        if (v == null)
                        {
                            realValue = defaultValue;
                        }
                        else
                        {
                            if (v is IFormattable)
                            {
                                realValue = ((IFormattable)v).ToString(formatStr, null);
                            }
                            else
                            {
                                realValue = v.ToStringEx();
                            }
                        }

                        
                    }
                    catch (Exception)
                    {
                    }


                    tempDic.Add(m.Groups[0].Value, realValue);



                }

                if (tempDic.Count>0)
                {
                    foreach (var keyValue in tempDic)
                    {
                        srcStr = srcStr.Replace(keyValue.Key, keyValue.Value);
                    }
                }
            }

            return srcStr;
        }

        /// <summary>
        ///     替换字符串中指定的数据,数据来源于valueCollection指定的字典类,,valueCollection中,key为关键字,value为该关键字对应的值
        /// </summary>
        /// <param name="srcStr">源字符串</param>
        /// <param name="valueCollection">字典类的数据源</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatString(string srcStr,Dictionary<string,object> valueCollection)
        {
            return FormatString(srcStr, getDictionaryValue, valueCollection);
        }

        private static object getDictionaryValue(string keyName, object state)
        {
            if (state==null || !(state is Dictionary<string,object>))
            {
                return null;
            }

            var valueCollection = (Dictionary<string, object>) state;

            if (valueCollection != null && valueCollection.Count > 0)
            {
                return valueCollection.ContainsKey(keyName)
                               ? valueCollection[keyName]
                               : null;
            }
            else
            {
                return null;
            }

        }

        private static void filterKeyInfo(string srcStr,out string keyName,out string formatStr,out string defaultValue)
        {
            keyName = string.Empty;
            formatStr = string.Empty;
            defaultValue = string.Empty;

            if (srcStr.Length <= 0)
            {
            //    keyName = string.Empty;
            //    formatStr = string.Empty;
            //    defaultValue = string.Empty;
                return;
            }

            var tempSrcStr = srcStr.Trim(' ', '{', '}');

            var keyIndex = tempSrcStr.IndexOf(',');

            if (keyIndex < 0)
            {
                keyName = tempSrcStr;
                //formatStr = string.Empty;
                //defaultValue = string.Empty;
                return;
            }
            else
            {
                keyName = tempSrcStr.Left(keyIndex);
            }
            

            var fStrStartIndex = tempSrcStr.IndexOf('\'', keyIndex);

            var fStrEndIndex = tempSrcStr.IndexOf(',', fStrStartIndex + 1);

            if (fStrEndIndex<0)
            {
                fStrEndIndex = tempSrcStr.LastIndexOf('\'');
            }
            else
            {
                fStrEndIndex = tempSrcStr.LastIndexOf('\'', fStrEndIndex);
            }
            

            if (fStrEndIndex == -1 || fStrStartIndex == -1)
            {
                //formatStr = string.Empty;
                //defaultValue = string.Empty;
                return;
            }

            if (fStrEndIndex <= fStrStartIndex + 1)
            {
                formatStr = string.Empty;
            }
            else
            {
                formatStr = tempSrcStr.Substring(fStrStartIndex + 1, fStrEndIndex - fStrStartIndex - 1).Trim();
            }

            var dvStartIndex = tempSrcStr.IndexOf(',', fStrEndIndex);

            if (dvStartIndex != -1)
            {
                defaultValue = tempSrcStr.Substring(dvStartIndex + 1).Trim(' ','\'');
            }
            //else
            //{
            //    defaultValue = string.Empty;
            //}


        }

        public delegate object FormatterGetValueFunc(string keyName, object state);
    }
}
