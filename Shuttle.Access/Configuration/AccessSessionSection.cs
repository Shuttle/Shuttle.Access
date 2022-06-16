using System;
using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Access
{
    public class AccessSessionSection : ConfigurationSection
    {
        [ConfigurationProperty("sessionDuration", IsRequired = false, DefaultValue = "00:01:00")]
        public TimeSpan SessionDuration => TimeSpan.Parse((string) this["sessionDuration"]);

        public static IAccessSessionConfiguration GetConfiguration()
        {
            var section = ConfigurationSectionProvider.Open<AccessSessionSection>("shuttle", "accessSession");
            var configuration = new AccessSessionConfiguration();

            if (section != null)
            {
                configuration.SessionDuration = section.SessionDuration;
            }

            return configuration;
        }
    }
}