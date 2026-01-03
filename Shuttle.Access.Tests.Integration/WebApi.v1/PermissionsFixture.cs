using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<RegisterPermission>(message => message.Name.Equals(permission)), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.PostAsync(new()
        {
            Name = permission
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.ServiceBus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_search_permissions_async()
    {
        var permission = new Data.Models.Permission
        {
            Id = Guid.NewGuid(),
            Name = "integration://available-permission",
            Status = (int)PermissionStatus.Active
        };

        var factory = new FixtureWebApplicationFactory();

        factory.PermissionQuery.Setup(m => m.SearchAsync(It.IsAny<Data.Models.Permission.Specification>(), default)).Returns(Task.FromResult(new List<Data.Models.Permission> { permission }.AsEnumerable()));

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

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<SetPermissionStatus>(message => message.Id.Equals(permissionId)), null)).Verifiable();

        var response = await factory.GetAccessClient().Permissions.SetStatusAsync(permissionId, new()
        {
            Id = permissionId,
            Status = (int)PermissionStatus.Deactivated
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.ServiceBus.VerifyAll();
    }
}