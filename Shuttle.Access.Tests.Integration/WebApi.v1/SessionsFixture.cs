﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class SessionsFixture
{
    private const string Permission = "integration://system-permission";

    [Test]
    public async Task Should_be_able_to_get_a_session_using_the_token_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var session = new Access.DataAccess.Session
        {
            IdentityId = Guid.NewGuid()
        };

        factory.SessionQuery.Setup(m => m.SearchAsync(It.IsAny<Access.DataAccess.Session.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new List<Access.DataAccess.Session> { session }.AsEnumerable()));

        var client = factory.GetAccessClient();

        var response = await client.Sessions.PostSearchAsync(new());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content!.First().IdentityId, Is.EqualTo(session.IdentityId));
    }

    [Test]
    public async Task Should_be_able_to_register_a_session_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var sessionToken = Guid.NewGuid();
        var session = new Session(sessionToken.ToByteArray(), Guid.NewGuid(), "identity-name", DateTimeOffset.Now, DateTimeOffset.Now);

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RegisterSession>(), default))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RegisterSession)message).Registered(sessionToken, session);
            });

        var client = factory.GetAccessClient();

        var response = await client.Sessions.PostAsync(
            new Messages.v1.RegisterSession
            {
                IdentityName = "identity",
                Password = "password"
            });

        var sessionRegistered = response;

        Assert.That(sessionRegistered, Is.Not.Null);
        Assert.That(sessionRegistered.IsSuccessStatusCode, Is.True);
        Assert.That(sessionRegistered.Content, Is.Not.Null);
        Assert.That(sessionRegistered.Content!.Token, Is.EqualTo(sessionToken));
        Assert.That(sessionRegistered.Content.IdentityName, Is.EqualTo(session.IdentityName));

        factory.DatabaseContextFactory.Verify(m => m.Create(), Times.AtLeast(1));
        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<RegisterSession>(), default), Times.Once);
    }
}