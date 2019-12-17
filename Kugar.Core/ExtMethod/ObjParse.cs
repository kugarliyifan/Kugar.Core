using System.Collections.Generic;

namespace Kugar.Core.ExtMethod
{
    //值类型相关


    public static class ListTypeAbout
    {

    }


    //public static class ValueTypeAbout
    //{
    //    public static bool IsNum(this object str)
    //    {
    //        if (str==null)
    //        {
    //            return false;
    //        }

    //        decimal temp;

    //        return decimal.TryParse(str.ToString(), out temp);

    //    }

    //    public static int ToInt(this object str)
    //    {
    //        return ToInt(str, 0);
    //    }

    //    public static int ToInt(this object str, int defaultvalue)
    //    {
    //        if (str==null)
    //        {
    //            return defaultvalue;
    //        }

    //        int t;

    //        return str.ToString().ToInt(defaultvalue);


    //        //int tempindex = -1;
    //        //tempindex = str.ToString().IndexOf('.');

    //        //var strvalue = str.ToString();

    //        //if (tempindex!=-1)
    //        //{
    //        //    strvalue = strvalue.Substring(tempindex - 1, tempindex);
    //        //}

    //        //if (int.TryParse(strvalue, out t))
    //        //{
    //        //    return t;
    //        //}
    //        //else
    //        //{
    //        //    return defaultvalue;
    //        //}
    //    }

    //    public static int ToMinInt(this object str ,int minvalue)
    //    {
    //        int i = ToInt(str);
    //        if (i<minvalue)
    //        {
    //            return minvalue;
    //        }

    //        return i;
    //    }

    //    public static byte ToByte(this object str)
    //    {
    //        return ToByte(str, 0);
    //    }

    //    public static byte ToByte(this object str, byte defaultvalue)
    //    {
    //        byte temp;



    //        if (str!=null && byte.TryParse(str.ToStringEx() , out temp))
    //        {
    //            return temp;
    //        }
    //        return defaultvalue;
    //    }

    //    public static double ToDouble(this object str)
    //    {
    //        return ToDouble(str, 0);
    //    }

    //    public static double ToDouble(this object str, double defaultvalue)
    //    {
    //        if (str==null)
    //        {
    //            return defaultvalue;
    //        }

    //        Double t;

    //        if (Double.TryParse(str.ToString(), out t))
    //        {
    //            return t;
    //        }
    //        else
    //        {
    //            return defaultvalue;
    //        }
    //    }

    //    public static decimal ToDecimal(this object str)
    //    {
    //        return ToDecimal(str, 0);
    //    }

    //    public static decimal ToDecimal(this object str, decimal defaultvalue)
    //    {
    //        if (str==null)
    //        {
    //            return defaultvalue;
    //        }

    //        decimal t;

    //        if (decimal.TryParse(str.ToString(), out t))
    //        {
    //            return t;
    //        }
    //        else
    //        {
    //            return defaultvalue;
    //        }
    //    }

    //    //Bool粘贴
    //    public static bool ToBool(this object str)
    //    {
    //        return ToBool(str, false);
    //    }

    //    public static bool ToBool(this object str, bool defaultvalue)
    //    {
    //        if (str==null)
    //        {
    //            return defaultvalue;
    //        }

    //        bool b;
    //        if (bool.TryParse(str.ToString(), out b))
    //        {
    //            return b;
    //        }
    //        else
    //        {
    //            var i = 0;

    //            if(int.TryParse(str.ToStringEx(),out i))
    //            {
    //                if (i==0)
    //                {
    //                    return false;
    //                }
    //                else
    //                {
    //                    return true;
    //                }
    //            }
    //            else
    //            {
    //                return defaultvalue;
    //            }

    //        }
    //    }


    //    public static string ToStringEx(this object obj)
    //    {
    //        return ToStringEx(obj, string.Empty);
    //    }

    //    public static string ToStringEx(this object obj, string defaultvalue)
    //    {
    //        if (obj == null)
    //        {
    //            return defaultvalue;
    //        }

    //        return obj.ToString();
    //    }

    //    /// <summary>
    //    ///     转换到float型
    //    /// </summary>
    //    /// <param name="str"></param>
    //    /// <returns></returns>
    //    public static float ToFloat(this object str)
    //    {
    //        return ToFloat(str, 0);
    //    }

    //    public static float ToFloat(this object str, float defaultvalue)
    //    {
    //        float t;

    //        if (float.TryParse(str.ToStringEx(""), out t))
    //        {
    //            return t;
    //        }
    //        else
    //        {
    //            return defaultvalue;
    //        }
    //    }


    //    public static DateTime ToDateTime(this object str)
    //    {
    //        return ToDateTime(str, DateTime.MinValue);
    //    }

    //    public static DateTime ToDateTime(this object str, DateTime defaultvalue)
    //    {
    //        DateTime d;

    //        if (DateTime.TryParse(str.ToString(), out d))
    //        {
    //            return d;
    //        }
    //        else
    //        {
    //            return defaultvalue;
    //        }

    //    }


    //    /// <summary>
    //    ///     转换到float型
    //    /// </summary>
    //    /// <param name="str"></param>
    //    /// <returns></returns>
    //    public static UInt16 ToUInt16(this object str)
    //    {
    //        return ToUInt16(str, 0);
    //    }

