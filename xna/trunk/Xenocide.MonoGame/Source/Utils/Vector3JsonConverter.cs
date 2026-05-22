using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;

namespace Xenocide.Utils
{
    public class Vector3DictionaryConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType) return false;
            var open = typeToConvert.GetGenericTypeDefinition();
            if (open != typeof(Dictionary<,>)) return false;
            var keyType = typeToConvert.GetGenericArguments()[0];
            return keyType == typeof(Vector3);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var valueType = typeToConvert.GetGenericArguments()[1];
            var converterType = typeof(Vector3DictionaryConverter<>).MakeGenericType(valueType);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private class Vector3DictionaryConverter<TValue> : JsonConverter<Dictionary<Vector3, TValue>>
        {
            public override Dictionary<Vector3, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null) return null;

                var result = new Dictionary<Vector3, TValue>();
                using var doc = JsonDocument.ParseValue(ref reader);
                foreach (var entry in doc.RootElement.EnumerateObject())
                {
                    var parts = entry.Name.Split(',');
                    var key = new Vector3(
                        float.Parse(parts[0], CultureInfo.InvariantCulture),
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture));

                    var raw = entry.Value.GetRawText();
                    var bytes = System.Text.Encoding.UTF8.GetBytes(raw);
                    var value = JsonSerializer.Deserialize<TValue>(bytes.AsSpan(), options);
                    result[key] = value;
                }
                return result;
            }

            public override void Write(Utf8JsonWriter writer, Dictionary<Vector3, TValue> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                foreach (var kvp in value)
                {
                    var key = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", kvp.Key.X, kvp.Key.Y, kvp.Key.Z);
                    writer.WritePropertyName(key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
            }
        }
    }
}
