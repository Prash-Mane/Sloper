using System;
using Newtonsoft.Json;

namespace SloperMobile
{
    public class IntJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || !int.TryParse(reader.Value.ToString(), out var number))
                return 0;
            return number;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
