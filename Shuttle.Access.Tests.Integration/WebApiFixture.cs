using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Shuttle.Access.RestClient;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Tests.Integration
{
    [TestFixture]
    public class WebApiFixture 
    {
        public WebApplicationFactory<Startup> Factory { get; private set; }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            Factory = new FixtureWebApplicationFactory<Startup>()
                .WithWebHostBuilder(
                    _ => { });
        }

        public IAccessClient GetClient(HttpClient httpClient)
        {
            Guard.AgainstNull(httpClient, nameof(httpClient));

            return new AccessClient(new AccessClientConfiguration("http://localhost/", "identity", "password"), new HttpClientFactory(httpClient));
        }
    }

    internal class HttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}