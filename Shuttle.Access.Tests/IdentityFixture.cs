using NUnit.Framework;

namespace Shuttle.Access.Tests;

[TestFixture]
public class IdentityFixture
{
    [Test]
    public void Should_be_able_to_add_an_alternate_name()
    {
        var identity = new Identity();

        identity.Register("name", "description", [], "registered-by", string.Empty, true);

        var added = identity.AddAlternateName("alternate@example.com");

        Assert.That(added.Name, Is.EqualTo("alternate@example.com"));
        Assert.That(identity.AlternateNames, Is.EquivalentTo(new[] { "alternate@example.com" }));
        Assert.That(identity.IsKnownAs("alternate@example.com"), Is.True);
        Assert.That(identity.IsKnownAs("name"), Is.True);
    }

    [Test]
    public void Should_not_be_able_to_add_a_name_the_identity_is_already_known_by()
    {
        var identity = new Identity();

        identity.Register("name", "description", [], "registered-by", string.Empty, true);
        identity.AddAlternateName("alternate@example.com");

        Assert.That(() => identity.AddAlternateName("alternate@example.com"), Throws.TypeOf<ApplicationException>());
        Assert.That(() => identity.AddAlternateName("name"), Throws.TypeOf<ApplicationException>());
    }

    [Test]
    public void Should_be_able_to_remove_an_alternate_name()
    {
        var identity = new Identity();

        identity.Register("name", "description", [], "registered-by", string.Empty, true);
        identity.AddAlternateName("alternate@example.com");

        var removed = identity.RemoveAlternateName("alternate@example.com");

        Assert.That(removed.Name, Is.EqualTo("alternate@example.com"));
        Assert.That(identity.AlternateNames, Is.Empty);
    }

    [Test]
    public void Should_not_be_able_to_remove_an_unknown_alternate_name()
    {
        var identity = new Identity();

        identity.Register("name", "description", [], "registered-by", string.Empty, true);

        Assert.That(() => identity.RemoveAlternateName("unknown@example.com"), Throws.TypeOf<InvalidOperationException>());
    }
}
