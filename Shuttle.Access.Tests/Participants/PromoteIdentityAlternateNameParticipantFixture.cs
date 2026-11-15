using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class PromoteIdentityAlternateNameParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_replace_the_primary_name_with_an_alternate_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();

        var id = await SeedAsync(eventStore, "primary@example.com", "alternate@example.com");

        var participant = new PromoteIdentityAlternateNameParticipant(eventStore, idKeyRepository.Object);

        await participant.HandleAsync(new PromoteIdentityAlternateName(id, "alternate@example.com", IdentityAlternateNamePromotionMode.Replace, Guid.NewGuid(), "audit-identity-name"), CancellationToken.None);

        var identity = new Identity();
        (await eventStore.GetAsync(id)).Apply(identity);

        Assert.That(identity.Name, Is.EqualTo("alternate@example.com"));
        Assert.That(identity.AlternateNames, Is.Empty);

        idKeyRepository.Verify(m => m.RemoveAsync(Identity.Key("primary@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_swap_the_primary_name_with_an_alternate_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();

        var id = await SeedAsync(eventStore, "primary@example.com", "alternate@example.com");

        var participant = new PromoteIdentityAlternateNameParticipant(eventStore, idKeyRepository.Object);

        await participant.HandleAsync(new PromoteIdentityAlternateName(id, "alternate@example.com", IdentityAlternateNamePromotionMode.Swap, Guid.NewGuid(), "audit-identity-name"), CancellationToken.None);

        var identity = new Identity();
        (await eventStore.GetAsync(id)).Apply(identity);

        Assert.That(identity.Name, Is.EqualTo("alternate@example.com"));
        Assert.That(identity.AlternateNames, Is.EquivalentTo(new[] { "primary@example.com" }));

        idKeyRepository.Verify(m => m.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Should_throw_when_the_name_is_not_a_known_alternate_name()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();
        var participant = new PromoteIdentityAlternateNameParticipant(eventStore, idKeyRepository.Object);

        Assert.That(async () =>
        {
            var id = await SeedAsync(eventStore, "primary@example.com");

            await participant.HandleAsync(new PromoteIdentityAlternateName(id, "unknown@example.com", IdentityAlternateNamePromotionMode.Replace, Guid.NewGuid(), "audit-identity-name"), CancellationToken.None);
        }, Throws.TypeOf<InvalidOperationException>());
    }

    private static async Task<Guid> SeedAsync(FixtureEventStore eventStore, string name, params string[] alternateNames)
    {
        var id = Guid.NewGuid();
        var stream = await eventStore.GetAsync(id);
        var aggregate = stream.Get<Identity>();

        stream.Add(aggregate.Register(name, "description", [], "registered-by", string.Empty, true));

        foreach (var alternateName in alternateNames)
        {
            stream.Add(aggregate.AddAlternateName(alternateName));
        }

        await eventStore.SaveAsync(stream);

        return id;
    }
}
