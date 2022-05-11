using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;

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
                        services.AddSingleton(new Mock<IDatabaseContextFactory>().Object);
                        services.AddSingleton(new Mock<IAuthenticationService>().Object);
                        services.AddSingleton(new Mock<IAuthorizationService>().Object);
                        services.AddSingleton(new Mock<ISessionRepository>().Object);
                        services.AddSingleton(new Mock<ISessionQuery>().Object);
                        services.AddSingleton(new Mock<IServiceBus>().Object);
                        services.AddSingleton(new Mock<IMediator>().Object);
                        services.AddSingleton<IAccessService>(accessService.Object);
                        // check if still required after refactor:
                        services.AddSingleton(new Mock<IHashingService>().Object);
                        services.AddSingleton(new Mock<IEventStore>().Object);
                        services.AddSingleton(new Mock<IPasswordGenerator>().Object);
                    });
                    hostBuilder.UseStartup<TStartup>().UseTestServer();
                });
            return builder;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("access-session-token", Guid.NewGuid().ToString());

            base.ConfigureClient(client);
        }
    }
}