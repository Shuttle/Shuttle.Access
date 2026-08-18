using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Access.AspNetCore;

namespace Shuttle.Access.Tests;

[TestFixture]
public class AccessAuthorizationOptionsValidatorFixture
{
    private static bool HasValidator(IServiceCollection services)
    {
        return services.Any(item =>
            item.ServiceType == typeof(IValidateOptions<AccessAuthorizationOptions>) &&
            item.ImplementationType == typeof(AccessAuthorizationOptionsValidator));
    }

    [Test]
    public void Should_fail_when_the_base_address_is_empty()
    {
        Assert.That(new AccessAuthorizationOptionsValidator().Validate(null, new()).Failed, Is.True);
    }

    [Test]
    [TestCase("localhost:5599", Description = "Missing scheme — `Uri.TryCreate` treats 'localhost' as the scheme.")]
    [TestCase("/v1/sessions", Description = "Relative.")]
    [TestCase("ftp://localhost:5599", Description = "Not an HTTP(S) scheme.")]
    public void Should_fail_when_the_base_address_is_not_an_absolute_http_uri(string baseAddress)
    {
        Assert.That(new AccessAuthorizationOptionsValidator().Validate(null, new() { BaseAddress = baseAddress }).Failed, Is.True);
    }

    [Test]
    [TestCase("http://localhost:5599")]
    [TestCase("https://access.example.com")]
    public void Should_succeed_when_the_base_address_is_an_absolute_http_uri(string baseAddress)
    {
        Assert.That(new AccessAuthorizationOptionsValidator().Validate(null, new() { BaseAddress = baseAddress }).Succeeded, Is.True);
    }

    [Test]
    public void Should_register_the_validator_for_the_delegated_resolver()
    {
        var services = new ServiceCollection();

        services.AddAccessAuthorization();

        Assert.That(HasValidator(services), Is.True);
    }

    [Test]
    public void Should_remove_the_validator_when_the_session_resolver_is_replaced()
    {
        var services = new ServiceCollection();

        // An application that resolves sessions itself — the Shuttle.Access web API — has no web API to call and
        // therefore no `BaseAddress` to configure.
        services.AddAccessAuthorization().UseSessionResolver<FixtureSessionResolver>();

        Assert.That(HasValidator(services), Is.False);
    }

    private class FixtureSessionResolver : ISessionResolver
    {
        public Task<SessionResolutionResult> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SessionResolutionResult.None);
        }
    }
}
