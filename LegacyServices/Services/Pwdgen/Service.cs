using System.Security.Cryptography;

namespace LegacyServices.Services.Pwdgen;
internal class Service : BaseResponseService<Options>
{
    private static readonly char[][] buckets = [
        "0123456789".ToCharArray(),
        "abcdefghijklmnopqrstuvwxyz".ToCharArray(),
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(),
        " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".ToCharArray()
    ];

    public Service() : base(129)
    {
        Name = "Pwdgen";
        repeat = true;
    }

    protected override Task<byte[]?> GetResponse(Options options, int iteration)
    {
        var count = options.Count;
        if (count == 0)
        {
            count = 6;
        }
        if (iteration > count)
        {
            return Task.FromResult((byte[]?)null);
        }

        var length = options.Length;
        if (length == 0)
        {
            length = 8;
        }
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            var bucket = buckets[RandomNumberGenerator.GetInt32(buckets.Length)];
            chars[i] = bucket[RandomNumberGenerator.GetInt32(bucket.Length)];
        }
        var password = new string(chars) + Tools.CRLF;
        return Task.FromResult(password.Latin1())!;
    }
}
