namespace LegacyServices.Services.Daytime;
internal class Service : BaseResponseService<Options>
{
    public Service() : base(13)
    {
        Name = "Daytime";
    }

    protected override Task<byte[]?> GetResponse(Options options, int _)
    {
        return Task.FromResult((options.GetDate() + Tools.CRLF).Utf())!;
    }
}
