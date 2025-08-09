namespace LegacyServices.Services.Time;
internal class Service : BaseResponseService<Options>
{
    public Service() : base(37)
    {
        Name = "Time";
    }

    protected override Task<byte[]?> GetResponse(Options options, int _)
    {
        return Task.FromResult(options.GetDate())!;
    }
}
