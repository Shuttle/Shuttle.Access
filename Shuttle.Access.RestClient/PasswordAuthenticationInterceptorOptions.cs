namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationInterceptorOptions
{
    public const string SectionName = "Shuttle:Access:Client:PasswordAuthenticationInterceptor";

    public string IdentityName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}