using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.OleDb;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.ExtMethod
{
    public class BeforceRowExportEventArgs:EventArgs
    {
        public BeforceRowExportEventArgs()
        {
            row = null;
            IsExport = true;
            IsBreakExport = false;
        }

        public BeforceRowExportEventArgs(DataRow _row):this()
        {
            row = _row;
        }

        /// <summary>
        ///     已更新的数据行
        /// </summary>
        public DataRow row { set; get; }

        /// <summary>
        ///     当前行是否导出
        /// </summary>
        public bool IsExport { set; get; }

        /// <summary>
        ///     
        /// </summary>
        public bool IsBreakExport { set; get; }
    }

    public class AfterRowExportEventArgs:EventArgs
    {
        public AfterRowExportEventArgs()
        {
            row = null;
            IsBreakExport = false ;
        }

        public AfterRowExportEventArgs(DataRow _row):this()
        {
            row = _row;
        }

        /// <summary>
        ///     已更新的数据行
        /// </summary>
        public DataRow row { set; get; }

        /// <summary>
        ///     是否继续导出,如果设置为否,则退出导出过程
        /// </summary>
        public bool IsBreakExport { set; get; }
    }

    public class PropertyAttributeMappingColumn
    {
        public string ColumnName { set; get; }
    }

    public static class DataTableAbout
    {
        #region "变量"

        private static Dictionary<Type, Dictionary<PropertyInfo, PropertyAttributeMappingColumn>> _buffPropertyColumn = new Dictionary<Type, Dictionary<PropertyInfo, PropertyAttributeMappingColumn>>();

        private static Dictionary<Type, IEnumerable<PropertyInfo>> _buffPropertyList =new Dictionary<Type, IEnumerable<PropertyInfo>>();

        #endregion

        /// <summary>
        ///     为DataTable添加一个行号列,列名为空字符串,起始值为0
        /// </summary>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static bool BuildRowIndex(this DataTable tbl)
        {
            return BuildRowIndex(tbl, "", 0);
        }

        /// <summary>
        ///     为DataTable添加一个行号列
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="rowIndexName">行号列的列名</param>
        /// <param name="rowIndexStart">行号列的起始序号</param>
        /// <returns></returns>
        public static bool BuildRowIndex(this DataTable tbl,string rowIndexName,int rowIndexStart)
        {
            if (tbl.IsEmptyData())
            {
                return false;
            }

            if (rowIndexName.IsNullOrEmpty())
            {
                rowIndexName = "_RowIndex";
            }

            if (!tbl.Columns.Contains(rowIndexName))
            {
                tbl.Columns.Add(rowIndexName, typeof (int));
            }

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                var row = tbl.Rows[i];

                row[rowIndexName] = rowIndexStart + i;
            }

            return true;
        }

        #if NET2

        public static IEnumerable<DataRow> AsEnumerable(this DataTable tbl)
        {
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                yield return  tbl.Rows[i];
            }
        }

        #endif

        private static Dictionary<string,string> _emptyDictionary=new Dictionary<string, string>();

        /// <summary>
        ///     导出DataTable为Excel文件
        /// </summary>
        /// <param name="tbl">源表</param>
        /// <param name="ExportTblName">导出的excel中表名</param>
        /// <param name="outPath">导出的路径,如果不存在,则会自动创建</param>
        /// <param name="ColMapping">列映射容器 (tbl列名:excel列名)</param>
        /// <param name="AfterExport">回调函数,每插入一行回调一次</param>
        /// <returns></returns>
        public static bool ExportToExcel(this DataTable tbl, string ExportTblName, string outPath, Dictionary<string, string> ColMapping, EventHandler<BeforceRowExportEventArgs> BeforceExport, EventHandler<AfterRowExportEventArgs> AfterExport,bool exportMappingOnly=false)
        {
            if (tbl == null || tbl.Rows.Count <= 0 || tbl.Columns.Count <= 0)// || ColMapping==null || ColMapping.Count<=0)
            {
                throw new Exception("输入参数错误");
            }

            if (ColMapping == null)
            {
                ColMapping = _emptyDictionary;// new Dictionary<string, string>();
            }

            if (ExportTblName.IsNullOrEmpty() && tbl.TableName.IsNullOrEmpty())
            {
                throw new Exception("输入参数错误");
            }

            string tblname = ExportTblName.Replace(" ", "");

            if (ExportTblName.IsNullOrEmpty())
            {
                tblname = tbl.TableName;
            }

            //var cnnstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + outPath +
            //             ";Extended Properties=Excel 8.0;";

            OleDbConnection cnn = null;

            try
            {
                cnn = new OleDbConnection(GetExcelConnection(outPath));
                cnn.Open();
            }
            catch (Exception)
            {
                return false;
            }


            //创建表格

            StringBuilder sb = null;

            List<string> exportColumnName = new List<string>();
            if (exportMappingOnly && ColMapping.Count>0)
            {
                foreach (var mapping in ColMapping)
                {
                    exportColumnName.Add(mapping.Key);
                }
            }
            else
            {
                foreach (DataColumn column in tbl.Columns)
                {
                    exportColumnName.Add(column.ColumnName);
                }
            }
            

            for (int j = 0; j < (int)Math.Ceiling((decimal)(tbl.Rows.Count / 65535)) + 1; j++)
            {
                sb=new StringBuilder(255);
                sb.AppendLine("Create Table ");
                sb.AppendLine(tblname.Trim() + "_" + j);
                sb.AppendLine("(");

                for (int i = 0; i < exportColumnName.Count; i++)
                {
                    sb.AppendLine(string.Format("{0} nvarchar,",
                                                ColMapping.TryGetValue(exportColumnName[i],
                                                                       exportColumnName[i])
                                                ));
                }

                sb.Remove(sb.Length - 3, 3);

                sb.AppendLine(")");

                using (var _cmd = cnn.CreateCommand())
                {
                    _cmd.CommandText = sb.ToString();

                    try
                    {
                        _cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        return false;
                    }                    
                }



            }

            string str1 = "", str2 = "";
            for (int i = 0; i < exportColumnName.Count; i++)
            {
                //var cols = exportColumnName;

                str1 += "," + ColMapping.TryGetValue(exportColumnName[i], exportColumnName[i]);
                str2 += ",?";
            }

            str1 = "(" + str1.Remove(0, 1) + ")";
            str2 = "(" + str2.Remove(0, 1) + ")";



            using (var cmd = cnn.CreateCommand())
            {
                cmd.Parameters.Clear();

                for (int j = 0; j < exportColumnName.Count; j++)
                {
                    cmd.Parameters.Add("?" + exportColumnName[j],OleDbType.VarWChar);
                }

                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    //创建插入语句
                    var str = "insert into " + tblname.Trim() + "_" + (i/65535) + " " + str1 + " values" + str2;


                    cmd.CommandText = str;

                    var row = tbl.Rows[i];

                    var ea = new BeforceRowExportEventArgs(row);

                    if (BeforceExport!=null)
                    {
                        BeforceExport(null, ea);
                        if (ea.IsBreakExport)
                        {
                            break;
                        }

                        if (!ea.IsExport)
                        {
                            continue;
                        }

                        row = ea.row;
                    }



                    for (int j = 0; j < exportColumnName.Count; j++)
                    {
                        cmd.Parameters[j].Value = row[exportColumnName[j]].ToStringEx();
                    }

                    var tr = cnn.BeginTransaction();

                    cmd.Transaction = tr;

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        tr.Rollback();
                        continue;
                    }

                    tr.Commit();

                    if (AfterExport != null)
                    {
                        var temp = new AfterRowExportEventArgs(row);

                        AfterExport(null, temp);

                        if (temp.IsBreakExport)
                        {
                            break;
                        }
                    }
                }
            }



            cnn.Close();

            return true;


        }

        /// <summary>
        ///     导出DataTable为Excel文件
        /// </summary>
        /// <param name="tbl">源表</param>
        /// <param name="ExportTblName">导出的excel中表名</param>
        /// <param name="outPath">导出的路径,如果不存在,则会自动创建</param>
        /// <param name="ColMapping">列映射容器 (tbl列名:excel列名)</param>
        /// <param name="AfterExport">回调函数,每插入一行回调一次</param>
        /// <returns></returns>
        public static bool ExportToExcel(this DataTable tbl,string  ExportTblName,string outPath, Dictionary<string, string> ColMapping,EventHandler<AfterRowExportEventArgs> AfterExport)
        {
            return ExportToExcel(tbl, ExportTblName, outPath, ColMapping, null, AfterExport);
        }

        /// <summary>
        ///     导入excel文件中的所有表到本身DataSet
        /// </summary>
        /// <param name="ds">用于导入的DataSet</param>
        /// <param name="filepath">excel文件名</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool ImportFromExcel(this DataSet ds, string filepath)
        {
            if (!File.Exists(filepath))
            {
                return false;
            }

            OleDbConnection odn = null;

            try
            {
                odn = new OleDbConnection(GetExcelConnection(filepath));
                odn.Open();
            }
            catch (Exception)
            {
                return false;
            }


            var dt = new DataTable();

            dt = odn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            if (dt == null)
            {
                return false;
            }

            var temptblname = new List<string>(5);

            //读取所有表名
            foreach (DataRow dr in dt.Rows)
            {
                string tempName = dr["Table_Name"].ToString();

                int iDolarIndex = tempName.IndexOf('$');

                if (iDolarIndex > 0)
                {
                    tempName = tempName.Substring(0, iDolarIndex);
                }

                //修正了Excel2003中某些工作薄名称为汉字的表无法正确识别的BUG。
                if (tempName[0] == '\'')
                {
                    if (tempName[tempName.Length - 1] == '\'')
                    {
                        tempName = tempName.Substring(1, tempName.Length - 2);
                    }
                    else
                    {
                        tempName = tempName.Substring(1, tempName.Length - 1);
                    }

                }

                if (!temptblname.Contains(tempName))
                {
                    temptblname.Add(tempName);
                }

            }
            odn.Close();

            //循环加载所有表
            foreach (var n in temptblname)
            {
                var temptbl = new DataTable(n);

                if (ImportFromExcel(temptbl, filepath, n))
                {
                    ds.Tables.Add(temptbl);
                }

            }

            return true;

        }

        /// <summary>
        ///     导入excel文件中的一个表到到本身DataTable
        /// </summary>
        /// <param name="tbl">用于导入的DataTable</param>
        /// <param name="filepath">excel文件名</param>
        /// <param name="tablename">excel中的表名</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool ImportFromExcel(this DataTable tbl, string filepath, string tablename)
        {
            tbl.Rows.Clear();
            tbl.Columns.Clear();

            if (!File.Exists(filepath))
            {
                return false;
            }

            try
            {
                OleDbConnection con = new OleDbConnection(GetExcelConnection(filepath));
                OleDbDataAdapter adapter = new OleDbDataAdapter("Select * from [" + tablename + "$]", con);

                con.Open();
                adapter.FillSchema(tbl, SchemaType.Mapped);
                adapter.Fill(tbl);

                con.Close();

                tbl.TableName = tablename;
            }
            catch (Exception)
            {
                return false;
            }




            return true;
        }


        /// <summary>
        ///     从DataReader中构建一个只有列结构约束的DataTable,该DataTable不包含任何行,默认为空名称
        /// </summary>
        /// <param name="dr">源DataReader</param>
        /// <returns></returns>
        public static DataTable ToEmptyDataTable(this IDataReader dr)
        {
            return ToEmptyDataTable(dr, "");
        }

        /// <summary>
        ///     从DataReader中构建一个只有列结构约束的DataTable,该DataTable不包含任何行
        /// </summary>
        /// <param name="dr">源DataReader</param>
        /// <param name="tblName">生成后的DataTable的名称</param>
        /// <returns></returns>
        public static DataTable ToEmptyDataTable(this IDataReader dr, string tblName)
        {
            if (dr == null)
            {
                return null;
            }

            if (tblName == null)
            {
                tblName = "";
            }

            DataTable tbl = null;
            try
            {
                DataTable schemaTable = dr.GetSchemaTable();
                tbl = new DataTable(tblName);

                foreach (DataRow myRow in schemaTable.Rows)
                {
                    DataColumn myDataColumn = new DataColumn(myRow[0].ToString(), myRow[0].GetType());
                    tbl.Columns.Add(myDataColumn);
                }

            }
            catch (Exception)
            {
                return null;
            }


            return tbl;
        }

        /// <summary>
        ///     将DataReader对象的数据读入离线的DataTable对象中
        /// </summary>
        /// <param name="dr">DataReader对象</param>
        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
        public static DataTable ToDataTable(this IDataReader dr)
        {
            return ToDataTable(dr, 0, -1);
        }

        /// <summary>
        ///     将DataReader对象的数据读入离线的DataTable对象中
        /// </summary>
        /// <param name="dr">DataReader对象</param>
        /// <param name="startIndex">起始位置,0为从头开始</param>
        /// <param name="count">读取的行数,当该值为-1时,表示全部读取</param>
        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
        public static DataTable ToDataTable(this IDataReader dr, int startIndex, int count)
        {
            return ToDataTable(dr, "", startIndex, count);
        }

        /// <summary>
        ///     将DataReader对象的数据读入离线的DataTable对象中(用于追加数据)
        /// </summary>
        /// <param name="dr">DataReader对象</param>
        /// <param name="tblName">目标DataTable对象的表名</param>
        /// <param name="startIndex">起始位置,0为从头开始</param>
        /// <param name="count">读取的行数,当该值为-1时,表示全部读取</param>
        /// <returns>返回一个填充完成的DataTable对象,出错时返回null</returns>
        public static DataTable ToDataTable(this IDataReader dr, string tblName, int startIndex, int count)
        {
            if (dr == null || startIndex < 0 || count == 0)
            {
                return null;
            }

            if (startIndex > 0)
            {
                //跳过指定数量的数据行
                for (int i = 0; i < startIndex; i++)
                {
                    if (!dr.Read())
                    {
                        break;
                    }
                }
            }

            var tbl = dr.ToEmptyDataTable(tblName);

            if (tbl == null)
            {
                return null;
            }

            try
            {
                int c = 0;
                while (dr.Read())
                {
                    c++;

                    DataRow row = tbl.NewRow();
                    for (int i = 0; i < tbl.Columns.Count; i++)
                    {
                        row[i] = dr[i];
                    }

                    tbl.Rows.Add(row);

                    if (c != -1 && c >= count)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }



            return tbl;
        }

        /// <summary>
        ///     返回指定的DataTable中是否为空数据,如果为空,返回True,如果有数据行,返回False
        /// </summary>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static bool IsEmptyData(this DataTable tbl)
        {
            if (tbl==null || tbl.Rows.Count<=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     返回指定的DataTable中是否包含数据,如果包含,返回True,空数据返回False
        /// </summary>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static bool IsNotEmptyData(this DataTable tbl)
        {
            return !IsEmptyData(tbl);
        }

        /// <summary>
        ///     获取指定列的对应OleDbType类型,即.Net基础数据类型与OleDbType之间的转换
        /// </summary>
        /// <param name="dataColumn">源列</param>
        /// <returns></returns>
        public static OleDbType GetRefOleDataType(DataColumn dataColumn)
        {
            switch (dataColumn.DataType.Name)
            {
                case "String"://字符串
                    {
                        return OleDbType.VarWChar;
                    }
                case "Double"://数字
                    {
                        return OleDbType.Double;
                    }
                case "Decimal"://数字
                    {
                        return OleDbType.Decimal;
                    }
                case "DateTime"://时间
                    {
                        return OleDbType.Date;
                    }
                case "Int16":
                    {
                        return OleDbType.SmallInt;
                    }
                case "Int32":
                    {
                        return OleDbType.Integer;
                    }
                case "Int64":
                    {
                        return OleDbType.BigInt;
                    }
                default:
                    {
                        return OleDbType.VarWChar;
                    }
            }
        }

        /// <summary>
        ///     判断该DataReader是否包含指定名称的列
        /// </summary>
        /// <param name="dr">源DataReader</param>
        /// <param name="name">用于比较的列名,忽略大小写</param>
        /// <returns></returns>
        public static bool IsContainsColumn(this DbDataReader dr, string name)
        {
            int count = dr.FieldCount;
            for (int i = 0; i < count; i++)
            {
                if ( string.Compare(dr.GetName(i),name,true)==0)
                {
                    return true;
                }
            }
            return false;
        }

        #region "DataTableToObjectList"


        public static IEnumerable<T> DataTableToObjectList<T>(this DataTable tbl) where T : new()
        {
            if (tbl.IsEmptyData())
            {
                return null;
            }

            var lst = new List<T>();

            foreach (DataRow dataRow in tbl.Rows)
            {
                var t = DataRowToObject<T>(dataRow);

                lst.Add(t);
            }

            return lst;
        }

        public static IEnumerable<T> DataTableToObjectList<T>(this DbDataReader dr, Action<T, DbDataReader> afterPreRowBuild = null, Action<IEnumerable<T>, DbDataReader> afterAllRowBuild = null) where T : new()
        {
            if (dr==null || !dr.HasRows)
            {
                return null;
            }

            var tlist = new List<T>();



            while (dr.Read())
            {
                var temp = dr.DataRowToObject<T>(afterPreRowBuild);

                tlist.Add(temp);
            }

            if (afterAllRowBuild!=null)
            {
                afterAllRowBuild(tlist, dr);
            }

            return tlist;
        }



        /// <summary>
        ///     将DataRow转换为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T DataRowToObject<T>(this DataRow row) where T:new()
        {
            if (row==null)
            {
                return default(T);
            }

            var t = typeof (T);

            var dic = GetObjectPropertyMapping(typeof(T));


            if (dic.Count<=0)
            {
                return default(T);
            }

            var obj = new T();

            foreach (var d in dic)
            {
                if (!row.Table.Columns.Contains(d.Value.ColumnName))
                {
                    continue;
                }

                var v = row[d.Value.ColumnName];

                if (v is DBNull)
                {
                    v = null;
                }

                d.Key.SetValue(obj, v, null);

                //d.Key.SetValue(obj, row[d.Value.ColumnName],null);
            }

            return obj;


        }

        /// <summary>
        ///     将DataReader转为实体，，只读取一行数据 <br/>
        ///     该函数需要预先在实体内设置Attribute特性，自动将T类型中所有属性名匹配DataReader中的列名，存在相同名称的，则设置值<br/>
        ///     注：该函数只能同步与属性名称相同的列的数据<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="afterBuild">成功转换为实体后，执行的操作</param>
        /// <returns></returns>
        public static T DataRowToObject<T>(this DbDataReader dr,Action<T,DbDataReader> afterBuild=null) where T:new()
        {
            var dic = GetObjectPropertyMapping(typeof(T));

            var obj = new T();

            foreach (var d in dic)
            {
                try
                {
                    if (!dr.IsContainsColumn(d.Value.ColumnName))
                    {
                        continue;
                    }

                    var v = dr[d.Value.ColumnName];

                    if (v is DBNull)
                    {
                        v = null;
                    }

                    d.Key.SetValue(obj, v, null);

                    //d.Key.SetValue(obj, dr[d.Value.ColumnName],null);
                }
                catch (Exception)
                {
                    continue;
                }

            }

            if (afterBuild!=null)
            {
                afterBuild(obj, dr);
            }

            return obj;

        }

        #endregion

        #region "DataRowToObjectListNoAttribute"

        /// <summary>
        ///     将DbDataReader转为一个可枚举的实体集<br/>
        ///     该函数将读取DbDataReader中所有行的数据,<br/>
        /// </summary>
        /// <typeparam name="T">要求该类型比如有默认构造函数</typeparam>
        /// <param name="dr"></param>
        /// <param name="afterPreRowBuild">每一行转换完成后,回调该函数</param>
        /// <param name="afterAllRowBuild">所有行转换后回调该函数</param>
        /// <returns></returns>
        public static IEnumerable<T> DataRowToObjectListNoAttribute<T>(this DbDataReader dr,  Action<T, DbDataReader> afterPreRowBuild = null, Action<IEnumerable<T>, DbDataReader> afterAllRowBuild = null) where T:new()
        {
            return DataRowToObjectListNoAttribute<T>(dr, null, afterPreRowBuild, afterAllRowBuild);
        }

        /// <summary>
        ///     将DbDataReader转为一个可枚举的实体集,该函数将读取DbDataReader中所有行的数据
        /// </summary>
        /// <typeparam name="T">该类型可以不需要有默认构造函数,可以由beforcePreRowBuild回调返回构建后的对象</typeparam>
        /// <param name="dr"></param>
        /// <param name="beforcePreRowBuild"> 每一行转换开始前,回调该函数,用于自定义创建T对象的实例</param>
        /// <param name="afterPreRowBuild">每一行转换完成后,回调该函数</param>
        /// <param name="afterAllRowBuild">所有行转换后回调该函数</param>
        /// <returns></returns>
        public static IEnumerable<T> DataRowToObjectListNoAttribute<T>(this DbDataReader dr, Func<DbDataReader, T> beforcePreRowBuild = null, Action<T, DbDataReader> afterPreRowBuild = null, Action<IEnumerable<T>, DbDataReader> afterAllRowBuild = null)
        {
            if (dr == null || !dr.HasRows)
            {
                return null;
            }

            var tlist = new List<T>();

            while (dr.Read())
            {
                var temp = dr.DataRowToObjectNoAttribute<T>(beforcePreRowBuild,afterPreRowBuild);

                tlist.Add(temp);
            }


            if (afterAllRowBuild != null)
            {
                afterAllRowBuild(tlist, dr);
            }

            return tlist;
        }
        
        /// <summary>
        ///     将DataReader转为实体，，只读取一行数据 <br/>
        ///     该函数不需要预先在实体内设置Attribute特性，自动将T类型中所有属性名匹配DataReader中的列名，存在相同名称的，则设置值<br/>
        ///     注 : 如果需要将值设置到 "属性的属性" ,则列名为 "属性.属性.属性"<br/>
        ///     注：该函数只能同步与属性名称相同的列的数据<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="afterBuild">成功转换为实体后，执行的操作</param>
        /// <returns></returns>
        public static T DataRowToObjectNoAttribute<T>(this DbDataReader dr,Action<T,DbDataReader> afterBuild=null) where T:new()
        {
            return DataRowToObjectNoAttribute<T>(dr, null, afterBuild);
        }

        /// <summary>
        ///     将DataReader转为实体，只读取一行数据<br/>
        ///     该函数不需要预先在实体内设置Attribute特性，自动将T类型中所有属性名匹配DataReader中的列名，存在相同名称的，则设置值<br/>
        ///     注 : 如果需要将值设置到 "属性的属性" ,则列名为 "属性.属性.属性"
        ///     注：该函数只能同步与属性名称相同的列的数据<br/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">源DataReader</param>
        /// <param name="beforceBuild">每一行转换开始前,回调该函数,用于自定义创建T对象的实例</param>
        /// <param name="afterBuild">成功转换为实体后，执行的操作</param>
        /// <returns></returns>
        public static T DataRowToObjectNoAttribute<T>(this DbDataReader dr, Func<DbDataReader,T> beforceBuild = null, Action<T, DbDataReader> afterBuild = null)
        {
            var dic = typeof(T).GetPropertyExecutorList(4,true);

            //var dic = GetTypePropertyInfo(typeof(T));

            if (dic == null)
            {
                return default(T);
            }

            if (dr == null)
            {
                throw new ArgumentNullException("dr");
            }

            try
            {
                dr.GetValue(0);
            }
            catch (InvalidOperationException)
            {
                dr.Read();
            }

            T obj = default(T);

            if (beforceBuild==null)
            {
                obj = buildDefaultObj<T>();
            }
            else
            {
                obj = beforceBuild(dr);
            }
            
            for (int i = 0; i < dr.FieldCount; i++)
            {
                try
                {
                    var fieldName = dr.GetName(i);

                    var v = dr[i];

                    if (v is DBNull)
                    {
                        v = null;
                    }

                    obj.FastSetValue(fieldName, v);

                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (afterBuild != null)
            {
                afterBuild(obj, dr);
            }

            return obj;
        }

        #endregion

        private static T buildDefaultObj<T>()
        {
            return Activator.CreateInstance<T>();
        }

        private static Dictionary<PropertyInfo, PropertyAttributeMappingColumn> GetObjectPropertyMapping(Type t)
        {
            Dictionary<PropertyInfo, PropertyAttributeMappingColumn> dic = null;

            if (_buffPropertyColumn.ContainsKey(t))
            {
                dic = _buffPropertyColumn[t];
            }
            else
            {
                dic = new Dictionary<PropertyInfo, PropertyAttributeMappingColumn>();

                var plist = t.GetProperties();

                foreach (var p in plist)
                {
                    if (!(p.CanWrite && p.CanRead))
                    {
                        continue;
                    }

                    var attr = t.GetCustomAttributes(typeof(PropertyAttributeMappingColumn), true);

                    if (attr != null && attr.Length > 0 )
                    {
                        dic.Add(p, (PropertyAttributeMappingColumn)attr[0]);
                    }
                }

                _buffPropertyColumn.Add(t, dic);
            }

            if (dic==null || dic.Count<=0)
            {
                return null;
            }

            return dic;
        }

        private static IEnumerable<PropertyInfo> GetTypePropertyInfo(Type t)
        {
            IEnumerable<PropertyInfo> dic = null;

            if (_buffPropertyList.ContainsKey(t))
            {
                dic = _buffPropertyList[t];
            }
            else
            {
                dic = new List<PropertyInfo>();

                var plist = t.GetProperties();

                foreach (var p in plist)
                {
                    if (p.CanRead && p.CanWrite)
                    {
                        ((List<PropertyInfo>)dic).Add(p);
                    }
                }

                _buffPropertyList.Add(t, dic);
            }

            if (dic == null)
            {
                return null;
            }

            return dic;
        }

        // 获取链接字符串
        private static string GetExcelConnection(string strFilePath)
        {
            //    if (!File.Exists(strFilePath))
            //    {
            //        throw new Exception("指定的Excel文件不存在！");
            //    }
            return
                 @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                 @"Data Source=" + strFilePath + ";" +
                 @"Extended Properties=" + Convert.ToChar(34).ToString() +
                 @"Excel 8.0;" + "Imex=2;HDR=Yes;" + Convert.ToChar(34).ToString();
        }

        //获取指定列的类型对应数据库的字段类型名称
        private static string TypeToString(DataColumn dataColumn)
        {
            switch (dataColumn.DataType.Name)
            {
                case "String"://字符串
                    {
                        return "NVarChar";
                    }
                case "Double"://数字
                    {
                        return "Double";
                    }
                case "Decimal"://数字
                    {
                        return "Decimal";
                    }
                case "DateTime"://时间
                    {
                        return "NVarChar";
                    }
                case "Single":
                case "Float":
                    {
                        return "Single";
                    }
                case "Int16":
                    {
                        
                        return "SmallInt";
                    }
                case "Int32":
                    {
                        return "Integer";
                    }
                case "Int64":
                    {
                        return "BigInt";
                    }
                default://
                    {
                        return "NVarChar";
                    }
            }
        }
    }

    public class HandleExcelEventArgs : EventArgs
    {
        public HandleExcelEventArgs(ISheet currentSheet, IRow currentExcelRow, DataRow sourceRow)
        {
            CurrentSheet = currentSheet;
            CurrentExcelRow = currentExcelRow;
            SourceRow = sourceRow;

            IsBreak = false;
        }

        public IWorkbook CurrentWorkBook { get { return CurrentSheet==null?null:CurrentSheet.Workbook; } }
        public ISheet CurrentSheet { private set; get; }
        public IRow CurrentExcelRow { private set; get; }

        public DataRow SourceRow { private set; get; }

        /// <summary>
        ///     是否退出所有导出过程
        /// </summary>
        public bool IsBreak { set; get; }
    }

    public class BeforeHandleExcelRowEventArgs : HandleExcelEventArgs
    {
        public BeforeHandleExcelRowEventArgs(ISheet currentSheet, IRow currentExcelRow, DataRow sourceRow)
            : base(currentSheet, currentExcelRow, sourceRow)
        {
            IsSkip = false;
        }

        /// <summary>
        ///     是否跳过当前行的导出,IsBreak 的判断优先于IsSkip
        /// </summary>
        public bool IsSkip { get; set; }
    }

    public static class DataRowExt
    {
        /// <summary>
        ///     获取指定列名的值,如果指定列名不存在,则返回默认值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="colName">指定列名</param>
        /// <param name="defautleValue">默认值</param>
        /// <returns></returns>
        public static object GetValue(this DataRow row, string colName, object defautleValue)
        {
            if (row.Table.Columns.Contains(colName))
            {
                return row[colName];
            }
            else
            {
                return defautleValue;
            }
        }

        /// <summary>
        ///     获取指定列名的值,并转换为Int类型
        /// </summary>
        /// <param name="row"></param>
        /// <param name="colName">指定列名</param>
        /// <returns></returns>
        public static int GetInt(this DataRow row,string colName)
        {
            return GetInt(row, colName, default(int));
        }

        /// <summary>
        ///     获取指定列名的值,并转换为Int类型
        /// </summary>
        /// <param name="row"></param>
        /// <param name="colName">指定列名</param>
        /// <param name="defaultValue">默认值 </param>
        /// <returns></returns>
        public static int GetInt(this DataRow row,string colName,int defaultValue=0)
        {
            if (!row.Table.Columns.Contains(colName))
            {
                return defaultValue;
            }

            return row[colName].ToInt(defaultValue);
        }

        public static int GetInt(this DataRow row, int colIndex)
         {
             return GetInt(row, colIndex, default(int));
         }

        public static int GetInt(this DataRow row, int colIndex, int defaultValue=0)
        {
            if (row.Table.Columns.Count<colIndex)
            {
                return defaultValue;
            }

            return row[colIndex].ToInt(defaultValue);
        }

        public static string GetString(this DataRow row, string colName)
        {
            return GetString(row, colName, default(string));
        }

        public static string GetString(this DataRow row, string colName, string defaultValue="")
        {
            if (!row.Table.Columns.Contains(colName))
            {
                return defaultValue;
            }

            return row[colName].ToStringEx(defaultValue);
        }

        public static string GetString(this DataRow row, int colIndex)
        {
            return GetString(row, colIndex, default(string));
        }

        public static string GetString(this DataRow row, int colIndex, string defaultValue)
        {
            if (row.Table.Columns.Count < colIndex)
            {
                return defaultValue;
            }

            return row[colIndex].ToStringEx(defaultValue);
        }



    }

    /// <summary>
    ///     调用NPOI,以数据流的方式操作Excel文件
    /// </summary>
    public  class ExcelUtil
    {
        //NIOP 中excel的每一列的宽度因子,,
       private const int ExcelColWidthFactor = 256;

        /// <summary>
       ///     将DataTable导出为Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="tableName">导出到excel时,Excel的表名</param>
        /// <param name="exportMapping">列映射关系, [Excel输出列名,DataTable对应列名]</param>
        /// <param name="beforeExportPreRow">导出每一行前的回调</param>
        /// <param name="afterExportPreRow">导出每一行后的回调</param>
        /// <param name="afterExportAllRow">导出完所有数据行的回调</param>
        /// <returns></returns>
        public static bool ExportToExcel(DataTable dt, Stream targetExportStream, string tableName, Dictionary<string, string> exportMapping=null, EventHandler<BeforeHandleExcelRowEventArgs> beforeExportPreRow=null, EventHandler<HandleExcelEventArgs> afterExportPreRow=null, EventHandler<HandleExcelEventArgs> afterExportAllRow=null)
        {
            if (exportMapping == null)
            {
                exportMapping = new Dictionary<string, string>(dt.Columns.Count);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    exportMapping.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                }
            }

            tableName = tableName ?? dt.TableName;

            if (String.IsNullOrWhiteSpace(tableName))
            {
                tableName = "sheet";
            }

            IWorkbook hssfworkbook = new HSSFWorkbook();

            var sheetList = new ISheet[(int)Math.Ceiling((decimal)(dt.Rows.Count / 65534)).ToMinInt(1)];

            for (int j = 0; j < (int)Math.Ceiling((decimal)(dt.Rows.Count / 65534)) + 1; j++)
            {
                sheetList[j] = hssfworkbook.CreateSheet(String.Format("{0}_{1}", tableName, j));
            }

            for (int i = 0; i < sheetList.Length; i++)
            {
                var tempSheet = sheetList[i];

                var headRow = tempSheet.CreateRow(0);

                int tempColIndex = 0;
                foreach (var mapping in exportMapping)
                {
                    sheetList[i].SetColumnWidth(tempColIndex, 19 * ExcelColWidthFactor);
                    headRow.CreateCell(tempColIndex).SetCellValue(mapping.Value);

                    tempColIndex += 1;
                }
            }

            if (dt.Rows.Count<=0)
            {
                return true;
            }

            ISheet currentSheet = sheetList[0];
            int currentSheetIndex = 0;
            int currentExportingRowIndex = 1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 65534)
                {
                    currentSheetIndex += 1;

                    currentSheet = sheetList[currentSheetIndex];

                    currentExportingRowIndex = 1;
                }

                var row = dt.Rows[i];
                var excelRow = currentSheet.CreateRow(currentExportingRowIndex);


                if (beforeExportPreRow != null)
                {
                    var e = new BeforeHandleExcelRowEventArgs(currentSheet, excelRow, row);
                    beforeExportPreRow(hssfworkbook, e);

                    if (e.IsBreak)
                    {
                        break;
                    }


                    if (e.IsSkip)
                    {
                        continue;
                    }
                }

                var currentCellIndex = 0;
                foreach (var mapping in exportMapping)
                {
                    excelRow.CreateCell(currentCellIndex).SetCellValue(row[mapping.Key].ToStringEx());
                    currentCellIndex += 1;
                }

                currentExportingRowIndex += 1;

                if (afterExportPreRow != null)
                {
                    var e = new HandleExcelEventArgs(currentSheet, excelRow, row);
                    afterExportPreRow(hssfworkbook, e);

                    if (e.IsBreak)
                    {
                        break;
                    }

                }
            }

            if (afterExportAllRow != null)
            {
                var e = new HandleExcelEventArgs(currentSheet, null, null);

                afterExportAllRow(hssfworkbook, e);

                if (e.IsBreak)
                {
                    return false;
                }
            }

            try
            {
                hssfworkbook.Write(targetExportStream);
            }
            catch (Exception)
            {
                return false;
            }


            return true;
        }

        /// <summary>
        ///     从Excel数据流中所有表导入到DataSet中
        /// </summary>
        /// <param name="stream">Excel数据源的数据流</param>
        /// <param name="targetds">导出的目标DataSet对象</param>
        /// <param name="exportMapping">列映射关系, [Excel输出列名,DataTable对应列名]</param>
        /// <param name="beforeImportPreRow">导入每一行前的回调</param>
        /// <param name="afterImportPreRow">导入每一行后的回调</param>
        /// <param name="afterImportAllRow">导入完所有数据行的回调</param>
        public static void ImportFromExcel(Stream stream, DataSet targetds, Dictionary<string, string> exportMapping, EventHandler<BeforeHandleExcelRowEventArgs> beforeImportPreRow, EventHandler<HandleExcelEventArgs> afterImportPreRow, EventHandler<HandleExcelEventArgs> afterImportAllRow)
        {
            IWorkbook workbook=new HSSFWorkbook(stream);

            if (workbook.NumberOfSheets<=0)
            {
                return ;
            }

            targetds.Clear();

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                var dt = new DataTable();

                importFromExcel(workbook.GetSheetAt(i), dt, exportMapping, beforeImportPreRow, afterImportAllRow, afterImportAllRow);

                targetds.Tables.Add(dt);
            }
        }

        /// <summary>
        ///     从Excel数据流中导入指定名称的表到DataTable中,并返回DataTable对象
        /// </summary>
        /// <param name="fileStream">Excel数据源的数据流</param>
        /// <param name="sheetName">将要导入的源表名</param>
        /// <param name="exportMapping">列映射关系, [Excel输出列名,DataTable对应列名]</param>
        /// <param name="beforeImportPreRow">导入每一行前的回调</param>
        /// <param name="afterImportPreRow">导入每一行后的回调</param>
        /// <param name="afterImportAllRow">导入完所有数据行的回调</param>
        /// <returns></returns>
        public static DataTable ImportFromExcel(Stream fileStream, string sheetName = null, Dictionary<string, string> exportMapping = null, EventHandler<BeforeHandleExcelRowEventArgs> beforeImportPreRow = null, EventHandler<HandleExcelEventArgs> afterImportPreRow = null, EventHandler<HandleExcelEventArgs> afterImportAllRow = null)
        {
            if (fileStream==null)
            {
                throw new ArgumentNullException("fileStream");
            }

            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new ArgumentNullException("sheetName");
            }

            IWorkbook workbook = null;

            try
            {
                workbook = new HSSFWorkbook(fileStream);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("fileStream");
            }

            ISheet sheet = null;

            try
            {
                sheet = workbook.GetSheet(sheetName);
            }
            catch (Exception)
            {
            }

            if (sheet==null)
            {
                throw new ArgumentOutOfRangeException("sheetName");
            }

            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheetName = workbook.GetSheetName(0);
            }

            var tbl = new DataTable(sheetName);

            importFromExcel(sheet, tbl, exportMapping, beforeImportPreRow, afterImportPreRow, afterImportAllRow);

            return tbl;
        }

        /// <summary>
        ///     从Excel数据流中导入指定名称的表到DataTable中,并返回DataTable对象
        /// </summary>
        /// <param name="fileData"> Excel数据源</param>
        /// <param name="startIndex">Byte数据的起始编码</param>
        /// <param name="count">有效数据的长度</param>
        /// <param name="sheetName">将要导入的源表名</param>
        /// <param name="exportMapping">列映射关系, [Excel输出列名,DataTable对应列名]</param>
        /// <param name="beforeImportPreRow">导入每一行前的回调</param>
        /// <param name="afterImportPreRow">导入每一行后的回调</param>
        /// <param name="afterImportAllRow">导入完所有数据行的回调</param>
        /// <returns>返回导入后的数据</returns>
        public static DataTable ImportFromExcel(byte[] fileData,int startIndex,int count, string sheetName, Dictionary<string, string> exportMapping, EventHandler<BeforeHandleExcelRowEventArgs> beforeImportPreRow, EventHandler<HandleExcelEventArgs> afterImportPreRow, EventHandler<HandleExcelEventArgs> afterImportAllRow)
        {
            var stream = new ByteStream(fileData, startIndex,count);

            return ImportFromExcel(stream, sheetName, exportMapping, beforeImportPreRow, afterImportPreRow, afterImportPreRow);
        }

        /// <summary>
        ///     从Excel数据流中导入指定名称的表到DataTable中,并返回DataTable对象
        /// </summary>
        /// <param name="fileData"> Excel数据源</param>
        /// <param name="sheetName">将要导入的源表名</param>
        /// <param name="exportMapping">列映射关系, [Excel输出列名,DataTable对应列名]</param>
        /// <param name="beforeImportPreRow">导入每一行前的回调</param>
        /// <param name="afterImportPreRow">导入每一行后的回调</param>
        /// <param name="afterImportAllRow">导入完所有数据行的回调</param>
        /// <returns>返回导入后的数据</returns>
        public static DataTable ImportFromExcel(byte[] fileData, string sheetName, Dictionary<string, string> exportMapping, EventHandler<BeforeHandleExcelRowEventArgs> beforeImportPreRow, EventHandler<HandleExcelEventArgs> afterImportPreRow, EventHandler<HandleExcelEventArgs> afterImportAllRow)
        {
            var stream = new ByteStream(fileData);

            return ImportFromExcel(stream, sheetName, exportMapping, beforeImportPreRow, afterImportPreRow, afterImportPreRow);
        }

        private static void importFromExcel(ISheet excelSheet, DataTable table, Dictionary<string, string> exportMapping, EventHandler<BeforeHandleExcelRowEventArgs> beforeExportPreRow, EventHandler<HandleExcelEventArgs> afterExportPreRow, EventHandler<HandleExcelEventArgs> afterExportAllRow)
        {
            if (excelSheet.LastRowNum<0)
            {
                return;
            }

            IRow headerRow = excelSheet.GetRow(0);

            if (headerRow==null)
            {
                return;
            }

            var startColIndex = headerRow.FirstCellNum;
            var endColIndex = headerRow.LastCellNum;

            if (endColIndex < 0)
            {
                return;
            }

            if (exportMapping==null)
            {
                exportMapping = new Dictionary<string, string>(endColIndex - startColIndex);
            }

            if (exportMapping.Count<=0)
            {
                for (int i = startColIndex; i < endColIndex; i++)
                {
                    var headerTitle = headerRow.GetCell(i).StringCellValue;

                    exportMapping.Add(headerTitle, headerTitle);
                }
            }

            for (int i = startColIndex; i < endColIndex; i++)
            {
                var headerTitle = headerRow.GetCell(i).StringCellValue;

                if (!exportMapping.ContainsKey(headerTitle)) continue;

                DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                column.DataType = cellTypeToType(headerRow.GetCell(0).CellType);

                table.Columns.Add(column);
            }

            for (int i = excelSheet.FirstRowNum+1; i < excelSheet.LastRowNum; i++)
            {
                var excelRow = excelSheet.GetRow(i);

                if (excelRow == null)
                {
                    continue;
                }

                var dataRow = table.NewRow();

                if (beforeExportPreRow!=null)
                {
                    var e = new BeforeHandleExcelRowEventArgs(excelSheet, excelRow, dataRow);

                    beforeExportPreRow(excelSheet.Workbook, e);

                    if (e.IsBreak)
                    {
                        break;
                    }

                    if (e.IsSkip)
                    {
                        continue;
                    }
                }

                //导出所有列
                for (int j = startColIndex; j < endColIndex; j++)
                {
                    var cell = excelRow.GetCell(j);

                    if (cell == null)
                    {
                        continue;
                    }

                    var title = headerRow.GetCell(j).StringCellValue;

                    if (exportMapping.ContainsKey(title))
                    {
                        if (cell.CellType == CellType.Blank || cell.CellType == CellType.Unknown)
                        {
                            dataRow[exportMapping[title]] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                dataRow[exportMapping[title]] = getCellValue(cell);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }   
                    }
                }

                table.Rows.Add(dataRow);

                if (afterExportPreRow!=null)
                {
                    var e = new HandleExcelEventArgs(excelSheet, excelRow, dataRow);

                    afterExportPreRow(excelSheet.Workbook, e);

                    if (e.IsBreak)
                    {
                        break;
                    }
                }
            }

            if (afterExportAllRow!=null)
            {
                var e = new HandleExcelEventArgs(excelSheet, null, null);

                afterExportAllRow(excelSheet.Workbook, e);
            }
        }

        private static object getCellValue(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Blank:
                    return  null;
                    break;
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                    break;
                case CellType.Numeric:

                    //if(DateUtil.IsCellDateFormatted(cell))
                    //{
                    //    return cell.DateCellValue;
                    //}
                    //else
                    //{
                    //    return cell.NumericCellValue;
                    //}
                    var str = cell.ToStringEx();

                    DateTime tempdate;
                    if (DateTime.TryParse(str, out tempdate))
                    {
                        return tempdate;
                    }
                    else
                    {
                        return cell.NumericCellValue;
                    }
                    //return  cell.ToString();    //This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number.
                    break;
                case CellType.String:
                    return cell.StringCellValue;
                    break;
                case CellType.Error:
                    return  cell.ErrorCellValue.ToString();
                    break;
                case CellType.Formula:
                    return cell.StringCellValue;
                default:
                    return  cell.ToStringEx();
                    break;
            }
        }

        private  static Type cellTypeToType(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Blank:
                    return typeof(object);
                    //dr[i] = "[null]";
                    break;
                case CellType.Boolean:
                    return typeof (bool);
                    //dr[i] = cell.BooleanCellValue;
                    break;
                case CellType.Numeric:
                    return typeof (object);
                    //dr[i] = cell.ToString();    //This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number.
                    break;
                case CellType.String:
                    return typeof (string);
                    break;
                case CellType.Error: 
                    //dr[i] = cell.ErrorCellValue;
                    return typeof (string);
                    break;
                case CellType.Formula:  //公式
                    return typeof (string);
                default:
                    return typeof(object);
                    break;
            }
        }

        //private static DataTable getDataTableFromExcel03()
        //{
        //}

        //private static DataTable getDataTableFromExcel07(Stream stream, 
        //                                                string sheetName, 
        //                                                Dictionary<string, string> exportMapping, 
        //                                                EventHandler<BeforeHandleExcelRowEventArgs> beforeImportPreRow, 
        //                                                EventHandler<HandleExcelEventArgs> afterImportPreRow, 
        //                                                EventHandler<HandleExcelEventArgs> afterImportAllRow,
        //                                                bool hasHeader = true)
        //{
        //    using (var pck = new OfficeOpenXml.ExcelPackage())
        //    {
        //        try
        //        {
        //            pck.Load(stream);
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
                
        //        var fromRow = hasHeader?1:0;
        //        var fromColumn =hasHeader?1:0;
        //        var toColumn = 0;

        //        var ws = pck.Workbook.Worksheets.First();
        //        DataTable tbl = new DataTable();
        //        toColumn =ws.Dimension.End.Column;

        //        for (int i = 1; i < toColumn; i++)
        //        {
        //            var cell = ws.Cells[fromRow, i, fromRow, i].First();

        //            var columnName = "";
        //            var cellValue = cell.Value.ToStringEx();


        //        }

        //        foreach (var firstRowCell in ws.Cells[fromRow, fromColumn, fromRow, toColumn])
        //        {
        //            var columnName = "";
        //            var cellValue = firstRowCell.Value;
        //            if (hasHeader)
        //            {
        //                if (exportMapping.HasData())
        //                {
                            
        //                }                        
        //            }


        //            tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
        //        }
        //        var startRow = hasHeader ? fromRow + 1 : fromRow;
        //        for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
        //        {
        //            var wsRow = ws.Cells[rowNum, fromColumn, rowNum, toColumn];
        //            var row = tbl.NewRow();
        //            foreach (var cell in wsRow)
        //            {
        //                row[cell.Start.Column - fromColumn] = cell.Text;
        //            }
        //            tbl.Rows.Add(row);
        //        }
        //        return tbl;
        //    }
        //}
    }


}
