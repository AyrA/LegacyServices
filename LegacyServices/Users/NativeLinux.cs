namespace LegacyServices.Users;

internal class NativeLinux() : UserList
{
    public override UserInfo[] GetUsers(string serverName)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException();
        }
        if (serverName != null && serverName != Environment.MachineName)
        {
            throw new ArgumentException("Querying remote hosts on Linux is not supported");
        }
        var users = Tools.Exec("who")
            .Trim()
            .Split('\n')
            .Select(m => m.Trim().Split(' ')[0])
            .ToArray();
        return [.. users.Select(m => new UserInfo(m, null))];
    }
}