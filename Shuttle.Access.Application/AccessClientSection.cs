using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Access.Application
{
    public class AccessClientSection : ConfigurationSection
    {
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url => (string)this["url"];

        [ConfigurationProperty("identityName", IsRequired = true)]
        public string IdentityName => (string)this["identityName"];

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password => (string)this["password"];

        public static IClientConfiguration GetConfiguration()
        {
            var section =
                ConfigurationSectionProvider.Open<AccessClientSection>("shuttle", "accessClient");

            if (section == null)
            {
                throw new ConfigurationErrorsException(Resources.ClientSectionException);
            }

            return new ClientConfiguration(section.Url, section.IdentityName, section.Password);
        }
    }
}