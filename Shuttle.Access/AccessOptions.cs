namespace Shuttle.Access;

public class AccessOptions
{
    public const string SectionName = "Shuttle:Access";
    public bool AllowPasswordAuthentication { get; set; } = true;
    public ConfigurationOptions Configuration { get; set; } = new();

    public string ConnectionStringName { get; set; } = "Access";
    public string ExtensionFolder { get; set; } = "./.extension";
    public List<KnownApplicationOptions> KnownApplications { get; set; } = [];
    public bool OAuthRegisterUnknownIdentities { get; set; } = true;
    public string Realm { get; set; } = "API";
    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan SessionRenewalTolerance { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan SessionTokenExchangeValidityTimeSpan { get; set; } = TimeSpan.FromMinutes(1);
}

public class ConfigurationOptions
{
    public string AdministratorIdentityName { get; set; } = "shuttle-admin";
    public string AdministratorPassword { get; set; } = "shuttle-admin";
    public bool ShouldConfigure { get; set; } = true;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);
}

public class KnownApplicationOptions
{
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SessionTokenExchangeUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}