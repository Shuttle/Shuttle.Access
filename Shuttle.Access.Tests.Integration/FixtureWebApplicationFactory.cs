using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shuttle.Access.DataAccess;

namespace Shuttle.Access.Tests.Integration
{
    public class FixtureWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var accessService = new Mock<IAccessService>();

            accessService.Setup(m => m.HasPermission(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
            accessService.Setup(m => m.Contains(It.IsAny<Guid>())).Returns(true);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(hostBuilder =>
                {
                    hostBuilder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<ISessionService>(new SessionService());
                        services.AddSingleton(new Mock<ISessionRepository>().Object);
                        services.AddSingleton(new Mock<ISessionQuery>().Object);
                        services.AddSingleton(accessService.Object);
                    });
                    hostBuilder.UseStartup<TStartup>().UseTestServer();
                });
            return builder;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("access-sessiontoken", Guid.NewGuid().ToString());

            base.ConfigureClient(client);
        }
    }
}