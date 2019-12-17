using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Kugar.Core.ExtMethod
{
    public static class ADOExt
    {
        public static DbParameterCollection SetValue(this System.Data.Common.DbParameterCollection collection, string paramName,
                                    object value)
        {
            collection[paramName].Value = value;

            return collection;
        }

        public static void SetValue(this DbParameterCollection collection, object entity)
        {
            var pList = entity.GetType().GetPropertyExecutorList();

            foreach (var propertyExecutorse in pList)
            {
                var tempParam = string.Format("@{0}", propertyExecutorse.Key);

                if (collection.Contains(tempParam))
                {
                    collection.SetValue(tempParam, propertyExecutorse.Value.GetValue(entity));
                }
            }
        }

        public static DbCommand AddParameters(this DbCommand cmd, IEnumerable<ADOCmdParam> lst)
        {
            if (lst == null)
            {
                throw new ArgumentNullException("lst");
            }

            foreach (var item in lst)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ParamName))
                {
                    AddParameter(cmd, item.ParamName, item.ValueType, item.Size, item.Value);
                }

            }

            return cmd;
        }

        public static DbCommand AddParameter(this DbCommand cmd, string paramName, DbType valueType, object value)
        {
            return AddParameter(cmd, paramName, valueType, -1, value);
        }

        public static DbCommand AddParameter(this DbCommand cmd, string paramName, DbType valueType, int size, object value)
        {
            DbParameter param = cmd.CreateParameter();

            param.DbType = valueType;
            param.ParameterName = paramName;

            if (size > 0)
            {
                param.Size = size;
            }

            param.Value = value;

            cmd.Parameters.Add(param);

            return cmd;
        }

        public static SqlParameter Copy(this SqlParameter src)
        {
            var t = new SqlParameter();

            t.ParameterName = src.ParameterName;
            t.Offset = src.Offset;
            t.CompareInfo = src.CompareInfo;
            t.DbType = src.DbType;
            t.Direction = src.Direction;
            t.IsNullable = src.IsNullable;
            t.LocaleId = src.LocaleId;
            t.Offset = src.Offset;
            t.Precision = src.Precision;
            t.Scale = src.Scale;
            t.Size = src.Size;
            t.SourceVersion = src.SourceVersion;
            t.SqlDbType = src.SqlDbType;

            return t;
        }

        /// <summary>
        ///     循环读取DataReader中的行
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="action"></param>
        public static void Foreach(this DbDataReader reader, Action<DbDataReader> action)
        {
            while (reader.Read())
            {
                action(reader);
            }
        }

        /// <summary>
        ///     将DataReader转换成只前进的IEnumerable接口
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<DbDataReader> AsEnumerable(this DbDataReader reader)
        {
            return new DbDataReaderEnumerable(reader);
        }

        public static bool IsDBNull(this IDataReader dr, string colName)
        {
            return dr.IsDBNull(dr.GetOrdinal(colName));
        }

        #region 获取DbType与Type之间的转换 GetDBParameterType

        private static Lazy<Dictionary<Type, DbType>> _dBTypeMap = new Lazy<Dictionary<Type, DbType>>(loadDBTypeMap);

        public static Type GetDbParameterType(this DbType dbType)
        {
            return _dBTypeMap.Value.FirstOrDefault(x => x.Value == dbType).Key;
        }

        public static DbType GetDbParameterType(this Type type)
        {
            return _dBTypeMap.Value[type];
        }

        private static Dictionary<Type, DbType> loadDBTypeMap()
        {
            var typeMap = new Dictionary<Type, DbType>()
                              {
                                    [typeof (byte)] = DbType.Byte,
                                    [typeof (sbyte)] = DbType.SByte,
                                    [typeof (short)] = DbType.Int16,
                                    [typeof (ushort)] = DbType.UInt16,
                                    [typeof (int)] = DbType.Int32,
                                    [typeof (uint)] = DbType.UInt32,
                                    [typeof (long)] = DbType.Int64,
                                    [typeof (ulong)] = DbType.UInt64,
                                    [typeof (float)] = DbType.Single,
                                    [typeof (double)] = DbType.Double,
                                    [typeof (decimal)] = DbType.Decimal,
                                    [typeof (bool)] = DbType.Boolean,
                                    [typeof (string)] = DbType.String,
                                    [typeof (char)] = DbType.StringFixedLength,
                                    [typeof (Guid)] = DbType.Guid,
                                    [typeof (DateTime)] = DbType.DateTime,
                                    [typeof (DateTimeOffset)] = DbType.DateTimeOffset,
                                    [typeof (byte[])] = DbType.Binary,
                                    [typeof (byte?)] = DbType.Byte,
                                    [typeof (sbyte?)] = DbType.SByte,
                                    [typeof (short?)] = DbType.Int16,
                                    [typeof (ushort?)] = DbType.UInt16,
                                    [typeof (int?)] = DbType.Int32,
                                    [typeof (uint?)] = DbType.UInt32,
                                    [typeof (long?)] = DbType.Int64,
                                    [typeof (ulong?)] = DbType.UInt64,
                                    [typeof (float?)] = DbType.Single,
                                    [typeof (double?)] = DbType.Double,
                                    [typeof (decimal?)] = DbType.Decimal,
                                    [typeof (bool?)] = DbType.Boolean,
                                    [typeof (char?)] = DbType.StringFixedLength,
                                    [typeof (Guid?)] = DbType.Guid,
                                    [typeof (DateTime?)] = DbType.DateTime,
                                    [typeof (DateTimeOffset?)] = DbType.DateTimeOffset,
                                    [typeof (Binary)] = DbType.Binary

                              };


            return typeMap;
        }

        #endregion

        private static Lazy<TypeConverter> _sqlDBTypeConvert = new Lazy<TypeConverter>(getSqlDBTypeConvert);

        public static SqlDbType GetSQLDbType(Type type)
        {
            if (_sqlDBTypeConvert.Value.CanConvertFrom(type))
            {
                return (SqlDbType) _sqlDBTypeConvert.Value.ConvertFrom(type);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        private static TypeConverter getSqlDBTypeConvert()
        {
            SqlParameter p1 = new SqlParameter();
            TypeConverter tc = TypeDescriptor.GetConverter(p1.DbType);

            return tc;
        }

        internal class DbDataReaderEnumerable:IEnumerable<DbDataReader>
        {
            private DbDataReader _reader = null;

            public DbDataReaderEnumerable(DbDataReader reader)
            {
                _reader = reader;
            }

            public IEnumerator<DbDataReader> GetEnumerator()
            {
                while (_reader.Read())
                {
                    yield return _reader;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
    }



    public class ADOCmdParam
    {
        public ADOCmdParam(string paramName,DbType valueType,object value):this(paramName,valueType,-1,value)
        {

        }

        public ADOCmdParam(string paramName,DbType valueType,int size,object value)
        {
            ParamName=paramName;
            ValueType=valueType;
            Size=size;
            Value=value;
        }

        public string ParamName;
        public DbType ValueType;
        public int Size;
        public object Value;
    }
}
