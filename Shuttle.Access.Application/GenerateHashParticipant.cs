using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class GenerateHashParticipant(IHashingService hashingService) : IParticipant<GenerateHash>
{
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);

    public async Task ProcessMessageAsync(GenerateHash message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        message.Hash = _hashingService.Sha256(message.Value);

        await Task.CompletedTask;
    }
}