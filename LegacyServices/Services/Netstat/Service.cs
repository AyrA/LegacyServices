namespace LegacyServices.Services.Netstat;
internal class Service : BaseResponseService<Options>
{
    public Service() : base(15)
    {
        Name = "Netstat";
    }

    protected override Task<byte[]?> GetResponse(Options options, int _)
    {
        var lines = Tools.Exec("netstat", "-an").TrimEnd().Split('\n');
        if (!options.All)
        {
            lines = [.. lines.Where(m => m.ContainsCI("LISTEN"))];
        }
        return Task.FromResult((string.Join(Tools.CRLF, lines) + Tools.CRLF).Utf())!;
    }
}
