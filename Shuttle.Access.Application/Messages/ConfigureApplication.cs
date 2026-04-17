namespace Shuttle.Access.Application;

public class ConfigureApplication
{
    public string AdministratorIdentityName { get; set; } = "shuttle-admin";
    public string AdministratorPassword { get; set; } = "shuttle-admin";
    public bool ShouldConfigure { get; set; } = true;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}