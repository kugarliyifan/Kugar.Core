using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

#if NET45
    using System.Runtime.Serialization.Formatters.Soap;  
#endif


using System.Text;
using System.Xml.Serialization;

namespace Kugar.Core.ExtMethod.Serialization
{
    public static class SerializationExt
    {
        #region "二进制"

        public static byte[] SerializeToByte(this object obj)
        {
            var bm = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bm.Serialize(ms,obj);

                return ms.ToArray();
            }



        }



        public static string SerializeToString(this object obj)
        {
            return Convert.ToBase64String(SerializeToByte(obj));
        }


        public static object DeserializeToObject(this byte[] buf)
        {
            return DeserializeToObject(buf, 0, buf.Length);
        }

        public static object DeserializeToObject(this byte[] buf, int offset, int count)
        {
            var ret = DeserializeToObject<object>(new ArraySegment<byte>(buf,
                                                              offset,
                                                              count));

            return ret;
        }

        public static T DeserializeToObject<T>(this byte[] buf)
        {
            return DeserializeToObject<T>(buf, 0, buf.Length);
        }


        public static T DeserializeToObject<T>(this byte[] buf,int offset,int count)
        {
            var ret=DeserializeToObject<T>(new ArraySegment<byte>(buf,
                                                              offset,
                                                              count));

            return ret;
        }

        public static object DeserializeToObject(this ArraySegment<byte> buf)
        {
            return DeserializeToObject<object>(buf);
        }

        public static T DeserializeToObject<T>(this ArraySegment<byte> buf)
        {
            var bf = new BinaryFormatter();

            try
            {
                using (var ms = new MemoryStream(buf.Array, buf.Offset, buf.Count))
                {
                    var t=bf.Deserialize(ms);

                    return (T)Convert.ChangeType(t, typeof (T));
                }

            }
            catch (Exception)
            {
                return default(T);
            }


        }

        public static object DeserializeToObject(this string str)
        {
            return DeserializeToObject<object>(str);
        }

        public static T DeserializeToObject<T>(this string str)
        {
            byte[] buf = Convert.FromBase64String(str);

            try
            {
                return DeserializeToObject<T>(buf,0,buf.Length);
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

#if NET45
        #region "SOAP"

        /// <summary>
        ///     序列化对象到SOAP格式
        /// </summary>
        /// <param name="Object">要序列化的对象</param>
        /// <returns></returns>
        public static string SerializeToSOAP(this object Object)
         {
             if (Object == null)
                 throw new ArgumentException("Object can not be null");
             using (MemoryStream Stream = new MemoryStream())
             {
                 SoapFormatter Serializer = new SoapFormatter();
                 Serializer.Serialize(Stream, Object);
                 Stream.Flush();
                 return UTF8Encoding.UTF8.GetString(Stream.GetBuffer(), 0, (int)Stream.Position);
             }
         }

        /// <summary>
        ///     SOAP格式反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SOAP">soap格式的字符串</param>
        /// <returns></returns>
         public static T SOAPToObject<T>(string SOAP)
         {
             if (string.IsNullOrEmpty(SOAP))
                 throw new ArgumentException("SOAP can not be null/empty");
             using (MemoryStream Stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(SOAP)))
             {
                 SoapFormatter Formatter = new SoapFormatter();
                 return (T)Formatter.Deserialize(Stream);
             }
         }

        #endregion
#endif



        #region "XML"

        /// <summary>
        ///     序列化对象到XML字符串
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        public static string SerializeToXML(object Object)
         {
             if (Object == null)
                 throw new ArgumentException("Object can not be null");
             using (MemoryStream Stream = new MemoryStream())
             {
                 XmlSerializer Serializer = new XmlSerializer(Object.GetType());
                 Serializer.Serialize(Stream, Object);
                 Stream.Flush();
                 return UTF8Encoding.UTF8.GetString(Stream.GetBuffer(), 0, (int)Stream.Position);
             }
         }

        /// <summary>
        ///     xml格式字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="XML"></param>
        /// <returns></returns>
         public static T XMLToObject<T>(string XML)
         {
             if (string.IsNullOrEmpty(XML))
                 throw new ArgumentException("XML can not be null/empty");
             using (MemoryStream Stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(XML)))
             {
                 XmlSerializer Serializer = new XmlSerializer(typeof(T));
                 return (T)Serializer.Deserialize(Stream);
             }
         }


        #endregion

    }
}
