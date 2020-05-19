using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fasterflect;
using Newtonsoft.Json.Serialization;
using NPOI.SS.Formula.Functions;

namespace Kugar.Core.BaseStruct
{
    //[JsonConverter(typeof(VM_PagedList_JsonConverter))]
    /// <summary>
    /// 分页参数信息
    /// </summary>
    public interface IPagedInfo
    {
        /// <summary>
        /// 页码
        /// </summary>
        [DisplayName("页码"),Description("页码")]
        int PageIndex { set; get; }

        /// <summary>
        /// 每页大小
        /// </summary>
        [DisplayName("每页大小"), Description("每页大小")]
        int PageSize { set; get; }

        /// <summary>
        /// 总记录数
        /// </summary>
        [DisplayName("总记录数"), Description("总记录数")]
        int TotalCount { set; get; }

        /// <summary>
        /// 总页数
        /// </summary>
        [DisplayName("总页数"), Description("总页数")]
        int PageCount { get; }

        /// <summary>
        /// 是否有数据
        /// </summary>
        /// <returns></returns>
        bool HasData();
    }

    [JsonConverter(typeof(VM_PagedList_JsonConverter))]
    public interface IPagedList<out T>: IPagedInfo
    {
        /// <summary>
        /// 获取分页后的数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetData();

        //int PageIndex { set; get; }
        //int PageSize { set; get; }
        //int TotalCount { set; get; }
        //int PageCount { get; }
        IPagedList<TExport> Cast<TExport>(Func<T,TExport> castFunc);
        //bool HasData();
        T[] ToArray();
    }

    [Serializable]
    [JsonConverter(typeof(VM_PagedList_JsonConverter))]
    public class VM_PagedList<T> : ISerializable, IPagedList<T>
    {
        public VM_PagedList()
        {

        }

        public VM_PagedList(SerializationInfo info, StreamingContext context) : this()
        {
            this.PageIndex = info.GetInt32("PageIndex");
            this.PageSize = info.GetInt32("PageSize");
            this.TotalCount = info.GetInt32("TotalCount");
            this.Data = (T[])info.GetValue("Data", typeof(T[]));

        }

