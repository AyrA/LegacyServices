using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegacyServices.Json;

internal class ScientificNotationConverterLong : JsonConverter<long>
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

internal class ScientificNotationConverterInt : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt32(out var result) ? result : Convert.ToInt32(reader.GetDecimal());
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