    //    public static UInt16 ToUInt16(this object str, UInt16 defaultvalue)
    //    {
    //        UInt16 t;

    //        if (UInt16.TryParse(str.ToStringEx(""), out t))
    //        {
    //            return t;
    //        }
    //        else
    //        {
    //            return defaultvalue;
    //        }
    //    }

    //    public static string ToStringEx(this int n)
    //    {
    //        return ToStringEx(n, 10);
    //    }

    //    public static string ToStringEx(this int n, int jinzhi)
    //    {
    //        return Convert.ToString(n, jinzhi).ToUpper();
    //    }

    //    /// <summary>
    //    ///     返回指定int的第index位是1还是0,1返回true,0返回false
    //    /// </summary>
    //    /// <param name="i"></param>
    //    /// <param name="index">第index位</param>
    //    /// <returns></returns>
    //    public static bool GetBit(this int i, int index)
    //    {
    //        if (index > 32)
    //        {
    //            return false;
    //        }

    //        int p = (int)Math.Pow(2, index);

    //        if ((i & p) == p)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    ///     返回指定ushort的第index位是1还是0,1返回true,0返回false
    //    /// </summary>
    //    /// <param name="i"></param>
    //    /// <param name="index">第index位</param>
    //    /// <returns></returns>
    //    public static bool GetBit(this UInt16 i, ushort index)
    //    {
    //        if (index > 16)
    //        {
    //            return false;
    //        }

    //        ushort p = (ushort)Math.Pow(2, index);

    //        if ((i & p) == p)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    public static bool IsEquals(this float n, float value)
    //    {
    //        if ((n - value) < (Math.Pow(10, -6)))
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }




    //    public static int DivRem(this int n, int divnum)
    //    {
    //        int i = 0;

    //        Math.DivRem(n, divnum, out i);

    //        return i;
    //    }

    //    public static long DivRem(this long n, long divnum)
    //    {
    //        long i = 0;

    //        Math.DivRem(n, divnum, out i);

    //        return i;
    //    }



    //    public static float Round(this float n, int count)
    //    {
    //        return (float)Math.Round(n, count);
    //    }

    //    public static double Round(this double n, int count)
    //    {
    //        return Math.Round(n, count);
    //    }

    //    public static float Pow(this float n, int num)
    //    {
    //        return (float)Math.Pow(n, num);
    //    }






    //    public static string JoinToString(this byte[] n)
    //    {
    //        if (n == null || n.Length <= 0)
    //        {
    //            return string.Empty;
    //        }

    //        return JoinToString(n, ' ');
    //    }

    //    public static string JoinToString(this byte[] n, char splite)
    //    {
    //        if (n == null || n.Length <= 0)
    //        {
    //            return string.Empty;
    //        }

    //        return JoinToString(n, splite, 0, n.Length);
    //    }

    //    public static string JoinToString(this byte[] n, char splite, int start, int length)
    //    {
    //        return JoinToString(n, splite, start, length, "X2");
    //    }

    //    public static string JoinToString(this byte[] n, char splite, int start, int length,string strformat)
    //    {
    //        if (n == null || n.Length <= 0 || start > n.Length || start < 0 || length <= 0 || start + length > n.Length)
    //        {
    //            return string.Empty;
    //        }

    //        var s = new StringBuilder(length * 2);

    //        for (int i = start; i < start + length; i++)
    //        {
    //            s.Append(n[i].ToString(strformat) + splite);                
    //        }

    //        s=s.Remove(s.Length-1, 1);

    //        return s.ToStringEx();
    //    }

    //    public static string JoinToString(this Array n)
    //    {
    //        if (n == null || n.Length <= 0)
    //        {
    //            return string.Empty;
    //        }

    //        return JoinToString(n, ' ');
    //    }

    //    public static string JoinToString(this Array n, char splite)
    //    {
    //        if (n == null || n.Length <= 0)
    //        {
    //            return string.Empty;
    //        }

    //        return JoinToString(n, splite, 0, n.Length);
    //    }

    //    public static string JoinToString(this Array n, char splite, int start, int length)
    //    {
    //        if (n == null || n.Length <= 0 || start > n.Length || start < 0 || length <= 0 || start + length > n.Length)
    //        {
    //            return string.Empty;
    //        }

    //        var s = new StringBuilder(n.Length * 2);

    //        for (int i = start; i < length; i++)
    //        {
    //            s.Append(n.GetValue(i).ToStringEx() + splite);
    //        }

    //        return s.ToStringEx();
    //    }


    //}



    public static class KugarGlobalFunction
    {
        //public static void Swap(this object first,object second)
        //{
        //    object temp = first;

        //    first = second;

        //    second = temp;
        //}


    }

}

//namespace Kugar.ExtMethod.ControlAbout
//{
//    public static class ControlAbout
//    {
//        public static void RunAsyncDelegate(this Control ctrl, MethodInvoker fAsyncDelegate)
//        {
//            RunAsyncDelegate(ctrl, fAsyncDelegate, null);
//        }

//        public static void RunAsyncDelegate(this Control ctrl, MethodInvoker fAsyncDelegate, MethodInvoker fCallBackDelegate)
//        {
//            using (var backgroundWorker = new System.ComponentModel.BackgroundWorker())
//            {
//                backgroundWorker.DoWork += new DoWorkEventHandler(delegate(object o, DoWorkEventArgs workerEventArgs)
//                {
//                    if (fAsyncDelegate != null)
//                        fAsyncDelegate.Invoke();
//                });

