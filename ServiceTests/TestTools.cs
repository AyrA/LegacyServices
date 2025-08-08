using LegacyServices;
using System.Text.Json;

namespace ServiceTests;
internal static class TestTools
{
    private static readonly JsonSerializerOptions opt;

    static TestTools()
    {
        opt = new(JsonSerializerDefaults.General)
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        opt.Converters.Add(new IPEndPointConverter());
        opt.Converters.Add(new ScientificNotationConverter());
        opt.MakeReadOnly(true);
    }

    public static string ToJson<T>(this T? any) => JsonSerializer.Serialize(any, opt);

    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, opt);
}
