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

        foreach (var knownApplication in options.KnownApplications)
        {
            if (string.IsNullOrWhiteSpace(knownApplication.Name))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(knownApplication.Name)));
            }

            if (string.IsNullOrWhiteSpace(knownApplication.Title))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(knownApplication.Title)));
            }

            if (string.IsNullOrWhiteSpace(knownApplication.Description))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(knownApplication.Description)));
            }

            if (string.IsNullOrWhiteSpace(knownApplication.SessionTokenExchangeUrl))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(knownApplication.SessionTokenExchangeUrl)));
            }

            try
            {
                _ = new Uri(knownApplication.SessionTokenExchangeUrl);
            }
            catch
            {
                return ValidateOptionsResult.Fail($"Option 'SessionTokenExchangeUrl' value '{knownApplication.SessionTokenExchangeUrl}' is not a valid URI.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}