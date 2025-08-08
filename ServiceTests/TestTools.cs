using LegacyServices.Json;
using System.Text.Json;

namespace ServiceTests;
internal static class TestTools
{
    private static readonly JsonSerializerOptions opt;

    public static bool DoTimeoutTests { get; set; }

    static TestTools()
    {
        opt = new(JsonSerializerDefaults.General)
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        opt.Converters.Add(new IPEndPointConverter());
        opt.Converters.Add(new ScientificNotationConverterLong());
        opt.Converters.Add(new ScientificNotationConverterInt());
        opt.MakeReadOnly(true);
    }

    public static void IgnoreTimeoutTest()
    {
        if (!DoTimeoutTests)
        {
            Assert.Ignore("Timeout tests are ignored. Set TestTools.DoTimeoutTests to 'true' to run these tests");
        }
    }

    public static string ToJson<T>(this T? any) => JsonSerializer.Serialize(any, opt);

    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, opt);
}
