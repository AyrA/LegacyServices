namespace LegacyServices.Services.Qotd;

internal class Service : BaseResponseService<Options>
{
    public Service() : base(17)
    {
        Name = "QOTD";
    }

    protected override Task<byte[]?> GetResponse(Options options, int _)
    {
        return Task.FromResult((options.GetQuote() + Tools.CRLF).Utf())!;
    }
}
