using System.Configuration;
using Shuttle.Core.Configuration;

namespace Shuttle.Access.Api
{
    public class ClientSection : ConfigurationSection
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
                ConfigurationSectionProvider.Open<ClientSection>("shuttle", "accessClient");

            return new ClientConfiguration(section.Url, section.IdentityName, section.Password);
        }
    }
}