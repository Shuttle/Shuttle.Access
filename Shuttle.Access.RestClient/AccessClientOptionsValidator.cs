using System;
using Microsoft.Extensions.Options;

namespace Shuttle.Access.RestClient;

public class AccessClientOptionsValidator : IValidateOptions<AccessClientOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseAddress))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionMissingException, "BaseAddress"));
        }

        if (!Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionUriException, "BaseAddress"));
        }

        if (string.IsNullOrWhiteSpace(options.IdentityName))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionMissingException, "IdentityName"));
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionMissingException, "Password"));
        }

        return ValidateOptionsResult.Success;
    }
}