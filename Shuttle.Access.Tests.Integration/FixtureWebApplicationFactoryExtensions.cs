using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Shuttle.Access.RestClient;

namespace Shuttle.Access.Tests.Integration;

public static class FixtureWebApplicationFactoryExtensions
{
    public static IAccessClient GetAccessClient(this FixtureWebApplicationFactory factory, Action<HttpClient>? configureHttpClient = null)
    {
        var httpClient = factory.CreateClient();

        configureHttpClient?.Invoke(httpClient);

        return new AccessClient(Options.Create(new AccessClientOptions
        {
            BaseAddress = new("http://localhost/"),
            IdentityName = "identity",
            Password = "password"
        }), httpClient);
    }
}