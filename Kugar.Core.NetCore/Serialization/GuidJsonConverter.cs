using System;
using System.Collections.Generic;
using System.Linq;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Core.Serialization
{
    public class GuidJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is IEnumerable<Guid> v1)
            {
                writer.WriteStartArray();
                foreach (var guid in v1)
                {
                    writer.WriteValue(guid.ToStringEx());
                }
                writer.WriteEndArray();
            }
            else if (value is IEnumerable<Guid?> v2)
            {
                writer.WriteStartArray();

                foreach (var guid in v2)
                {
                    if (guid.HasValue)
                    {
                        writer.WriteValue(guid.Value.ToStringEx());
                    }
                }

                writer.WriteEndArray();
            }
            else
            {
                if (value == null)
                {
                    writer.WriteValue("");
                }
                else
                {
                    writer.WriteValueAsync(((Guid)value).ToString());
                }
            }

        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {

            if (objectType.IsImplementlInterface(typeof(IEnumerable<Guid>)) || objectType.IsImplementlInterface(typeof(IEnumerable<Guid?>)))
            {
                var value = JArray.ReadFrom(reader);

                if (value == null)
                {
                    return null;
                }


                if (value.HasData())
                {
                    var lst = new List<Guid>();

                    foreach (var item in value)
                    {
                        lst.Add(convertToGuid(item.ToStringEx(), objectType).GetValueOrDefault(Guid.Empty));
                    }

                    if (objectType == typeof(Guid[]))
                    {
                        return lst.ToArrayEx();
                    }
                    else if (objectType == typeof(Guid?[]))
                    {
                        return lst.Select(x => (Guid?)x).ToArrayEx();
                    }

                    return lst;
                }
                else
                {
                    return objectType.GetDefaultValue();
                }

            }
            else
            {
                var value = reader.Value;

                return convertToGuid(value.ToStringEx(), objectType);
            }


        }

        private Guid? convertToGuid(string str, object objectType)
        {
            //var str = value.ToStringEx();

            if (string.IsNullOrWhiteSpace(str))
            {
                if (objectType == typeof(Guid?))
                {
                    return null;
                }
                else//if(objectType==typeof(Guid))
                {
                    return Guid.Empty;
                }
            }
            else
            {
                if (Guid.TryParse(str, out var v))
                {
                    return v;
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid) || objectType == typeof(Guid?) || objectType.IsImplementlInterface(typeof(IEnumerable<Guid>)) || objectType.IsImplementlInterface(typeof(IEnumerable<Guid?>));
        }
    }
}
