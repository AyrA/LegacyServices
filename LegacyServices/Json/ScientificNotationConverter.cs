using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegacyServices.Json;

internal class ScientificNotationConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt64(out var result) ? result : Convert.ToInt64(reader.GetDecimal());
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}