        public VM_PagedList(IEnumerable<T> data, int pageIndex = 1, int pageSize = 10, int totalCount = 0)
        {
            Data = data ?? new T[0];
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        [XmlArray]
        public IEnumerable<T> Data { set; get; }

        public IEnumerable<T> GetData()
        {
            return Data;
        }


        public int PageIndex { set; get; }

        public int PageSize { set; get; }

        public int TotalCount { set; get; }

        public int PageCount
        {
            get
            {
                if (TotalCount % PageSize > 0)
                {
                    return (TotalCount / PageSize) + 1;
                }
                else
                {
                    return TotalCount / PageSize;
                }
            }
        }

        IPagedList<TExport> IPagedList<T>.Cast<TExport>(Func<T, TExport> castFunc)
        {
            if (castFunc == null)
            {
                throw new ArgumentNullException("castFunc");
            }

            if (Data == null)
            {
                return new VM_PagedList<TExport>(new TExport[0], PageIndex, PageSize, TotalCount);
            }

            var lst=new List<TExport>();

            foreach (T item in GetData())
            {
                lst.Add(castFunc(item));
            }

            return new VM_PagedList<TExport>(lst, PageIndex, PageSize, TotalCount);
        }


        public VM_PagedList<TExport> Cast<TExport>(Func<T, TExport> castFunc)
        {
            if (castFunc == null)
            {
                throw new ArgumentNullException("castFunc");
            }

            if (Data == null)
            {
                return new VM_PagedList<TExport>(new TExport[0], PageIndex, PageSize, TotalCount);
            }


            return new VM_PagedList<TExport>(Data.Select(x => castFunc(x)), PageIndex, PageSize, TotalCount);
        }

        public VM_PagedList<TExport> Cast<TExport>(IEnumerable<TExport> newData)
        {
            return new VM_PagedList<TExport>(newData, PageIndex, PageSize, TotalCount);
        }

        public bool HasData()
        {
            return Data.HasData();
        }

        public T[] ToArray()
        {
            if (Data == null)
            {
                return new T[0];
            }
            else
            {
                return Data.ToArray();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PageIndex", PageIndex);
            info.AddValue("PageSize", PageSize);
            info.AddValue("TotalCount", TotalCount);
            info.AddValue("Data", Data.ToArrayEx());
        }

        public static VM_PagedList<T> Empty(int pageIndex, int pageSize)
        {
            return new VM_PagedList<T>(new T[0], pageIndex, pageSize, 0);
        }
    }

    //[AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = true)]
    public class VM_PagedList_JsonConverter : JsonConverter
    {
        private static ConcurrentDictionary<Type, PagedListDataTypeInfo> _cacheTypeProperties = new ConcurrentDictionary<Type, PagedListDataTypeInfo>();
        private static HashSet<string> _hash = new HashSet<string>()
        {
            "Data",
            "PageIndex",
            "PageSize",
            "TotalCount"
        };



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            var isCamelCase = false;

            if (serializer.ContractResolver is CamelCasePropertyNamesContractResolver  c)
            {
                isCamelCase = true;
                //if (c.NamingStrategy.ProcessDictionaryKeys == true)
                {
                    writer.WritePropertyName("data");
                }
            }
#if NETCOREAPP
            else if (serializer.ContractResolver is DefaultContractResolver d)
            {
                isCamelCase = d.NamingStrategy is CamelCaseNamingStrategy;

                writer.WritePropertyName(d.NamingStrategy?.GetPropertyName("data",true) ?? "Data");
            }
#endif

            else
            {
                writer.WritePropertyName("Data");
            }

            
            writer.WriteStartArray();
            
            var data = (IEnumerable) value.FastInvoke("GetData");

            foreach (var v in data)
            {
                serializer.Serialize(writer,v);

                //writer.WriteValue(v);
            }

            writer.WriteEndArray();

            
            writer.WriteProperty(isCamelCase? "pageIndex" : "PageIndex", (int)value.FastGetValue("PageIndex"));
            writer.WriteProperty(isCamelCase ? "pageSize": "PageSize", (int)value.FastGetValue("PageSize"));
            writer.WriteProperty(isCamelCase ? "totalCount": "TotalCount", (int)value.FastGetValue("TotalCount"));

            var extProperties = _cacheTypeProperties.GetOrAdd(value.GetType(), getExtPropertyNames);

            if (extProperties.ExtProperties.HasData())
            {
                foreach (var extProperty in extProperties.ExtProperties)
                {
                    writer.WritePropertyName(extProperty.Name);
                    serializer.Serialize(writer, value.GetPropertyValue(extProperty.Name));
                    //writer.WriteProperty(extProperty.Name, value.FastGetValue(extProperty.Name));
                }
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //return base.ReadJson(reader, objectType, existingValue, serializer);

            //return serializer.Deserialize(reader, objectType);

            var json = (JObject)JObject.ReadFrom(reader);

            var newValue = Activator.CreateInstance(objectType);
            
            var data = json.GetJArray("Data");

            var config = _cacheTypeProperties.GetOrAdd(objectType, getExtPropertyNames);

            if (data.HasData())
            {
                newValue.SetPropertyValue("Data",serializer.Deserialize(data.CreateReader(),config.InnerDataProperty.PropertyType));
            }

            newValue.SetPropertyValue("PageIndex",json.GetInt("PageIndex"));
            newValue.SetPropertyValue("PageSize",json.GetInt("PageSize"));
            newValue.SetPropertyValue("TotalCount",json.GetInt("TotalCount"));

            foreach (var property in config.ExtProperties)
            {
                if (json.TryGetValue(property.Name,out var tmpValue))
                {
                    newValue.SetPropertyValue(property.Name,serializer.Deserialize(tmpValue.CreateReader(),property.PropertyType));
                }
            }

            return newValue;
        }

        private PagedListDataTypeInfo getExtPropertyNames(Type type)
        {
            var lst = type.GetProperties();

            var model = new PagedListDataTypeInfo();

            var dataProerty = lst.FirstOrDefault(x => x.Name == "Data");

            model.ExtProperties = lst.Where(x => !_hash.Contains(x.Name) && x.CanWrite && x.CanRead).ToArray();
            model.InnerDataListType = dataProerty.PropertyType.GetGenericArguments()
                .First();
            model.InnerDataProperty = dataProerty;

            return model;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VM_PagedList<>) || objectType.IsSubclassOf(typeof(VM_PagedList<>));
        }

        private class PagedListDataTypeInfo
        {
            public Type InnerDataListType { set; get; }

            public PropertyInfo InnerDataProperty { set; get; }

            public PropertyInfo[] ExtProperties { set; get; }
        }
    }
}
