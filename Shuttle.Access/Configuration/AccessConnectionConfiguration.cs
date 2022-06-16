using System;

namespace Shuttle.Access
{
    public class AccessConnectionConfiguration : IAccessConnectionConfiguration
    {
        public AccessConnectionConfiguration()
        {
            SessionDuration = TimeSpan.FromHours(1);
        }

        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        public TimeSpan SessionDuration { get; set; }
    }
}