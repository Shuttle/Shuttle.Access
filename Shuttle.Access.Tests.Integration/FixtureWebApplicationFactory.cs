using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Shuttle.Access.SqlServer;
using Shuttle.Access.WebApi;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.OAuth;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Integration;

public class FixtureWebApplicationFactory(Action<IWebHostBuilder>? webHostBuilder = null) : WebApplicationFactory<Program>
{
    public Mock<IIdentityQuery> IdentityQuery { get; } = new();
    public Mock<IMediator> Mediator { get; } = new();
    public Mock<IOAuthGrantRepository> OAuthGrantRepository { get; } = new();
    public Mock<IPermissionQuery> PermissionQuery { get; } = new();
    public Mock<IRoleQuery> RoleQuery { get; } = new();
    public Mock<IServiceBus> ServiceBus { get; } = new();
    public Mock<ISessionQuery> SessionQuery { get; } = new();
    public Mock<ISessionRepository> SessionRepository { get; } = new();
    public Mock<ISessionService> SessionService { get; } = new();

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new("Shuttle.Access", $"token={Guid.NewGuid():D}");

        base.ConfigureClient(client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        webHostBuilder?.Invoke(builder);

        SessionService.Setup(m => m.HasPermissionAsync(TODO, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(true));
        SessionService.Setup(m => m.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Messages.v1.Session
        {
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity-name",
            Permissions = ["*"],
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1))
        })!);

        builder.ConfigureServices(services =>
        {
            services.AddOptions<SqlServerStorageOptions>().Configure(options =>
            {
                options.ConfigureDatabase = false;
            });

            services.AddOptions<SqlServerEventProcessingOptions>().Configure(options =>
            {
                options.ConfigureDatabase = false;
            });

            services.AddSingleton(new Mock<ISubscriptionService>().Object);
            services.AddSingleton(SessionService.Object);
            services.AddSingleton(OAuthGrantRepository.Object);
            services.AddSingleton(IdentityQuery.Object);
            services.AddSingleton(Mediator.Object);
            services.AddSingleton(PermissionQuery.Object);
            services.AddSingleton(RoleQuery.Object);
            services.AddSingleton(SessionQuery.Object);
            services.AddSingleton(ServiceBus.Object);
            services.AddSingleton(SessionRepository.Object);
        });
    }
}