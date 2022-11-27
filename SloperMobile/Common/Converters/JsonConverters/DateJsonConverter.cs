using System;
using Newtonsoft.Json;

namespace SloperMobile
{
    public class DateJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || !DateTime.TryParse(reader.Value.ToString(), out var date))
                return null;
            return date;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
