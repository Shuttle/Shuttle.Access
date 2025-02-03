using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class GenerateHashParticipant : IParticipant<GenerateHash>
{
    private readonly IHashingService _hashingService;

    public GenerateHashParticipant(IHashingService hashingService)
    {
        _hashingService = Guard.AgainstNull(hashingService);
    }

    public async Task ProcessMessageAsync(IParticipantContext<GenerateHash> context)
    {
        Guard.AgainstNull(context);

        context.Message.Hash = _hashingService.Sha256(context.Message.Value);

        await Task.CompletedTask;
    }
}