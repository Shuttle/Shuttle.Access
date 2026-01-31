using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Refit;
using Shuttle.Access.AspNetCore;
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

        sessionsApi.Setup(m => m.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>()).Result).Returns(new ApiResponse<IEnumerable<Messages.v1.Session>>(new(HttpStatusCode.BadRequest), null, new()));
        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestSessionService(Options.Create(new AccessAuthorizationOptions()), new NullSessionCache(), accessClient.Object);

        Assert.That(await service.FindAsync(new Messages.v1.Session.Specification()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existing_session()
    {
        var identityId = Guid.NewGuid();
        var accessClient = new Mock<IAccessClient>();
        var sessionsApi = new Mock<ISessionsApi>();

        var response = new Mock<IApiResponse<IEnumerable<Messages.v1.Session>>>();

        response.Setup(m => m.IsSuccessStatusCode).Returns(true);
        response.Setup(m => m.Content).Returns([new() { IdentityId = identityId, Permissions = [], ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(5) }]);
        response.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);

        sessionsApi.Setup(m => m.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(response.Object);

        accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

        var service = new RestSessionService(Options.Create(new AccessAuthorizationOptions { PassThrough = false }), new SessionCache(), accessClient.Object);

        Assert.That(await service.FindAsync(new Messages.v1.Session.Specification { IdentityId = identityId }), Is.Not.Null);
        Assert.That(await service.FindAsync(new Messages.v1.Session.Specification { IdentityId = identityId }), Is.Not.Null); // returned from cache

        accessClient.Verify(m => m.Sessions.PostSearchAsync(It.IsAny<Messages.v1.Session.Specification>(), It.IsAny<CancellationToken>()).Result, Times.Exactly(1));
    }
}