//                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object o, RunWorkerCompletedEventArgs e)
//                {
//                    if (fCallBackDelegate != null)
//                        ctrl.BeginInvoke(fCallBackDelegate);
//                });

//                backgroundWorker.RunWorkerAsync();
//            }
//        } 
       
//        /// <summary>
//        ///     计算当前控件置于制定控件的垂直居中的top值
//        /// </summary>
//        /// <param name="ctrl"></param>
//        /// <param name="rect"></param>
//        /// <returns></returns>
//        public static int GetVerticalMiddle(this Control ctrl, Control rect)
//        {
//            if (ctrl==null)
//            {
//                return 0;
//            }

//            int temp = 0;

//            if (ctrl.Width > rect.Width)
//            {
//                temp = 0;
//            }
//            else
//            {
//                temp = rect.Height / 2 + ctrl.Size.Height/2;
//            }

//            //var temp = rect.Height/2 + ctrl.Size.Height;

//            //如果不是父控件，则需要再加上控件所在位置的top
//            if (ctrl.Parent!=rect)
//            {
//                var f = rect.FindForm();
//                if (f!=null)
//                {

//                    Point p = default(Point);

//                    rect.Invoke(() =>
//                    {
//                        p = rect.PointToScreen(new Point(0, 0));
//                    });

//                    temp += p.Y;
//                }
                
//            }

//            return temp;

//        }

//        /// <summary>
//        ///     计算当前控件置于制定控件的垂直居中的left值
//        /// </summary>
//        /// <param name="ctrl"></param>
//        /// <param name="rect"></param>
//        /// <returns></returns>
//        public static int GetHorizontalMiddle(this Control ctrl, Control rect)
//        {
//            if (ctrl == null)
//            {
//                return 0;
//            }

//            int temp = 0;

//            if (ctrl.Width>rect.Width)
//            {
//                temp = 0;
//            }
//            else
//            {
//                temp = rect.Width / 2 - ctrl.Size.Width/2;
//            }

//            //如果不是父控件，则需要再加上控件所在位置的left
//            if (ctrl.Parent != rect)
//            {
//                var f = rect.FindForm();
//                if (f != null)
//                {
//                    //temp += f.Left + ctrl.Left;

//                    Point p=default(Point);

//                    rect.Invoke(()=>
//                                    {
//                                        p=rect.PointToScreen(new Point(0, 0));
//                                    });

//                    temp += p.X;

//                }
//                //temp += rect.FindForm().Left;
//            }

//            return temp;

//        }

//        public static void Invoke(this Control ctrl,MethodInvoker invoker)
//        {
//            Invoke(ctrl,invoker,null);
//        }

//        public static void Invoke(this Control ctrl,MethodInvoker invoker,params object[] param)
//        {
//            if (ctrl!=null)
//            {
//                ctrl.Invoke(invoker, param);
//            }
//        }
//    }

//}

//namespace Kugar.ExtMethod.Dictionary
//{
//    public static class DictionaryAbout
//    {
//        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> i,TKey key)
//        {
//            return TryGetValue(i, key, default(TValue));
//        }

//        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> i, TKey key, TValue defalutvalue)
//        {
//            TValue tv;

//            if(i.TryGetValue(key,out tv))
//            {
//                return tv;
//            }

//            return defalutvalue;
//        }

//        //IDictionary接口
//        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> i, TKey key)
//        {
//            return TryGetValue(i, key, default(TValue));
//        }

//        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> i, TKey key, TValue defalutvalue)
//        {
//            TValue tv;

//            if (i.TryGetValue(key, out tv))
//            {
//                return tv;
//            }

//            return defalutvalue;
//        }


//        /// <summary>
//        ///     字典转存为字符串,关键字与值之间使用 = 号相连,项目之间使用 '|' 号相连
//        /// </summary>
//        /// <param name="src">原字典</param>
//        /// <returns></returns>
//        public static string ToStingEx<TKey, TValue>(IDictionary<TKey, TValue> src)
//        {
//            return ToStingEx(src, '=', '|');
//        }

//        public static string ToStingEx<TKey, TValue>(IDictionary<TKey, TValue> src, char linesplite)
//        {
//            return ToStingEx(src, '=', linesplite);
//        }

//        public static string ToStingEx<TKey, TValue>(IDictionary<TKey, TValue> src, char valuesplite, char linesplite)
//        {
//            if (src == null || src.Count <= 0)
//            {
//                return string.Empty;
//            }

//            StringBuilder sb = new StringBuilder(256);

//            foreach (var o in src)
//            {
//                sb.Append(o.Key.ToString() + valuesplite + o.Value.ToString() + linesplite);
//            }

//            return sb.ToString();

//        }


//        public static string ToStingEx<TKey, TValue>(Dictionary<TKey, TValue> src)
//        {
//            return ToStingEx(src, '=', '|');
//        }

//        public static string ToStingEx<TKey, TValue>(Dictionary<TKey, TValue> src, char linesplite)
//        {
//            return ToStingEx(src, '=', linesplite);
//        }

