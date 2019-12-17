using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Kugar.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.ExtMethod
{
    public static class SerialObjectExt
    {
        public static T DeserializeToObject<T>(this string str)
        {
            return DeserializeToObject(str, default(T));
        }

        public static T DeserializeToObject<T>(this string str, T defaultValue)
        {
            return (T) DeserializeToObject(str,defaultValue);

            //T obj;

            //System.Runtime.Serialization.IFormatter bf = new BinaryFormatter();

            //byte[] byt = Convert.FromBase64String(str);

            //using (var ms = new System.IO.MemoryStream(byt, 0, byt.Length))
            //{
            //    try
            //    {
            //        obj = (T)bf.Deserialize(ms);

            //        if (obj.Equals(default(T)))
            //        {
            //            return defaultValue;
            //        }
            //        else
            //        {
            //            return obj;
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        return defaultValue;
            //    }


            //}


        }

        public static object DeserializeToObject(this string str,object defaultValue=null)
        {
            object obj;

            System.Runtime.Serialization.IFormatter bf = new BinaryFormatter();

            byte[] byt = Convert.FromBase64String(str);

            using (var ms = new System.IO.MemoryStream(byt, 0, byt.Length))
            {
                try
                {
                    obj = bf.Deserialize(ms);

                    if (obj==null)
                    {
                        return defaultValue;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (Exception)
                {
                    return defaultValue;
                }


            }
        }

        public static string SerializeToString(this object obj)
        {
            System.Runtime.Serialization.IFormatter bf = new BinaryFormatter();

            string result = string.Empty;

            using (var ms = new System.IO.MemoryStream())
            {

                bf.Serialize(ms, obj);

                byte[] byt = new byte[ms.Length];

                byt = ms.ToArray();

                result = Convert.ToBase64String(byt);

                ms.Flush();

            }

            return result;
        }


        private static JsonConverter _dtConverter = new JSONCustomDateConverter();
        public static string SerializeToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj,_dtConverter);
        }

        public static JObject DeserializeFromJsonString(this string str,JObject defaultValue=null)
        {
            try
            {
                return (JObject)JsonConvert.DeserializeObject(str);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static T DeserializeFromJsonString<T>(this string str, T defaultValue = default(T))
        {
            try
            {
                return (T)JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
