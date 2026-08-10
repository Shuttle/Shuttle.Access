using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.WebApi;

public class AccessAuthenticationOptionsValidator : IValidateOptions<AccessAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessAuthenticationOptions options)
    {
        Guard.AgainstNull(options);

        foreach (var issuerOptions in options.Issuers)
        {
            if (string.IsNullOrWhiteSpace(issuerOptions.JwksUri))
            {
                return ValidateOptionsResult.Fail("JwksUri is required.");
            }

            if (string.IsNullOrWhiteSpace(issuerOptions.Uri))
            {
                return ValidateOptionsResult.Fail("Uri is required.");
            }

            if (issuerOptions.IdentityNameClaimTypes.Count == 0)
            {
                return ValidateOptionsResult.Fail("At least one entry is required in 'IdentityNameClaimTypes'.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
