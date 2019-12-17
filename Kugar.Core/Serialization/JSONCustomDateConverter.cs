using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kugar.Core.Serialization
{
    public class JSONCustomDateConverter : DateTimeConverterBase
    {

        private string _dateFormat;

        public JSONCustomDateConverter():this("yyyy-MM-dd HH:mm:ss")
        { }

        public JSONCustomDateConverter(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return existingValue.ToString().ToDateTime(_dateFormat);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToDateTime(value).ToString(_dateFormat));
            writer.Flush();
        }
    }
}