//        public static string ToStingEx<TKey, TValue>(Dictionary<TKey, TValue> src, char valuesplite, char linesplite)
//        {
//            if (src == null || src.Count <= 0)
//            {
//                return string.Empty;
//            }

//            StringBuilder sb = new StringBuilder(256);

//            foreach (var o in src)
//            {
//                sb.Append(o.Key.ToString() + valuesplite + o.Value.ToString() + linesplite);
//            }

//            return sb.ToString();

//        }


//        //字典粘贴
//        public static IDictionary<string, string> ToDictionary(this string src)
//        {
//            return ToDictionary(src, '=', '|', null);
//        }

//        public static IDictionary<string, string> ToDictionary(this string src, IDictionary<string, string> defaultvalue)
//        {
//            return ToDictionary(src, '=', '|', defaultvalue);
//        }

//        public static IDictionary<string, string> ToDictionary(this string src, char linesplite, IDictionary<string, string> defaultvalue)
//        {
//            return ToDictionary(src, '=', linesplite, defaultvalue);
//        }

//        /// <summary>
//        ///     字符串转字典
//        /// </summary>
//        /// <param name="srcstr">原字符串</param>
//        /// <param name="linesplite">不同项目之间的间隔字符</param>
//        /// <param name="valuesplite">关键字与值之间的间隔</param>
//        /// <returns></returns>
//        public static IDictionary<string, string> ToDictionary(this string srcstr, char valuesplite, char linesplite, IDictionary<string, string> defaultvalue)
//        {
//            if (string.IsNullOrEmpty(srcstr))
//            {
//                return defaultvalue;
//            }

//            var strary = srcstr.Split(linesplite);

//            if (strary.Length <= 0)
//            {
//                return defaultvalue;
//            }


//            var temp = new Dictionary<string, string>(10);


//            foreach (var s in strary)
//            {
//                try
//                {
//                    var s1 = s.Split(valuesplite);
//                    if (s1.Length == 2)
//                    {
//                        temp.Add(s1[0], s1[1]);
//                    }
//                }
//                catch (Exception)
//                {
                    
//                    throw;
//                }

//            }

//            return temp;

//        }

//    }
//}

//namespace Kugar.ExtMethod.IPAbout
//{
//    using CommonExtMethod;

//    public static class  IPEndPointAbout
//    {
//        public static IPEndPoint ToIPEndPoint(this object str)
//        {
//            return ToIPEndPoint(str, new IPEndPoint(IPAddress.Loopback, 1986));
//        }

//        public static IPEndPoint ToIPEndPoint(this object str, IPEndPoint defaultvalue)
//        {
//            if (str==null)
//            {
//                return defaultvalue;
//            }

//            if (str.ToString().IsNullOrEmpty())
//            {
//                return defaultvalue;
//            }

//            return str.ToString().ToIPEndPoint(defaultvalue);

//            //var str1 = str.ToString().Split(':');
//            //if (str1.Length != 2)
//            //{
//            //    return defaultvalue;
//            //}

//            //return new IPEndPoint(str1[0].ToIPAddress(), str1[1].ToInt(1986));

//        }

//        //IP地址的粘贴
//        public static IPAddress ToIPAddress(this object str)
//        {
//            return ToIPAddress(str, IPAddress.Loopback);
//        }

//        public static IPAddress ToIPAddress(this object str, IPAddress defaultvalue)
//        {
//            if (str==null)
//            {
//                return defaultvalue;
//            }

//            //IPAddress ia = null;

//            //if (str.ToString().IsNullOrEmpty() || !IPAddress.TryParse(str.ToString(), out ia))
//            //{
//            //    return defaultvalue;
//            //}

//            return str.ToString().ToIPAddress(defaultvalue);

//        }


//        public static IPEndPoint ToIPEndPoint(this string str)
//        {
//            return ToIPEndPoint(str, new IPEndPoint(IPAddress.Loopback, 1986));
//        }

//        public static IPEndPoint ToIPEndPoint(this string str, IPEndPoint defaultvalue)
//        {
//            if (string.IsNullOrEmpty(str))
//            {
//                return defaultvalue;
//            }

//            var str1 = str.Split(':');
//            if (str1.Length != 2)
//            {
//                return defaultvalue;
//            }

//            return new IPEndPoint(str1[0].ToIPAddress(), str1[1].ToInt(1986));

//        }

//        //IP地址的粘贴
//        public static IPAddress ToIPAddress(this string str)
//        {
//            return ToIPAddress(str, IPAddress.Loopback);
//        }

//        public static IPAddress ToIPAddress(this string str, IPAddress defaultvalue)
//        {
//            IPAddress ia = null;

//            if (string.IsNullOrEmpty(str) || !IPAddress.TryParse(str, out ia))
//            {
//                return defaultvalue;
//            }

//            return ia;
//        }

//    }

//}

//namespace Kugar.ExtMethod.Data
//{
//    using System.Data;
//    using System.Data.OleDb;
//    using System.Collections.Generic;
//    using Kugar.ExtMethod.Dictionary;
//    using Kugar.ExtMethod.CommonExtMethod;


