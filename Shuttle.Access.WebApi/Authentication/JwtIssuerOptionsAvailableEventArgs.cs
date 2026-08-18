using Microsoft.IdentityModel.JsonWebTokens;
using Shuttle.Contract;

namespace Shuttle.Access.WebApi;

public class JwtIssuerOptionsAvailableEventArgs(JsonWebToken jsonWebToken, IssuerOptions issuerOptions)
{
    public IssuerOptions IssuerOptions { get; } = Guard.AgainstNull(issuerOptions);
    public JsonWebToken JsonWebToken { get; } = Guard.AgainstNull(jsonWebToken);
}
