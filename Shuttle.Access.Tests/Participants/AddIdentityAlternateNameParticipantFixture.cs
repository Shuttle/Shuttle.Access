using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class AddIdentityAlternateNameParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_add_an_alternate_name_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();

        var id = await SeedAsync(eventStore);

        idKeyRepository.Setup(m => m.FindAsync(Identity.Key("alternate@example.com"), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        var participant = new AddIdentityAlternateNameParticipant(eventStore, idKeyRepository.Object);

        await participant.HandleAsync(new AddIdentityAlternateName(id, "alternate@example.com", Guid.NewGuid(), "audit-identity-name"), CancellationToken.None);

        var identity = new Identity();
        (await eventStore.GetAsync(id)).Apply(identity);

        Assert.That(identity.AlternateNames, Is.EquivalentTo(new[] { "alternate@example.com" }));

        idKeyRepository.Verify(m => m.AddAsync(id, Identity.Key("alternate@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Should_not_be_able_to_add_an_alternate_name_already_used_by_another_identity()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();
        var otherId = Guid.NewGuid();

        idKeyRepository.Setup(m => m.FindAsync(Identity.Key("taken@example.com"), It.IsAny<CancellationToken>())).ReturnsAsync(otherId);

        var participant = new AddIdentityAlternateNameParticipant(eventStore, idKeyRepository.Object);

        Assert.That(async () =>
        {
            var id = await SeedAsync(eventStore);

            await participant.HandleAsync(new AddIdentityAlternateName(id, "taken@example.com", Guid.NewGuid(), "audit-identity-name"), CancellationToken.None);
        }, Throws.TypeOf<ApplicationException>());
    }

    private static async Task<Guid> SeedAsync(FixtureEventStore eventStore)
    {
        var id = Guid.NewGuid();
        var stream = await eventStore.GetAsync(id);
        var aggregate = stream.Get<Identity>();

        stream.Add(aggregate.Register("name", "description", [], "registered-by", string.Empty, true));

        await eventStore.SaveAsync(stream);

        return id;
    }
}
