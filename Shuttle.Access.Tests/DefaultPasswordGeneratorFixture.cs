using NUnit.Framework;

namespace Shuttle.Access.Tests;

[TestFixture]
public class DefaultPasswordGeneratorFixture
{
    [Test]
    public void Should_be_able_to_generate_a_password()
    {
        var password = new DefaultPasswordGenerator().Generate();

        Assert.That(password, Is.Not.Null);
        Assert.That(password, Is.Not.Empty);
        Assert.That(password.Length, Is.EqualTo(8));
    }
}