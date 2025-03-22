namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationProviderOptions
{
    public const string SectionName = "Shuttle:Access:Client:PasswordAuthenticationProvider";

    public string IdentityName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}