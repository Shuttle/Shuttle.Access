using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Refit;
using Shuttle.Access.RestClient;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.Tests;

[TestFixture]
public class RestAccessServiceFixture
{
    private readonly Session _session = new(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var accessClient = new Mock<IAccessClient>();
        var sessionsApi = new Mock<ISessionsApi>();

        sessionsApi.Setup(m => m.GetAsync(It.IsAny<Guid>()).Result).Returns(new ApiResponse<Messages.v1.Session>(new(HttpStatusCode.BadRequest), null, new()));
        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestAccessService(Options.Create(new AccessOptions()), accessClient.Object);

        Assert.That(await service.ContainsAsync(Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existing_session()
    {
        var accessClient = new Mock<IAccessClient>();
        var sessionsApi = new Mock<ISessionsApi>();

        sessionsApi.Setup(m => m.GetAsync(It.IsAny<Guid>()).Result).Returns(new ApiResponse<Messages.v1.Session>(new(HttpStatusCode.OK), new() { Permissions = new() }, new()));

        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestAccessService(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), accessClient.Object);

        Assert.That(await service.ContainsAsync(_session.Token), Is.True);

        accessClient.Verify(m => m.Sessions.GetAsync(It.IsAny<Guid>()).Result, Times.Exactly(1));
    }
}