using System;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace Shuttle.Access;

public class AccessOptionsValidator: IValidateOptions<AccessOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionStringName))
        {
            return ValidateOptionsResult.Fail(string.Format(Resources.RequiredOptionMissing, nameof(options.ConnectionStringName)));
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

            Uri uri;

            try
            {
                uri = new(knownApplication.SessionTokenExchangeUrl);
            }
            catch
            {
                return ValidateOptionsResult.Fail($"Option 'SessionTokenExchangeUrl' value '{knownApplication.SessionTokenExchangeUrl}' is not a valid URI.");
            }

            knownApplication.SessionTokenExchangeUrl = uri.ToString().TrimEnd('/');
        }

        return ValidateOptionsResult.Success;
    }
}