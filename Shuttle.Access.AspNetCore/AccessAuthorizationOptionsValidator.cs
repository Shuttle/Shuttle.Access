using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptionsValidator : IValidateOptions<AccessAuthorizationOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessAuthorizationOptions options)
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