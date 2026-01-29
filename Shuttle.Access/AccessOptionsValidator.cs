using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access;

public class AccessOptionsValidator : IValidateOptions<AccessOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessOptions options)
    {
        if (Guid.Empty == options.SystemTenantId)
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(options.SystemTenantId)));
        }

        if (string.IsNullOrEmpty(options.SystemTenantName))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(options.SystemTenantName)));
        }

        return ValidateOptionsResult.Success;
    }
}