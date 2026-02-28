using Microsoft.Extensions.Options;

namespace Shuttle.Access.SqlServer;

public class AccessSqlServerOptionsValidator : IValidateOptions<AccessSqlServerOptions>
{
    public ValidateOptionsResult Validate(string? name, AccessSqlServerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Options 'ConnectionString' may not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}