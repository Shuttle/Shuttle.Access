using System;
using System.Collections.Generic;

namespace Shuttle.Access
{
    public class AccessOptions
    {
        public const string SectionName = "Shuttle:Access";

        public string ConnectionStringName { get; set; } = "Access";
        public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan SessionRenewalTolerance { get; set; } = TimeSpan.FromHours(1);
        public List<string> OAuthProviderNames { get; set; } = new();
        public bool OAuthRegisterUnknownIdentities { get; set; } = true;
        public string OAuthRedirectUri { get; set; } = default!;
        public string OAuthProviderSvgFolder { get; set; } = "./.files";
    }
}