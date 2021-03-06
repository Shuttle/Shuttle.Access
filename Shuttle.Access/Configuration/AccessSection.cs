﻿using System;
using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Access
{
    public class AccessSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = false, DefaultValue = "Access")]
        public string ConnectionStringName => (string) this["connectionStringName"];

        [ConfigurationProperty("sessionDuration", IsRequired = false, DefaultValue = "00:01:00")]
        public TimeSpan SessionDuration => TimeSpan.Parse((string) this["sessionDuration"]);

        public static IAccessConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<AccessSection>("shuttle", "access");
            var configuration = new AccessConfiguration();

            var connectionStringName = "Access";

            if (section != null)
            {
                connectionStringName = section.ConnectionStringName;
                configuration.SessionDuration = section.SessionDuration;
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
                throw new InvalidOperationException(string.Format(Resources.ConnectionStringMissing,
                    connectionStringName));
            }

            return settings;
        }
    }
}