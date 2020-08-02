using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Utils;
using System;

namespace DanceClass.Utils
{
    public class EnumToKeyValuePairConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string description = ((Enum)value).GetDescription();

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(description);
            writer.WritePropertyName("value");
            writer.WriteValue(value);
            writer.WriteEndObject();
        }
    }
}