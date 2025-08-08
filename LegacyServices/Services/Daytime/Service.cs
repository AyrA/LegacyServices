namespace LegacyServices.Services.Daytime;
internal class Service : BaseResponseService<Options>
{
    public Service() : base(13)
    {
        Name = "Daytime";
    }

    protected override byte[]? GetResponse(Options options)
    {
        return (options.GetDate() + Tools.CRLF).Utf();
    }
}
