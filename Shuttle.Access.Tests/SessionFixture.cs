using System;
using NUnit.Framework;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionFixture
{
    [Test]
    public void Should_be_able_to_determine_whether_a_domain_session_has_a_permission()
    {
        var session = new Session([], Guid.NewGuid(), "identity", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(30));

        session.AddPermission("system-a://component-a/function-a");

        Assert.That(session.HasPermission("system-a://component-a/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component-a/function-b"), Is.False);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission("system-a://component/*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission("system-a://*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.AddPermission("*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.True);
    }

    [Test]
    public void Should_be_able_to_determine_whether_a_message_session_has_a_permission()
    {
        var session = new Messages.v1.Session
        {
            Permissions = ["system-a://component-a/function-a"],
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity",
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        Assert.That(session.HasPermission("system-a://component-a/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component-a/function-b"), Is.False);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add("system-a://component/*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.False);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add("system-a://*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.False);

        session.Permissions.Add("*");

        Assert.That(session.HasPermission("system-a://component/function-a"), Is.True);
        Assert.That(session.HasPermission("system-a://component/function-b"), Is.True);
        Assert.That(session.HasPermission("system-a://component-b/function-a"), Is.True);
        Assert.That(session.HasPermission("system-b://component-a/function-a"), Is.True);
    }
}