using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RazzleServer.Wz.Util
{
    public class WzImagePropertyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(WzImageProperty);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var typeToken = token["Type"];
            if (typeToken == null)
            {
                throw new InvalidOperationException("invalid object");
            }

            var actualType = WzImagePropertyMapper.GetType(typeToken.ToObject<WzPropertyType>(serializer));
            if (existingValue == null || existingValue.GetType() != actualType)
            {
                var contract = serializer.ContractResolver.ResolveContract(actualType);
                existingValue = contract.DefaultCreator();
            }

            using var subReader = token.CreateReader();
            serializer.Populate(subReader, existingValue);

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}
