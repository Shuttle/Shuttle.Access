﻿using System;
using System.Collections.Generic;

namespace Shuttle.Access;

public class AccessOptions
{
    public const string SectionName = "Shuttle:Access";

    public string ConnectionStringName { get; set; } = "Access";
    public List<string> OAuthProviderNames { get; set; } = new();
    public string OAuthProviderSvgFolder { get; set; } = "./.files";
    public string OAuthRedirectUri { get; set; } = default!;
    public bool OAuthRegisterUnknownIdentities { get; set; } = true;
    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan SessionRenewalTolerance { get; set; } = TimeSpan.FromHours(1);
}