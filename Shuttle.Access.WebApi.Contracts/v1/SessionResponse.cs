namespace Shuttle.Access.WebApi.Contracts.v1;

public class SessionResponse
{
    public Session Session { get; set; } = new();
    public bool RegistrationRequested { get; set; }
    public string Result { get; set; } = string.Empty;
    public Guid? Token { get; set; }
    public List<Tenant> Tenants { get; set; } = [];
}