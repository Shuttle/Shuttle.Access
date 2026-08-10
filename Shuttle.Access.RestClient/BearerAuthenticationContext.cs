using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationContext(string bearer)
{
    public string Bearer { get; } = Guard.AgainstEmpty(bearer);
}