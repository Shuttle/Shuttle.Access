﻿using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class GenerateHashParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_generate_hash_async()
    {
        var hash = new byte[] { 0, 1, 2, 3, 4 };
        var hashingService = new Mock<IHashingService>();
        var generateHash = new GenerateHash { Value = "value" };

        hashingService.Setup(m => m.Sha256(generateHash.Value)).Returns(hash);

        var participant = new GenerateHashParticipant(hashingService.Object);

        await participant.ProcessMessageAsync(new ParticipantContext<GenerateHash>(generateHash, CancellationToken.None));

        Assert.That(generateHash.Hash, Is.Not.Null);
        Assert.That(generateHash.Hash, Is.EqualTo(hash));
    }
}