using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class ServerFixture
{
    [Test]
    public async Task Should_be_able_to_get_configuration_async()
    {
        var client = new FixtureWebApplicationFactory().CreateClient();

        var response = await client.GetFromJsonAsync<ServerConfiguration>("/v1/server/configuration");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Version, Is.EqualTo("1.0.0"));
    }
}