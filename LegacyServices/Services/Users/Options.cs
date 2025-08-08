namespace LegacyServices.Services.Users;

internal class Options : IEnable
{
    public bool Enabled { get; set; }
    public bool CurrentOnly { get; set; }
}
