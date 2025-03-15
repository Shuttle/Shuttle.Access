using System;
using System.Collections.Generic;

namespace Shuttle.Access;

public class AccessOptions
{
    public const string SectionName = "Shuttle:Access";

    public string ConnectionStringName { get; set; } = "Access";
    public string ExtensionFolder { get; set; } = "./.extension";
    public bool OAuthRegisterUnknownIdentities { get; set; } = true;
    public string Realm { get; set; } = "API";
    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan SessionRenewalTolerance { get; set; } = TimeSpan.FromHours(1);
    public List<KnownApplicationOptions> KnownApplications { get; set; } = new();
    public TimeSpan SessionTokenExchangeValidityTimeSpan { get; set; } = TimeSpan.FromMinutes(1);
}

public class KnownApplicationOptions
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SessionTokenExchangeUrl { get; set; } = string.Empty;
}