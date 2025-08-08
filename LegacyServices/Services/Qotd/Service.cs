namespace LegacyServices.Services.Qotd;

internal class Service : BaseResponseService<Options>
{
    public Service() : base(17)
    {
        Name = "QOTD";
    }

    protected override byte[]? GetResponse(Options options)
    {
        return (options.GetQuote() + Tools.CRLF).Utf();
    }
}
