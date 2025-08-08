using System.Text;

namespace LegacyServices;

internal static class Extensions
{
    public static bool EqualsCI(this string? a, string? b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);

    public static bool ContainsCI(this string? a, string? b) => a != null && b != null && a.Contains(b, StringComparison.InvariantCultureIgnoreCase);

    public static byte[] Utf(this string s) => Encoding.UTF8.GetBytes(s);

    public static string Utf(this IEnumerable<byte> b) => Encoding.UTF8.GetString([.. b]);

    public static byte[] Latin1(this string s) => Encoding.Latin1.GetBytes(s);

    public static string Latin1(this IEnumerable<byte> b) => Encoding.Latin1.GetString([.. b]);
}
