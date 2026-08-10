using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.WebApi;
using Shuttle.Mediator;
using Shuttle.Hopper;
using Shuttle.OAuth;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Integration;

public class FixtureWebApplicationFactory(Action<IWebHostBuilder>? webHostBuilder = null) : WebApplicationFactory<Program>
{
    public Mock<IIdentityQuery> IdentityQuery { get; } = new();
    public Mock<IMediator> Mediator { get; } = new();
    public Mock<IOAuthGrantRepository> OAuthGrantRepository { get; } = new();
    public Mock<IPermissionQuery> PermissionQuery { get; } = new();
    public Mock<IRoleQuery> RoleQuery { get; } = new();
    public Mock<IBus> Bus { get; } = new();
    public Mock<ISessionQuery> SessionQuery { get; } = new();

    protected override void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new("Shuttle.Access", $"token={Guid.NewGuid():D}");

        base.ConfigureClient(client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Empty);

        webHostBuilder?.Invoke(builder);

        var accessOptions = new AccessOptions();

        var session = new Query.Session
        {
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity-name",
            Permissions = [new() { Id = Guid.NewGuid(), Name = "*", TenantId = accessOptions.SystemTenantId }],
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1)),
        };

        builder.ConfigureServices(services =>
        {
            services.AddOptions<SqlServerStorageOptions>().Configure(options =>
            {
                options.ConfigureDatabase = false;
            });

            services.AddSingleton(new Mock<ISubscriptionService>().Object);
            services.AddSingleton(OAuthGrantRepository.Object);
            services.AddSingleton(IdentityQuery.Object);
            services.AddSingleton(Mediator.Object);
            services.AddSingleton(PermissionQuery.Object);
            services.AddSingleton(RoleQuery.Object);
            services.AddSingleton(SessionQuery.Object);
            services.AddSingleton(Bus.Object);

            // Credential validation is covered by `AccessSessionResolverFixture`.  These fixtures exercise the
            // endpoints, so the resolver is stubbed — but it still honours the presence of an `Authorization` header
            // so that removing it genuinely yields no session.
            services.AddScoped<ISessionResolver>(_ => new FixtureSessionResolver(session, accessOptions.SystemTenantId));
        });
    }

    private class FixtureSessionResolver(Query.Session session, Guid tenantId) : ISessionResolver
    {
        public Task<SessionResolutionResult> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(string.IsNullOrWhiteSpace(httpContext.Request.Headers.Authorization.FirstOrDefault())
                ? SessionResolutionResult.None
                : SessionResolutionResult.Authenticated(session, tenantId, Guid.NewGuid()));
        }
    }
}
