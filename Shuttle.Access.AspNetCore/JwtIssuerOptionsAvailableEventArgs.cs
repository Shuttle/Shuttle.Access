using Microsoft.IdentityModel.JsonWebTokens;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class JwtIssuerOptionsAvailableEventArgs(JsonWebToken jsonWebToken, IssuerOptions issuerOptions)
{
    public IssuerOptions IssuerOptions { get; } = Guard.AgainstNull(issuerOptions);
    public JsonWebToken JsonWebToken { get; } = Guard.AgainstNull(jsonWebToken);
}