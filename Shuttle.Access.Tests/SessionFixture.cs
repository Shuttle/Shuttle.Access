using NUnit.Framework;
using Shuttle.Access.WebApi.Contracts.v1;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionFixture
{
    [Test]
    public void Should_be_able_to_determine_whether_a_domain_session_has_a_permission()
    {
        var session = new Session(Guid.NewGuid(), [], Guid.NewGuid(), "identity", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(30));

        session.AddPermission(new(Guid.NewGuid(), "system-a://component-a/function-a", string.Empty, PermissionStatus.Active));

        Assert.That(session.HasPermission("system-a://component-a/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component-a/function-b"), Is.False);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission(new(Guid.NewGuid(), "system-a://component/*", string.Empty, PermissionStatus.Active));

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission(new(Guid.NewGuid(), "system-a://*", string.Empty, PermissionStatus.Active));

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission(new(Guid.NewGuid(), "*", string.Empty, PermissionStatus.Active));

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.True);
    }

    [Test]
    public void Should_be_able_to_determine_whether_a_message_session_has_a_permission()
    {
        var session = new WebApi.Contracts.v1.Session
        {
            Permissions = [new() { Name = "system-a://component-a/function-a" }],
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity",
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        Assert.That(session.HasPermission("system-a://component-a/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component-a/function-b"), Is.False);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add(new() { Name = "system-a://component/*" });

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add(new() { Name = "system-a://*" });

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add(new() { Name = "*" });

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.True);
    }
}