//    public static class DataTableAbout
//    {
//        /// <summary>
//        ///     导出DataTable为Excel文件
//        /// </summary>
//        /// <param name="tbl">源表</param>
//        /// <param name="ExportTblName">导出的excel中表名</param>
//        /// <param name="outPath">导出的路径,如果不存在,则会自动创建</param>
//        /// <param name="ColMapping">列映射容器 (tbl列名:excel列名)</param>
//        /// <param name="callback">回调函数,每插入一行回调一次</param>
//        /// <returns></returns>
//        public static bool ExportToExcel(this DataTable tbl,string ExportTblName,string outPath,Dictionary<string,string> ColMapping,EventHandler callback)
//        {
//            if (tbl==null || tbl.Rows.Count<=0 || tbl.Columns.Count<=0)// || ColMapping==null || ColMapping.Count<=0)
//            {
//                throw new Exception("输入参数错误");
//            }

//            if (ColMapping==null)
//            {
//                ColMapping=new Dictionary<string, string>();
//            }

//            if (ExportTblName.IsNullOrEmpty() && tbl.TableName.IsNullOrEmpty())
//            {
//                throw new Exception("输入参数错误");
//            }

//            string tblname=ExportTblName.Replace(" ","");

//            if (ExportTblName.IsNullOrEmpty())
//            {
//                tblname = tbl.TableName;
//            }

//            //var cnnstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + outPath +
//            //             ";Extended Properties=Excel 8.0;";

//            OleDbConnection cnn = null;
                
//            try
//            {
//                cnn = new OleDbConnection(GetExcelConnection(outPath));
//                cnn.Open();
//            }
//            catch (Exception)
//            {
//                return false;
//            }


//            //创建表格

//            var sb=new StringBuilder(256);

//            for (int j = 0; j < (int)Math.Ceiling((decimal)(tbl.Rows.Count / 65535)) + 1; j++)
//            {
//                sb.AppendLine("Create Table ");
//                sb.AppendLine(tblname.Trim() + "_" + j);
//                sb.AppendLine("(");

//                for (int i = 0; i < tbl.Columns.Count; i++)
//                {
//                    sb.AppendLine(string.Format("{0} {1},",
//                                                ColMapping.TryGetValue(tbl.Columns[i].ColumnName,
//                                                                       tbl.Columns[i].ColumnName),
//                                                TypeToString(tbl.Columns[i])));
//                }


//                sb.Remove(sb.Length - 3, 3);

//                sb.AppendLine(")");

//                var _cmd = cnn.CreateCommand();

//                _cmd.CommandText = sb.ToString();

//                try
//                {
//                    _cmd.ExecuteNonQuery();
//                }
//                catch (Exception)
//                {
//                    return false;
//                }

//            }


//            string str1="", str2 = "";
//            for (int i = 0; i < tbl.Columns.Count; i++)
//            {
//                var cols = tbl.Columns[i].ColumnName;

//                str1 += "," + ColMapping.TryGetValue(cols, cols);
//                str2 += ",?";
//            }

//            str1="(" + str1.Remove(0, 1) + ")";
//            str2="(" + str2.Remove(0, 1) + ")";





//            var cmd = cnn.CreateCommand();

//            cmd.Parameters.Clear();



//            for (int j = 0; j < tbl.Columns.Count; j++)
//            {
//                cmd.Parameters.Add("?"+ tbl.Columns[j].ColumnName,GetRefOleDataType(tbl.Columns[j]));
//            }


//            for (int i = 0; i < tbl.Rows.Count; i++)
//            {
//                //创建插入语句
//                var str = "insert into " + tblname.Trim() + "_" + (i / 65535) + " " + str1 + " values" +str2;
                          

//                cmd.CommandText = str;

//                var row = tbl.Rows[i];

//                for (int j = 0; j < cmd.Parameters.Count; j++)
//                {
//                    cmd.Parameters[j].Value = row[j];
//                }

//                var tr = cnn.BeginTransaction();
//                cmd.Transaction = tr;
//                try
//                {
//                    cmd.ExecuteNonQuery();
//                }
//                catch (Exception)
//                {
//                    tr.Rollback();
                    
//                    continue;
//                }

//                tr.Commit();

//                if (callback!=null)
//                {
//                    callback(null, null);
//                }
                

//            }

//            cnn.Close();

//            return true;


//        }

//        public static bool ImportFromExcel(this DataSet ds,string filepath)
//        {
//            if (!File.Exists(filepath))
//            {
//                return false;
//            }

//            OleDbConnection odn = null;

//            try
//            {
//                odn = new OleDbConnection(GetExcelConnection(filepath));
//                odn.Open();
//            }
//            catch (Exception)
//            {
//                return false;
//            }


//            var dt = new DataTable();

//            dt = odn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

//            if (dt == null)
//            {
//                return false;
//            }

//            var temptblname = new List<string>(5);
     
//            //读取所有表名
//            foreach (DataRow dr in dt.Rows)
//            {
//                string tempName = dr["Table_Name"].ToString();

//                int iDolarIndex = tempName.IndexOf('$');

//                if (iDolarIndex > 0)
//                {
//                    tempName = tempName.Substring(0, iDolarIndex);
//                }

//                //修正了Excel2003中某些工作薄名称为汉字的表无法正确识别的BUG。
//                if (tempName[0] == '\'')
//                {
//                    if (tempName[tempName.Length - 1] == '\'')
//                    {
//                        tempName = tempName.Substring(1, tempName.Length - 2);
//                    }
//                    else
//                    {
//                        tempName = tempName.Substring(1, tempName.Length - 1);
//                    }

