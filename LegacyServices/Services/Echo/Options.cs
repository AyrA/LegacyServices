namespace LegacyServices.Services.Echo;

internal class Options : IEnable
{
    public bool Enabled { get; set; }
    public int Timeout { get; set; }
    public long MaxData { get; set; }
}
