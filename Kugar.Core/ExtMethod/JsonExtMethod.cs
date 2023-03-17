using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.ExtMethod
{
    public static class JsonExtMethod
    {
        public static long GetLong(this JObject src, string propertyName, long defaultValue = 0L, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                return (long)v;
            }
            else
            {
                return defaultValue;
            }
        }
        
        
        public static string GetString(this JObject src, string propertyName, string defaultValue = "")
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, StringComparison.Ordinal, out v))
            {
                return v.ToStringEx();
            }
            else
            {
                return defaultValue;
            }
        }


        public static string GetString(this JObject src, string propertyName, string defaultValue  , StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                return v.ToStringEx();
            }
            else
            {
                return defaultValue;
            }
        }

        public static int GetInt(this JObject src, string propertyName, int defaultValue = 0,StringComparison comparison= StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName,comparison, out v))
            {
                if (v.Type== JTokenType.Integer)
                {
                    return (int) v;
                }

                return v.ToInt(defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static JObject GetJObject(this JObject src, string propertyName, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return null;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Object)
                {
                    return (JObject)v;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        public static JArray GetJArray(this JObject src, string propertyName, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return null;
            } 

             JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Array)
                {
                    return (JArray)v;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<JObject> GetJObjectArray(this JObject src, string propertyName, StringComparison comparison = StringComparison.Ordinal)
        {
            var array = GetJArray(src, propertyName);

            if (array==null)
            {
                yield break;
            }
            else
            {
                foreach (var item in array)
                {
                    yield return (JObject) item;
                }
            }
        }

#if !NETCOREAPP
        [Obsolete("函数名错误,请使用GetJObjectArray函数")]
        public static IEnumerable<JObject> GetJOjbectArray(this JObject src, string propertyName)
        {
            var array = GetJArray(src, propertyName);

            if (array==null)
            {
                yield break;
            }
            else
            {
                foreach (var item in array)
                {
                    yield return (JObject) item;
                }
            }
        }
#endif


        public static float GetFloat(this JObject src, string propertyName, float defaultValue = 0, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Float)
                {
                    return (float)v;
                }

                return v.ToFloat(defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static double GetDouble(this JObject src, string propertyName, double defaultValue = 0, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type==JTokenType.Float)
                {
                    return (double) v;
                }
                return v.ToDouble(defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static decimal GetDecimal(this JObject src, string propertyName, decimal defaultValue = 0, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Object)
                {
                    var o = (JObject)v;

                    return o["Value"].ToDecimal();
                }
                else if (v.Type == JTokenType.String)
                {
                    return v.ToStringEx().ToDecimal();
                }
                else
                {
                    return v.ToDecimal(defaultValue);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public static T GetIntEnum<T>(this JObject src, string propertyName, T defaultValue, StringComparison comparison = StringComparison.Ordinal)
        {
            var v = GetInt(src, propertyName, (int)Convert.ToInt32(defaultValue), comparison);

            if (Enum.IsDefined(typeof(T), v))
            {
                return (T)Enum.ToObject(typeof(T), v);
            }
            else
            {
                return defaultValue;
            }
        }

        public static DateTime GetDateTime(this JObject src, string propertyName, DateTime defaultValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Date)
                {
                    return (DateTime)v;
                }
            }
            return defaultValue;

        }

        public static DateTime GetDateTime(this JObject src, string propertyName,string format,DateTime defaultValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Date)
                {
                    return (DateTime)v;
                }
                else
                {
                    return v.ToStringEx().ToDateTime(format, defaultValue);
                }
            }
            return defaultValue;
        }

        public static DateTime? GetDateTimeNullable(this JObject src, string propertyName, DateTime? defaultValue=null, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Date)
                {
                    return (DateTime)v;
                }
            }
            return defaultValue;

        }

        public static DateTime? GetDateTimeNullable(this JObject src, string propertyName, string format, DateTime? defaultValue=null, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type == JTokenType.Date)
                {
                    return (DateTime)v;
                }
                else
                {
                    return v.ToStringEx().ToDateTimeNullable(format, defaultValue);
                }
            }
            return defaultValue;
        }

        public static bool GetBool(this JObject src, string propertyName, bool defaultValue = false, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!src.HasValues)
            {
                return defaultValue;
            }

            JToken v;

            if (src.TryGetValue(propertyName, comparison, out v))
            {
                if (v.Type==JTokenType.Boolean)
                {
                    return (bool) v;
                }

                return v.ToBool(defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static T GetValue<T>(this JObject src, string propertyName,T defaultValue=default(T), StringComparison comparison = StringComparison.Ordinal)
        {
            JToken token = null;

            if (src.TryGetValue(propertyName, comparison,out token))
            {
                return token.ToObject<T>();
            }
            else
            {
                return defaultValue;
            }
        }


        public static string ToStringEx(this JObject src, Formatting formatting = Formatting.Indented)
        {
            if (src==null)
            {
                return "{}";
            }
            else
            {
                return src.ToString(formatting);
            }
        }

        public static JArray ToJArray<T>(this IEnumerable<T> src)
        {
            if (src==null)
            {
                return new JArray();
            }
            else
            {
                return new JArray(src);
            }
        }

        public static JObject AddPropertyIf(this JObject json, bool isNeedAdd, string propertyName, JToken value)
        {
            if (isNeedAdd)
            {
                json.Add(propertyName,value);
            }

            return json;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ToPrimitiveValue(this JToken value)
        {
            if (value.Type == JTokenType.Integer)
            {
                return (int)value;
            }
            else if (value.Type == JTokenType.Boolean)
            {
                return (bool)value;
            }
            else if (value.Type == JTokenType.Date)
            {
                return (DateTime)value;
            }
            else if (value.Type == JTokenType.Float)
            {
                return (float)value;
            }
            else if (value.Type == JTokenType.String)
            {
                return (string)value;
            }
            else if (value.Type == JTokenType.Bytes)
            {
                return (byte[])value;
            }
            else if (value.Type == JTokenType.Null || value.Type == JTokenType.None)
            {
                return null;
            }

            else if (value.Type == JTokenType.Guid)
            {
                return (Guid)value;
            }

            return null;
        }

        public static Type GetPrimitiveType(this JToken value)
        {
            if (value.Type == JTokenType.Integer)
            {
                return typeof(int);
            }
            else if (value.Type == JTokenType.Boolean)
            {
                return typeof(bool);
            }
            else if (value.Type == JTokenType.Date)
            {
                return typeof(DateTime);
            }
            else if (value.Type == JTokenType.Float)
            {
                return typeof(float); 
            }
            else if (value.Type == JTokenType.String)
            {
                return typeof(string);
            }
            else if (value.Type == JTokenType.Bytes)
            {
                return typeof(byte[]);
            }
            else if (value.Type == JTokenType.Null || value.Type == JTokenType.None)
            {
                return typeof(object);
            }

            else if (value.Type == JTokenType.Guid)
            {
                return typeof(Guid);
            }

            return null;
        }

        public static JTokenType ToJsonObjectType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(int) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(long) ||
                type == typeof(uint) ||
                type == typeof(ushort) ||
                type == typeof(ulong) ||
                type.IsEnum
               )
            {
                return JTokenType.Integer;
            }
            else if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            {
                return JTokenType.Float;
            }
            else if (type == typeof(string))
            {
                return JTokenType.String;
            }
            else if (type == typeof(bool))
            {
                return JTokenType.Boolean;
            }
            else if (type.IsIEnumerable())
            {
                return JTokenType.Array;
            }
            else if (type == typeof(DateTime))
            {
                return JTokenType.String;
            }
            else if (type == typeof(Guid))
            {
                return JTokenType.String;
            }
            else
            {
                return JTokenType.Object;
            }
        }
        
        /// <summary>
        /// 字符串值转为json的JToken类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static JToken ToJsonToken(this string value )
        {
            if (value==null)
            {
                return null;
            }

            if (GeneralRegex.IsInt(value))
            {
                return value.ToInt();
            }
            else if (GeneralRegex.IsNumber(value))
            {
                return value.ToFloat();
            }
            else if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase) ||
                     value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            {
                return value.ToBool();
            }
            else if (GeneralRegex.IsGuid(value))
            {
                return Guid.Parse(value);
            }
            else
            {
                return value;
            }
        }


        #region WriterExt

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, string value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, int? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, long? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, float? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, bool? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, ushort? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, byte? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, decimal? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, DateTimeOffset? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, TimeSpan? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, Uri value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, object value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, byte[] value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, Guid? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, DateTime? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, sbyte? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, char? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, short? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, double? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, ulong? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, uint? value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, TimeSpan value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, ulong value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, float value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, double value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, short value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }


        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, ushort value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, char value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, sbyte value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, decimal value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, DateTime value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, Guid value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, DateTimeOffset value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, byte value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, bool value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, long value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, uint value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        public static JsonWriter WriteProperty(this JsonWriter writer, string propertyName, int value)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteValue(value);

            return writer;
        }

        #endregion
        
        #if NETCOREAPP
        

        #region WriterExt

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, string value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, int? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, long? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, float? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, bool? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, ushort? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, byte? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, decimal? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, DateTimeOffset? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, TimeSpan? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, Uri value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, object value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, byte[] value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, Guid? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, DateTime? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, sbyte? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, char? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, short? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, double? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, ulong? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, uint? value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, TimeSpan value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, ulong value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, float value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, double value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, short value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }


        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, ushort value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, char value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, sbyte value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, decimal value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, DateTime value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, Guid value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, DateTimeOffset value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, byte value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, bool value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, long value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, uint value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        public static async Task<JsonWriter>  WritePropertyAsync(this JsonWriter writer, string propertyName, int value)
        {
            await writer.WritePropertyNameAsync(propertyName);
            await writer.WriteValueAsync(value);

            return writer;
        }

        #endregion
        
        #endif
                
    }
    
}
