namespace LegacyServices.Services.Users;

internal interface IUserList
{
    public UserInfo[] GetUsers() => GetUsers(Environment.MachineName);

    UserInfo[] GetUsers(string serverName);
}
