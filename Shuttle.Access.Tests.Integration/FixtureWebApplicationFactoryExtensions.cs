using Microsoft.Extensions.Options;
using Shuttle.Access.RestClient;
using System;
using System.Net.Http;

namespace Shuttle.Access.Tests.Integration;

public static class FixtureWebApplicationFactoryExtensions
{
    public static IAccessClient GetAccessClient(this FixtureWebApplicationFactory factory, Action<HttpClient> configureHttpClient = null)
    {
        var httpClient = factory.CreateClient();

        if (configureHttpClient != null)
        {
            configureHttpClient(httpClient);
        }

        return new AccessClient(Options.Create(new AccessClientOptions
        {
            BaseAddress = new Uri("http://localhost/"),
            IdentityName = "identity",
            Password = "password"
        }), httpClient);
    }

}