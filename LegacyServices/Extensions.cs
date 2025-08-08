using System.Text;

namespace LegacyServices;

internal static class Extensions
{
    public static bool EqualsCI(this string? a, string? b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);

    public static bool ContainsCI(this string? a, string? b) => a != null && b != null && a.Contains(b, StringComparison.InvariantCultureIgnoreCase);

    public static byte[] Utf(this string s) => Encoding.UTF8.GetBytes(s);

    public static string Utf(this IEnumerable<byte> b) => Encoding.UTF8.GetString([.. b]);
}
