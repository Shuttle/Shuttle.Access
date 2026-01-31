namespace Shuttle.Access.WebApi;

public class ApiOptions
{
    public const string SectionName = "Shuttle:Access:Api";
    public bool AllowPasswordAuthentication { get; set; } = true;
    public string ExtensionFolder { get; set; } = "./.extension";
    public bool OAuthRegisterUnknownIdentities { get; set; } = true;
}