//                }

//                if (!temptblname.Contains(tempName))
//                {
//                    temptblname.Add(tempName);
//                }

//            }
//            odn.Close();

//            //循环加载所有表
//            foreach (var n in temptblname)
//            {
//                var temptbl = new DataTable(n);

//                if(ImportFromExcel(temptbl, filepath, n))
//                {
//                    ds.Tables.Add(temptbl);
//                }
    
//            }

//            return true;

//        }

//        /// <summary>
//        ///     导入excel文件到本身
//        /// </summary>
//        /// <param name="tbl"></param>
//        /// <param name="filepath">excel文件名</param>
//        /// <param name="tablename"></param>
//        /// <returns></returns>
//        public static bool ImportFromExcel(this DataTable tbl,string filepath,string tablename)
//        {
//            tbl.Rows.Clear();
//            tbl.Columns.Clear();

//            if(!File.Exists(filepath))
//            {
//                return false;
//            }

//            try
//            {
//                OleDbConnection con = new OleDbConnection(GetExcelConnection(filepath));
//                OleDbDataAdapter adapter = new OleDbDataAdapter("Select * from [" + tablename + "$]", con);

//                con.Open();
//                adapter.FillSchema(tbl, SchemaType.Mapped);
//                adapter.Fill(tbl);

//                con.Close();

//                tbl.TableName = tablename;
//            }
//            catch (Exception)
//            {
//                return false;
//            }




//            return true;
//        }

//        public static DataTable ToEmptyDataTable(this IDataReader dr)
//        {
//            return ToEmptyDataTable(dr, "");
//        }

//        public static DataTable ToEmptyDataTable(this IDataReader dr,string tblName)
//        {
//            if (dr==null)
//            {
//                return null;
//            }

//            if (tblName == null)
//            {
//                tblName = "";
//            }

//            DataTable tbl = null;
//            try
//            {
//                DataTable schemaTable = dr.GetSchemaTable();
//                tbl = new DataTable(tblName);

//                foreach (DataRow myRow in schemaTable.Rows)
//                {
//                    DataColumn myDataColumn = new DataColumn(myRow[0].ToString(), myRow[0].GetType());
//                    tbl.Columns.Add(myDataColumn);
//                }

//            }
//            catch (Exception)
//            {
//                return null;
//            }


//            return tbl;
//        }

//        /// <summary>
//        ///     将DataReader对象的数据读入离线的DataTable对象中
//        /// </summary>
//        /// <param name="dr">DataReader对象</param>
//        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
//        public static DataTable ToDataTable(this IDataReader dr)
//        {
//            return ToDataTable(dr, 0, -1);
//        }

//        /// <summary>
//        ///     将DataReader对象的数据读入离线的DataTable对象中
//        /// </summary>
//        /// <param name="dr">DataReader对象</param>
//        /// <param name="startIndex">起始位置,0为从头开始</param>
//        /// <param name="count">读取的行数,当该值为-1时,表示全部读取</param>
//        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
//        public static DataTable ToDataTable(this IDataReader dr,int startIndex,int count)
//        {
//            return ToDataTable(dr, "", startIndex, count);
//        }


//        /// <summary>
//        ///     将DataReader对象的数据读入离线的DataTable对象中
//        /// </summary>
//        /// <param name="dr">DataReader对象</param>
//        /// <param name="tblName">目标DataTable对象的表名</param>
//        /// <param name="startIndex">起始位置,0为从头开始</param>
//        /// <param name="count">读取的行数,当该值为-1时,表示全部读取</param>
//        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
//        public static DataTable ToDataTable(this IDataReader dr,string tblName,int startIndex,int count)
//        {
//            if (dr==null || startIndex <0 || count==0)
//            {
//                return null;
//            }

//            if (startIndex > 0)
//            {
//                //跳过指定数量的数据行
//                for (int i = 0; i < startIndex; i++)
//                {
//                    if (!dr.Read())
//                    {
//                        break;
//                    }
//                }
//            }

//            var tbl = dr.ToEmptyDataTable(tblName);

//            if (tbl==null)
//            {
//                return null;
//            }

//            try
//            {
//                int c = 0;
//                while (dr.Read())
//                {
//                    c++;

//                    DataRow row = tbl.NewRow();
//                    for (int i = 0; i < tbl.Columns.Count; i++)
//                    {
//                        row[i] = dr[i];
//                    }

//                    tbl.Rows.Add(row);

//                    if (c!=-1 && c >= count)
//                    {
//                        break;
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                return null;
//            }



//            return tbl;
//        }


//        /// <summary>
//        /// 获取链接字符串
//        /// </summary>
//        /// <param name="strFilePath"></param>
//        /// <returns></returns>
//        private static string GetExcelConnection(string strFilePath)
//        {
//        //    if (!File.Exists(strFilePath))
//        //    {
//        //        throw new Exception("指定的Excel文件不存在！");
//        //    }
//            return
//                 @"Provider=Microsoft.Jet.OLEDB.4.0;" +
//                 @"Data Source=" + strFilePath + ";" +
//                 @"Extended Properties=" + Convert.ToChar(34).ToString() +
//                 @"Excel 8.0;" + "Imex=2;HDR=Yes;" + Convert.ToChar(34).ToString();
//        }

