namespace LegacyServices.Services.Users;

internal record UserInfo(string Username, string? Domain)
{
    public string UPN => string.IsNullOrEmpty(Domain) ? Username : $"{Username}@{Domain}";
}