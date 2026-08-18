using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Ensures that the address of the Shuttle.Access web API has been configured, since the default
///     <see cref="DelegatedSessionResolver" /> cannot resolve a caller's session without it.
/// </summary>
/// <remarks>
///     <see cref="AccessAuthorizationBuilder.UseSessionResolver{T}" /> removes this validator — an application that
///     resolves sessions itself, such as the Shuttle.Access web API, has no web API to call.
/// </remarks>
public class AccessAuthorizationOptionsValidator : IValidateOptions<AccessAuthorizationOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessAuthorizationOptions options)
    {
        Guard.AgainstNull(options);

        if (string.IsNullOrWhiteSpace(options.BaseAddress))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionMissingException, nameof(AccessAuthorizationOptions.BaseAddress)));
        }

        // `Uri.TryCreate` accepts "localhost:5599" as absolute — 'localhost' becomes the scheme — so the scheme has to
        // be checked explicitly, since a missing "http://" is the likely misconfiguration.
        return !Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out var uri) ||
               (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            ? ValidateOptionsResult.Fail(string.Format(Resources.OptionUriException, nameof(AccessAuthorizationOptions.BaseAddress)))
            : ValidateOptionsResult.Success;
    }
}
