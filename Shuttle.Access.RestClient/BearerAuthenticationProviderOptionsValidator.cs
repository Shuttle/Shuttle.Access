using Microsoft.Extensions.Options;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationProviderOptionsValidator : IValidateOptions<BearerAuthenticationProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, BearerAuthenticationProviderOptions options)
    {
        if (options.GetTokenAsync == null)
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.OptionMissingException, "GetTokenAsync"));
        }

        return ValidateOptionsResult.Success;
    }
}