using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.WebApi;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.OAuth;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Tests.Integration;

public class FixtureWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IWebHostBuilder>? _webHostBuilder;

    public FixtureWebApplicationFactory(Action<IWebHostBuilder>? webHostBuilder = null)
    {
        _webHostBuilder = webHostBuilder;
    }

    public Mock<ISessionCache> SessionCache { get; } = new();
    public Mock<IDatabaseContextFactory> DatabaseContextFactory { get; } = new();
    public Mock<IIdentityQuery> IdentityQuery { get; } = new();
    public Mock<IMediator> Mediator { get; } = new();
    public Mock<IOAuthGrantRepository> OAuthGrantRepository { get; } = new();
    public Mock<IPermissionQuery> PermissionQuery { get; } = new();
    public Mock<IRoleQuery> RoleQuery { get; } = new();
    public Mock<IServiceBus> ServiceBus { get; } = new();
    public Mock<ISessionQuery> SessionQuery { get; } = new();
    public Mock<ISessionRepository> SessionRepository { get; } = new();

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new("Shuttle.Access", $"token={Guid.NewGuid():D}");

        base.ConfigureClient(client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        _webHostBuilder?.Invoke(builder);

        SessionCache.Setup(m => m.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), default)).Returns(ValueTask.FromResult(true));
        SessionCache.Setup(m => m.FindAsync(It.IsAny<Guid>(), default)).Returns(Task.FromResult(new Messages.v1.Session
        {
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity-name",
            Permissions = ["*"],
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1))
        })!);

        var databaseContext = new Mock<IDatabaseContext>();

        DatabaseContextFactory.Setup(m => m.Create()).Returns(databaseContext.Object);
        DatabaseContextFactory.Setup(m => m.Create(It.IsAny<string>())).Returns(databaseContext.Object);

        builder.ConfigureServices(services =>
        {
            services.AddOptions<SqlStorageOptions>().Configure(options =>
            {
                options.ConfigureDatabase = false;
            });

            services.AddOptions<SqlEventProcessingOptions>().Configure(options =>
            {
                options.ConfigureDatabase = false;
            });

            services.AddSingleton(new Mock<ISubscriptionService>().Object);
            services.AddSingleton(SessionCache.Object);
            services.AddSingleton(OAuthGrantRepository.Object);
            services.AddSingleton(DatabaseContextFactory.Object);
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