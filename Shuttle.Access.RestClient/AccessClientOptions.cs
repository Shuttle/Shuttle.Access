using System;

namespace Shuttle.Access.RestClient
{
    public class AccessClientOptions
    {
        public const string SectionName = "Shuttle:Access:Client";

        public Uri BaseAddress { get; set; }
        public string IdentityName { get; set; }
        public string Password { get; set; }
        public TimeSpan RenewToleranceTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
    }
}