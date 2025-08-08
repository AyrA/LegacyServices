namespace LegacyServices.Services.Chargen;

internal class Service : BaseResponseService<Options>
{
    public Service() : base(19)
    {
        Name = "Chargen";
        repeat = true;
    }

    protected override async Task<byte[]?> GetResponse(Options options, int iteration)
    {
        //Handle overflow in case the user limits to int.MaxValue
        if (options.LineLimit > 0 && (iteration > options.LineLimit || iteration < 0))
        {
            return null;
        }
        if (options.LineDelay > 0)
        {
            await Task.Delay(options.LineDelay);
        }
        return [.. Enumerable.Range(iteration - 1, 95).Select(m => (byte)((m % 95) + 0x20)), 0x0D, 0x0A];
    }
}
