using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Sql;

public class AuthenticationService : IAuthenticationService
{
    private readonly IEventStore _eventStore;
    private readonly IHashingService _hashingService;
    private readonly IIdKeyRepository _idKeyRepository;

    public AuthenticationService(IEventStore eventStore, IIdKeyRepository idKeyRepository, IHashingService hashingService)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _idKeyRepository = Guard.AgainstNull(idKeyRepository);
        _hashingService = Guard.AgainstNull(hashingService);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string identityName, string password, CancellationToken cancellationToken = default)
    {
        var id = await _idKeyRepository.FindAsync(Identity.Key(identityName), cancellationToken);

        if (!id.HasValue)
        {
            Authentication.Invoke(this, new(false, string.Format(Resources.InvalidIdentity, identityName)));

            return AuthenticationResult.Failure;
        }

        var identity = new Identity();

        (await _eventStore.GetAsync(id.Value)).Apply(identity);

        if (identity.PasswordMatches(_hashingService.Sha256(password)))
        {
            Authentication.Invoke(this, new(true, string.Empty));

            return AuthenticationResult.Success;
        }

        Authentication.Invoke(this, new(false, string.Format(Resources.InvalidPassword, identityName)));

        return AuthenticationResult.Failure;
    }

    public event EventHandler<AuthenticationEventArgs> Authentication = delegate
    {
    };
}