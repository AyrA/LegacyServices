namespace LegacyServices.Services.Chargen;

internal class Service : BaseResponseService<Options>
{
    private static readonly SemaphoreSlim globalDelay = new(1);
    private readonly byte[][] lines;
    private readonly byte[] fastLines;

    public Service() : base(19)
    {
        Name = "Chargen";
        repeat = true;

        //Pre-generate all possible lines
        var temp = new List<byte[]>();
        for (var i = 0; i < 95; i++)
        {
            temp.Add([.. Enumerable.Range(i, 95).Select(m => (byte)((m % 95) + 0x20)), 0x0D, 0x0A]);
        }
        lines = [.. temp];
        fastLines = [.. lines.SelectMany(m => m)];
    }

    public override void Config(Options config)
    {
        base.Config(config);
        useNodelay = !config.SpeedTest;
    }

    protected override async Task<byte[]?> GetResponse(Options options, int iteration)
    {
        if (options.SpeedTest)
        {
            iteration *= 95;
        }
        //Handle overflow in case the user limits to int.MaxValue
        if (options.LineLimit > 0 && (iteration > options.LineLimit || iteration < 0))
        {
            return null;
        }
        if (options.SpeedTest)
        {
            return fastLines;
        }
        if (options.LineDelay > 0)
        {
            if (options.GlobalDelay)
            {
                await globalDelay.WaitAsync();
                try
                {
                    await Task.Delay(options.LineDelay);
                }
                finally
                {
                    globalDelay.Release();
                }
            }
            else
            {
                await Task.Delay(options.LineDelay);
            }
        }
        return lines[(iteration - 1) % lines.Length];
    }
}
