using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.SqlServer;

public class AuthenticationService(IOptions<AccessOptions> accessOptions, IEventStore eventStore, IIdKeyRepository idKeyRepository, IHashingService hashingService)
    : IAuthenticationService
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task<AuthenticationResult> AuthenticateAsync(string identityName, string password, CancellationToken cancellationToken = default)
    {
        var id = await _idKeyRepository.FindAsync(Identity.Key(identityName), cancellationToken);

        if (!id.HasValue)
        {
            await _accessOptions.AuthenticationUnknownIdentity.InvokeAsync(new(identityName), cancellationToken);

            return AuthenticationResult.Failure;
        }

        var identity = new Identity();

        (await _eventStore.GetAsync(id.Value, cancellationToken: cancellationToken)).Apply(identity);

        if (identity.PasswordMatches(_hashingService.Sha256(password)))
        {
            await _accessOptions.Authenticated.InvokeAsync(new(identityName), cancellationToken);

            return AuthenticationResult.Success;
        }

        await _accessOptions.AuthenticationFailed.InvokeAsync(new(identityName), cancellationToken);

        return AuthenticationResult.Failure;
    }
}