//        private static string TypeToString(DataColumn dataColumn)
//        {
//            switch (dataColumn.DataType.Name)
//            {
//                case "String"://字符串
//                    {
//                        return "NVarChar";
//                    }
//                case "Double"://数字
//                    {
//                        return "Double";
//                    }
//                case "Decimal"://数字
//                    {
//                        return "Decimal";
//                    }
//                case "DateTime"://时间
//                    {
//                        return "Date";
//                    }
//                case "Single":
//                case "Float":
//                    {
//                        return "Single";
//                    }
//                case "Int16":
//                    {
//                        return "Int";
//                    }
//                default://
//                    {
//                        return "NVarChar";
//                    }
//            }
//        }

//        public static OleDbType GetRefOleDataType(DataColumn dataColumn)
//        {
//            switch (dataColumn.DataType.Name)
//            {
//                case "String"://字符串
//                    {
//                        return OleDbType.VarWChar;
//                    }
//                case "Double"://数字
//                    {
//                        return OleDbType.Double;
//                    }
//                case "Decimal"://数字
//                    {
//                        return OleDbType.Decimal;
//                    }
//                case "DateTime"://时间
//                    {
//                        return OleDbType.Date;
//                    }
//                case "Int16":
//                    {
//                        return OleDbType.Integer;
//                    }
//                default:
//                    {
//                        return OleDbType.VarWChar;
//                    }
//            }
//        }
//    }

//}




//namespace Kugar.Parse
//{
//    public class ObjectParse
//    {
//        //粘贴Int类型
//        public static int IntParse(object obj)
//        {
//            return IntParse(obj, 0);
//        }

//        public static int IntParse(string str)
//        {
//            return IntParse(str, 0);
//        }

//        public static int IntParse(object obj,int defaultvalue)
//        {
//            if (obj!=null)
//            {
//                return IntParse(obj.ToString(), defaultvalue);
//            }
//            else
//            {
//                return defaultvalue;
//            }
            
//        }

//        public static int IntParse(string str,int defaultvalue)
//        {
//            int t;

//            if (int.TryParse(str,out t))
//            {
//                return t;
//            }
//            else
//            {
//                return defaultvalue;
//            }
//        }

//        public static int IntParse(IDictionary<object, object> src, object key)
//        {
//            return IntParse(src, key, 0);
//        }

//        public static int IntParse(IDictionary<object,object> src, object key, int defaultvalue)
//        {
//            return IntParse(DictionaryFunction.ObjectPase(src, key, defaultvalue));
//        }


//        //粘贴Double类型
//        public static double DoubleParse(object obj)
//        {
//            return DoubleParse(obj, 0);
//        }

//        public static double DoubleParse(string str)
//        {
//            return DoubleParse(str, 0);
//        }

//        public static double DoubleParse(object obj, double defaultvalue)
//        {
//            if (obj != null)
//            {
//                return DoubleParse(obj.ToString(), defaultvalue);
//            }
//            else
//            {
//                return defaultvalue;
//            }

//        }

//        public static double DoubleParse(string str, double defaultvalue)
//        {
//            Double t;

//            if (Double.TryParse(str, out t))
//            {
//                return t;
//            }
//            else
//            {
//                return defaultvalue;
//            }
//        }

//        //粘贴decimal类型
//        public static decimal DecimalParse(object obj)
//        {
//            return DecimalParse(obj, 0);
//        }

//        public static decimal DecimalParse(string str)
//        {
//            return DecimalParse(str, 0);
//        }

//        public static decimal DecimalParse(object obj, decimal defaultvalue)
//        {
            

//            if (obj != null)
//            {
//                return DecimalParse(obj.ToString(), defaultvalue);
//            }
//            else
//            {
//                return defaultvalue;
//            }

//        }

//        public static decimal DecimalParse(string str, decimal defaultvalue)
//        {
//            decimal t;

//            if (decimal.TryParse(str, out t))
//            {
//                return t;
//            }
//            else
//            {
//                return defaultvalue;
//            }
//        }



//        //字符串的粘贴
//        public static string StringParse(object obj)
//        {
//            return StringParse(obj, string.Empty);
//        }

//        public static string StringParse(object obj,string defaultvalue)
//        {
//            if (obj!=null)
//            {
//                return obj.ToString();
//            }
//            else
//            {
//                return defaultvalue;
//            }
//        }

//        /// <summary>
//        ///     字典转存为字符串,关键字与值之间使用 = 号相连,项目之间使用 '|' 号相连
//        /// </summary>
//        /// <param name="src">原字典</param>
//        /// <returns></returns>
//        public static string StringParse(IDictionary<object,object> src)
//        {
//            return StringParse(src, '=', '|');
//        }

//        public static string StringParse(IDictionary<object,object> src, char linesplite)
//        {
//            return StringParse(src, '=', linesplite);
//        }

//        public static string StringParse(IDictionary<object,object> src, char valuesplite, char linesplite)
//        {
//            if (src==null || src.Count<=0)
//            {
//                return string.Empty;
//            }

//            StringBuilder sb=new StringBuilder(256);

//            foreach (var o in src)
//            {
//                sb.Append(o.Key.ToString() + valuesplite + o.Value.ToString() + linesplite);
//            }

//            return sb.ToString();

