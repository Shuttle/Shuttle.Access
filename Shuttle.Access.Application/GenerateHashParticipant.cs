using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class GenerateHashParticipant : IParticipant<GenerateHash>
    {
        private readonly IHashingService _hashingService;

        public GenerateHashParticipant(IHashingService hashingService)
        {
            Guard.AgainstNull(hashingService, nameof(hashingService));

            _hashingService = hashingService;
        }

        public void ProcessMessage(IParticipantContext<GenerateHash> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Hash = _hashingService.Sha256(context.Message.Value);
        }
    }
}