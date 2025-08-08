namespace LegacyServices.Services.Users;

internal class Service : BaseResponseService<Options>
{
    public Service() : base(11)
    {
        Name = "Users";
    }

    protected override Task<byte[]?> GetResponse(Options options, int _)
    {
        return Task.FromResult((string.Join(Tools.CRLF, GetUsers()) + Tools.CRLF).Utf())!;
    }

    private string[] GetUsers()
    {
        if (opt?.CurrentOnly ?? true)
        {
            return [Environment.UserName];
        }
        try
        {
            if (OperatingSystem.IsLinux())
            {
                return [.. new NativeLinux().GetUsers().Select(m => m.UPN)];
            }
            if (OperatingSystem.IsWindows())
            {
                return [.. new NativeWindows().GetUsers().Select(m => m.UPN)];
            }
        }
        catch
        {
            return [];
        }
        //Return current user on unsupported systems
        return [Environment.UserName];
    }
}
