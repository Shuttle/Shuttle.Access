namespace Shuttle.Access.WebApi.Contracts.v1;

public class SessionRequest
{
    public string Application { get; set; } = string.Empty;
    public string IdentityName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid Token { get; set; }
}