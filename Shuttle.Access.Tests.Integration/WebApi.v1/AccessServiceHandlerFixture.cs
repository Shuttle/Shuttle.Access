using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.WebApi.Handlers;
using Shuttle.Hopper;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

[TestFixture]
public class AccessServiceHandlerFixture
{
    private sealed class Fixture
    {
        public Mock<IBus> Bus { get; } = new();
        public Mock<IMediator> Mediator { get; } = new();
        public Mock<IPrimitiveEventQuery> PrimitiveEventQuery { get; } = new();
        public Mock<IProjectionQuery> ProjectionQuery { get; } = new();
        public Mock<ISessionQuery> SessionQuery { get; } = new();

        public Fixture(IEnumerable<Query.Session> sessions, bool projectionsPending)
        {
            SessionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(sessions);
            PrimitiveEventQuery.Setup(m => m.GetMaximumSequenceNumberAsync(It.IsAny<Recall.Query.PrimitiveEvent.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(10);
            ProjectionQuery.Setup(m => m.HasPendingProjectionsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(projectionsPending);
        }

        public AccessServiceHandler CreateHandler()
        {
            return new(Bus.Object, PrimitiveEventQuery.Object, ProjectionQuery.Object, SessionQuery.Object, Mediator.Object);
        }
    }

    private static Query.Session CreateSession()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity-name"
        };
    }

    [Test]
    public async Task Should_neither_check_projections_nor_defer_when_there_are_no_sessions_async()
    {
        var fixture = new Fixture([], projectionsPending: true);

        await fixture.CreateHandler().HandleAsync(new RolePermissionAdded { RoleId = Guid.NewGuid(), PermissionId = Guid.NewGuid() });

        fixture.PrimitiveEventQuery.Verify(m => m.GetMaximumSequenceNumberAsync(It.IsAny<Recall.Query.PrimitiveEvent.Specification>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.ProjectionQuery.Verify(m => m.HasPendingProjectionsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), It.IsAny<Action<TransportMessageBuilder>?>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Mediator.Verify(m => m.SendAsync(It.IsAny<RefreshSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_defer_when_there_are_sessions_and_projections_are_pending_async()
    {
        var fixture = new Fixture([CreateSession()], projectionsPending: true);

        var message = new IdentityRoleAdded { IdentityId = Guid.NewGuid(), RoleId = Guid.NewGuid() };

        await fixture.CreateHandler().HandleAsync(message);

        fixture.Bus.Verify(m => m.SendAsync(message, It.IsAny<Action<TransportMessageBuilder>?>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.Mediator.Verify(m => m.SendAsync(It.IsAny<RefreshSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_refresh_sessions_when_projections_have_caught_up_async()
    {
        var session = CreateSession();
        var fixture = new Fixture([session], projectionsPending: false);

        await fixture.CreateHandler().HandleAsync(new IdentityRoleRemoved { IdentityId = session.IdentityId, RoleId = Guid.NewGuid() });

        fixture.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), It.IsAny<Action<TransportMessageBuilder>?>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Mediator.Verify(m => m.SendAsync(It.Is<RefreshSession>(item => item.Id == session.Id), It.IsAny<CancellationToken>()), Times.Once);
        fixture.Bus.Verify(m => m.PublishAsync(It.Is<SessionRefreshed>(item => item.Id == session.Id), It.IsAny<Action<TransportMessageBuilder>?>(), It.IsAny<CancellationToken>()), Times.Once);

        // the removed role no longer links the identity to its sessions, so they are found by identity
        fixture.SessionQuery.Verify(m => m.SearchAsync(It.Is<Query.Session.Specification>(item => item.IdentityId == session.IdentityId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
