using System.Net;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class PermissionsFixture
{
    [Test]
    public async Task Should_be_able_to_post_permission_async()
    {
        const string permission = "integration://available-permission";

        var factory = new FixtureWebApplicationFactory();

        factory.Bus.Setup(m => m.SendAsync(It.Is<RegisterPermission>(message => message.Name.Equals(permission)), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.PostAsync(new()
        {
            Name = permission
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_post_permission_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Permissions.PostAsync(new()
        {
            Name = "integration://available-permission"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_set_permission_name_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Bus.Setup(m => m.SendAsync(It.Is<Messages.v1.SetPermissionName>(message => message.Id.Equals(id) && message.Name.Equals("new-name")), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.SetNameAsync(id, new()
        {
            Name = "new-name"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_set_permission_name_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Permissions.SetNameAsync(Guid.NewGuid(), new()
        {
            Name = "new-name"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_set_permission_description_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Bus.Setup(m => m.SendAsync(It.Is<Messages.v1.SetPermissionDescription>(message => message.Id.Equals(id) && message.Description.Equals("new-description")), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.SetDescriptionAsync(id, new()
        {
            Description = "new-description"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_set_permission_description_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Permissions.SetDescriptionAsync(Guid.NewGuid(), new()
        {
            Description = "new-description"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_search_permissions_async()
    {
        var permission = new Query.Permission
        {
            Id = Guid.NewGuid(),
            Name = "integration://available-permission",
            Status = PermissionStatus.Active
        };

        var factory = new FixtureWebApplicationFactory();

        factory.PermissionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Permission.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new List<Query.Permission> { permission }.AsEnumerable()));

        var response = await factory.GetAccessClient().Permissions.SearchAsync(new());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content!.Find(item => item.Id == permission.Id), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_set_permission_status_async()
    {
        var permissionId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Bus.Setup(m => m.SendAsync(It.Is<SetPermissionStatus>(message => message.Id.Equals(permissionId)), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.SetStatusAsync(permissionId, new()
        {
            Status = (int)PermissionStatus.Deactivated
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_set_permission_status_outside_the_system_tenant_async()
    {
        var factory = new FixtureWebApplicationFactory(tenantId: Guid.NewGuid());

        var response = await factory.GetAccessClient().Permissions.SetStatusAsync(Guid.NewGuid(), new()
        {
            Status = (int)PermissionStatus.Deactivated
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }
}