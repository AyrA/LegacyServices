namespace LegacyServices.Services.Netstat;
internal class Service : BaseResponseService<Options>
{
    public Service() : base(15)
    {
        Name = "Netstat";
    }

    protected override byte[]? GetResponse(Options options)
    {
        var lines = Tools.Exec("netstat", "-an").TrimEnd().Split('\n');
        if (!options.All)
        {
            lines = [.. lines.Where(m => m.ContainsCI("LISTEN"))];
        }
        return (string.Join(Tools.CRLF, lines) + Tools.CRLF).Utf();
    }
}
