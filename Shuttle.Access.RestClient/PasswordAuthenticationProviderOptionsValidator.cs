using Microsoft.Extensions.Options;

namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationProviderOptionsValidator : IValidateOptions<PasswordAuthenticationProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordAuthenticationProviderOptions options)
    {
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