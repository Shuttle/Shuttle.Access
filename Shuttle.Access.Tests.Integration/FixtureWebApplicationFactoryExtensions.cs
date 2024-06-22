using Microsoft.Extensions.Options;
using Shuttle.Access.RestClient;
using System;

namespace Shuttle.Access.Tests.Integration;

public static class FixtureWebApplicationFactoryExtensions
{
    public static IAccessClient GetAccessClient(this FixtureWebApplicationFactory factory)
    {
        return new AccessClient(Options.Create(new AccessClientOptions
        {
            BaseAddress = new Uri("http://localhost/"),
            IdentityName = "identity",
            Password = "password"
        }), new HttpClientFactory(factory.CreateClient()));
    }

}