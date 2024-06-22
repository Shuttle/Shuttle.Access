using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class ServerFixture
{
    [Test]
    public async Task Should_be_able_to_get_configuration_async()
    {
        var client = new FixtureWebApplicationFactory().GetAccessClient();

        var response = await client.Server.ConfigurationAsync();

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content.Version, Is.EqualTo("1.0.0"));
    }
}