//        }


//        //IPEndPort的粘贴
//        public static IPEndPoint IPEndPointParse(string ipaddress, string port, IPEndPoint defaultvalue)
//        {
//            if (string.IsNullOrEmpty(ipaddress) || string.IsNullOrEmpty(port))
//            {
//                return defaultvalue;
//            }

//            var ipe = new IPEndPoint(IPAddressParse(ipaddress), IntParse(port, 1986));

//            return ipe;
//        }

//        public static IPEndPoint IPEndPointParse(string ipaddress, string port)
//        {
//            return IPEndPointParse(ipaddress, port, new IPEndPoint(IPAddress.Loopback, 1986));
//        }

//        public static IPEndPoint IPEndPointParse(string str)
//        {
//            return IPEndPointParse(str, new IPEndPoint(IPAddress.Loopback, 1986));
//        }
        
//        public static IPEndPoint IPEndPointParse(string str,IPEndPoint defaultvalue)
//        {
//            if (string.IsNullOrEmpty(str))
//            {
//                return defaultvalue;
//            }

            

//            var str1 = str.Split(':');
//            if (str1.Length!=2)
//            {
//                return defaultvalue;
//            }

//            return IPEndPointParse(str1[0], str1[1]);
        
//        }

//        public static IPEndPoint IPEndPointParse(IDictionary<object, object> src, object key)
//        {
//            return IPEndPointParse(DictionaryFunction.ObjectPase(src, key).ToString());
//        }



//        //IP地址的粘贴
//        public static IPAddress IPAddressParse(string str)
//        {
//            return IPAddressParse(str, IPAddress.Loopback);
//        }

//        public static IPAddress IPAddressParse(string str,IPAddress defaultvalue)
//        {
//            IPAddress ia = null;



//            if (string.IsNullOrEmpty(str) || !IPAddress.TryParse(str,out ia))
//            {
//                return defaultvalue;
//            }

//            return ia;
//        }

//        //public static IPAddress IPAddressParse(IDictionary<object, object> src, object key)
//        //{
//        //    return IPAddressParse(DictionaryFunction.ObjectPase(src, key).ToString());
//        //}



//        //字典粘贴
//        public static Dictionary<string, string> DictionaryParse(string src)
//        {
//            return DictionaryParse(src, '=', '|', null);
//        }

//        public static Dictionary<string, string> DictionaryParse(string src, Dictionary<string, string> defaultvalue)
//        {
//            return DictionaryParse(src, '=', '|', defaultvalue);
//        }

//        public static Dictionary<string ,string > DictionaryParse(string src,char linesplite, Dictionary<string, string> defaultvalue)
//        {
//            return DictionaryParse(src, '=', linesplite, defaultvalue);
//        }

//        /// <summary>
//        ///     字符串转字典
//        /// </summary>
//        /// <param name="srcstr">原字符串</param>
//        /// <param name="linesplite">不同项目之间的间隔字符</param>
//        /// <param name="valuesplite">关键字与值之间的间隔</param>
//        /// <returns></returns>
//        public static Dictionary<string, string> DictionaryParse(string srcstr, char valuesplite, char linesplite, Dictionary<string, string> defaultvalue)
//        {
//            if (string.IsNullOrEmpty(srcstr))
//            {
//                return defaultvalue;
//            }

//            var strary = srcstr.Split(linesplite);

//            if (strary.Length<=0)
//            {
//                return defaultvalue;
//            }


//            var temp = new Dictionary<string, string>(10);

         
//            foreach (var s in strary)
//            {
//                var s1 = s.Split(valuesplite);
//                if (s1.Length==2)
//                {
//                    temp.Add(s1[0],s1[1]);            
//                }
//            }

//            return temp;

//        }


//        //Listzhantie
//        public static List<string> ListParse(string srcstr)
//        {
//            return ListParse(srcstr, '\n');
//        }

//        public static List<string> ListParse(string srcstr,char linesplite)
//        {
//            return ListParse(srcstr, linesplite, null);
//        }

//        public static List<string> ListParse(string srcstr,char linesplite,List<string> defaultvalue)
//        {
//            if (string.IsNullOrEmpty(srcstr))
//            {
//                return defaultvalue;
//            }

//            var s1 = srcstr.Split(linesplite);

//            if (s1.Length<=0)
//            {
//                return defaultvalue;
//            }

//            var temp = new List<string>(10);

//            foreach (var s in s1)
//            {
//                temp.Add(s);
//            }

//            return temp;
//        }

//        //Bool粘贴
//        public static bool BooleanParse(string str)
//        {
//            return BooleanParse(str, false);
//        }

//        public static bool BooleanParse(string str,bool defaultvalue)
//        {
//            bool b;
//            if(bool.TryParse(str,out b))
//            {
//                return b;
//            }
//            return defaultvalue;
//        }


//    }

//    public class DictionaryFunction
//    {
//        public static object ObjectPase(IDictionary<object, object> src, object keyvalue)
//        {
//            return ObjectPase(src, keyvalue, null);
//        }

//        public static object ObjectPase(IDictionary<object, object> src, object keyvalue, object defaultvalue)
//        {
//            object temp;

//            if (src.TryGetValue(keyvalue, out temp))
//            {
//                return temp;
//            }

//            return defaultvalue;
//        }
//    }


//}
