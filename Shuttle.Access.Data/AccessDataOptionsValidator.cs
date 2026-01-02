using Microsoft.Extensions.Options;

namespace Shuttle.Access.Data;

public class AccessDataOptionsValidator : IValidateOptions<AccessDataOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessDataOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Options 'ConnectionString' may not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}