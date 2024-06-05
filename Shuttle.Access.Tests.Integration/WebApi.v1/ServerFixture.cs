using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class ServerFixture : WebApiFixture
    {
        [Test]
        public async Task Should_be_able_to_get_configuration_async()
        {
            using (var httpClient = Factory.CreateClient())
            {
                var response = await GetClient(httpClient).Server.ConfigurationAsync();

                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Version, Is.EqualTo("1.0.0"));
            }
        }
    }

}