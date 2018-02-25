using System;
using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Access
{
    public class AccessSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = false, DefaultValue = "Access")]
        public string ConnectionStringName => (string)this["connectionStringName"];

        public static AccessConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<AccessSection>("shuttle", "access");
            var configuration = new AccessConfiguration();

            var connectionStringName = "Access";

            if (section != null)
            {
                connectionStringName = section.ConnectionStringName;
            }

            configuration.ProviderName = GetSettings(connectionStringName).ProviderName;
            configuration.ConnectionString = GetSettings(connectionStringName).ConnectionString;

            return configuration;
        }

        private static ConnectionStringSettings GetSettings(string connectionStringName)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (settings == null)
            {
                throw new InvalidOperationException(string.Format(Resources.ConnectionStringMissing, connectionStringName));
            }

            return settings;
        }
    }
}