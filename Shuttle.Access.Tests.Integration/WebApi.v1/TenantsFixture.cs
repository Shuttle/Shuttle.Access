using System.Net;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class TenantsFixture
{
    [Test]
    public async Task Should_be_able_to_register_tenant_in_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(1));

        var response = await factory.GetAccessClient().Tenants.RegisterAsync(new()
        {
            Name = "tenant",
            AdministratorIdentityName = "identity-name"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<RegisterTenant>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_not_be_able_to_register_tenant_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Tenants.RegisterAsync(new()
        {
            Name = "tenant",
            AdministratorIdentityName = "identity-name"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_delete_tenant_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Tenants.DeleteAsync(Guid.NewGuid());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_set_tenant_status_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Tenants.SetStatusAsync(Guid.NewGuid(), new()
        {
            Status = 1
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }
}
