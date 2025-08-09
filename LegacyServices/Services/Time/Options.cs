using System.Net;

namespace LegacyServices.Services.Time;

internal class Options : IEnable
{
    private static readonly DateTime Epoch = new(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public bool Enabled { get; set; }

    public bool UseInt64 { get; set; }

    public byte[] GetDate()
    {
        var secs = (ulong)Math.Floor(DateTime.UtcNow.Subtract(Epoch).TotalSeconds);
        var ret = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)secs));
        if (UseInt64)
        {
            return ret;
        }
        return ret[4..];
    }
}
