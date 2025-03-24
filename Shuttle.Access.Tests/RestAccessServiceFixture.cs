using System;
using System.Collections.Generic;
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
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var accessClient = new Mock<IAccessClient>();
        var sessionsApi = new Mock<ISessionsApi>();

        sessionsApi.Setup(m => m.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>()).Result).Returns(new ApiResponse<IEnumerable<Messages.v1.SessionResponse>>(new(HttpStatusCode.BadRequest), null, new()));
        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestSessionCache(Options.Create(new AccessOptions()), accessClient.Object);

        Assert.That(await service.FindAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existing_session()
    {
        var accessClient = new Mock<IAccessClient>();
        var sessionsApi = new Mock<ISessionsApi>();

        sessionsApi.Setup(m => m.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>()).Result).Returns(new ApiResponse<IEnumerable<Messages.v1.SessionResponse>>(new(HttpStatusCode.OK), [new() { Permissions = new() }], new()));

        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestSessionCache(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), accessClient.Object);

        Assert.That(await service.FindAsync(Guid.NewGuid()), Is.Not.Null);

        accessClient.Verify(m => m.Sessions.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>()).Result, Times.Exactly(1));
    }
}