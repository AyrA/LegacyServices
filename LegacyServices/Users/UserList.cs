namespace LegacyServices.Users;

internal abstract class UserList : IUserList
{
    public UserInfo[] GetUsers() => GetUsers(Environment.MachineName);

    public abstract UserInfo[] GetUsers(string serverName);

}
