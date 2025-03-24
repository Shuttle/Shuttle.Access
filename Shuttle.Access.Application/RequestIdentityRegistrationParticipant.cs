using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Application;

public class RequestIdentityRegistrationParticipant : IParticipant<RequestIdentityRegistration>
{
    private readonly IHashingService _hashingService;
    private readonly IServiceBus _serviceBus;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMediator _mediator;

    public RequestIdentityRegistrationParticipant(IServiceBus serviceBus, IHashingService hashingService, ISessionRepository sessionRepository, IMediator mediator)
    {
        _serviceBus = Guard.AgainstNull(serviceBus);
        _hashingService = Guard.AgainstNull(hashingService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
        _mediator = Guard.AgainstNull(mediator);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestIdentityRegistration> context)
    {
        Guard.AgainstNull(context);

        if (context.Message.IdentityId.HasValue)
        {
            var session = await _sessionRepository.FindAsync(context.Message.IdentityId.Value, context.CancellationToken);

            if (session != null && session.HasPermission(AccessPermissions.Identities.Register))
            {
                context.Message.Allowed(session.IdentityName, session.HasPermission(AccessPermissions.Identities.Activate));
            }
        }

        if (!context.Message.IsAllowed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(context.Message.RegisterIdentityMessage.Password))
        {
            var generatePassword = new GeneratePassword();

            await _mediator.SendAsync(generatePassword);

            context.Message.RegisterIdentityMessage.Password = generatePassword.GeneratedPassword;
        }

        var generateHash = new GenerateHash { Value = context.Message.RegisterIdentityMessage.Password };

        await _mediator.SendAsync(generateHash);

        context.Message.RegisterIdentityMessage.Password = string.Empty;
        context.Message.RegisterIdentityMessage.PasswordHash = generateHash.Hash;
        context.Message.RegisterIdentityMessage.RegisteredBy = context.Message.RegisteredBy;
        context.Message.RegisterIdentityMessage.Activated = context.Message.RegisterIdentityMessage.Activated && context.Message.IsActivationAllowed;
        context.Message.RegisterIdentityMessage.System = context.Message.RegisterIdentityMessage.System;

        await _serviceBus.SendAsync(context.Message.RegisterIdentityMessage);
    }
}