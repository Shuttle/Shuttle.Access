using System;

namespace Shuttle.Access
{
    public class AccessConfiguration : IAccessConfiguration
    {
        public AccessConfiguration()
        {
            SessionDuration = TimeSpan.FromHours(1);
        }

        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        public TimeSpan SessionDuration { get; set; }
    }
}