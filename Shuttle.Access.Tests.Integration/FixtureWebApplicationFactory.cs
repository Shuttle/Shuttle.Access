using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shuttle.Access.WebApi;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Tests.Integration;

public class FixtureWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IAccessService> AccessService { get; } = new();
    public Mock<IDatabaseContextFactory> DatabaseContextFactory { get; } = new();
    public Mock<ISessionService> SessionService { get; } = new();

    private readonly Action<IWebHostBuilder> _webHostBuilder;

    public FixtureWebApplicationFactory(Action<IWebHostBuilder> webHostBuilder = null)
    {
        _webHostBuilder = webHostBuilder;
    }

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

        base.ConfigureClient(client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        _webHostBuilder?.Invoke(builder);

        AccessService.Setup(m => m.HasPermission(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
        AccessService.Setup(m => m.Contains(It.IsAny<Guid>())).Returns(true);

        DatabaseContextFactory.Setup(m => m.Create()).Returns(new Mock<IDatabaseContext>().Object);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(new Mock<ISubscriptionService>().Object);
            services.AddSingleton(SessionService.Object);
            services.AddSingleton(DatabaseContextFactory.Object);
            services.AddSingleton(AccessService.Object);
            //services.AddSingleton(new Mock<IAuthenticationService>().Object);
            //services.AddSingleton(new Mock<IAuthorizationService>().Object);
            //services.AddSingleton(new Mock<ISessionRepository>().Object);
            //services.AddSingleton(new Mock<IServiceBus>().Object);
            //services.AddSingleton(new Mock<IMediator>().Object);
            //services.AddSingleton(new Mock<IHashingService>().Object);
            //services.AddSingleton(new Mock<IEventStore>().Object);
            //services.AddSingleton(new Mock<IPasswordGenerator>().Object);
        });
    }
}