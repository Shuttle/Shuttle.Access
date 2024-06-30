using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.WebApi;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Integration;

public class FixtureWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IWebHostBuilder> _webHostBuilder;

    public FixtureWebApplicationFactory(Action<IWebHostBuilder> webHostBuilder = null)
    {
        _webHostBuilder = webHostBuilder;
    }

    public Mock<IAccessService> AccessService { get; } = new();
    public Mock<IDatabaseContextFactory> DatabaseContextFactory { get; } = new();
    public Mock<IPermissionQuery> PermissionQuery { get; } = new();
    public Mock<IRoleQuery> RoleQuery { get; } = new();
    public Mock<ISessionQuery> SessionQuery { get; } = new();
    public Mock<ISessionService> SessionService { get; } = new();
    public Mock<ISessionRepository> SessionRepository { get; } = new();
    public Mock<IServiceBus> ServiceBus { get; } = new();

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
            services.AddSingleton(AccessService.Object);
            services.AddSingleton(DatabaseContextFactory.Object);
            services.AddSingleton(PermissionQuery.Object);
            services.AddSingleton(RoleQuery.Object);
            services.AddSingleton(SessionQuery.Object);
            services.AddSingleton(ServiceBus.Object);
            services.AddSingleton(SessionRepository.Object);
            services.AddSingleton(SessionService.Object);
        });
    }
}