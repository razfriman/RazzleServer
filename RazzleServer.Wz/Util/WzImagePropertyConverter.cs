using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RazzleServer.Wz.Util
{
    public class WzImagePropertyConverter : JsonConverter<string>
    {
        public override bool HandleNull => true;

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(WzImageProperty);

        public override string Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // reader.JToken.Load(reader);
            // var typeToken = token["Type"];
            // if (typeToken == null)
            // {
            //     throw new InvalidOperationException("invalid object");
            // }
            //
            // var actualType = WzImagePropertyMapper.GetType(typeToken.ToObject<WzPropertyType>(serializer));
            // if (existingValue == null || existingValue.GetType() != actualType)
            // {
            //     var contract = serializer.ContractResolver.ResolveContract(actualType);
            //     existingValue = contract.DefaultCreator();
            // }
            //
            // using var subReader = token.CreateReader();
            // serializer.Populate(subReader, existingValue);
            //
            // return existingValue;
            return reader.GetString() ?? "No description provided.";
        }

        public override void Write(
            Utf8JsonWriter writer,
            string value,
            JsonSerializerOptions options) =>
        throw new NotImplementedException();
        // public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        //     JsonSerializer serializer)
        // {
        //     var token = JToken.Load(reader);
        //     var typeToken = token["Type"];
        //     if (typeToken == null)
        //     {
        //         throw new InvalidOperationException("invalid object");
        //     }
        //
        //     var actualType = WzImagePropertyMapper.GetType(typeToken.ToObject<WzPropertyType>(serializer));
        //     if (existingValue == null || existingValue.GetType() != actualType)
        //     {
        //         var contract = serializer.ContractResolver.ResolveContract(actualType);
        //         existingValue = contract.DefaultCreator();
        //     }
        //
        //     using var subReader = token.CreateReader();
        //     serializer.Populate(subReader, existingValue);
        //
        //     return existingValue;
        // }
    }
}
