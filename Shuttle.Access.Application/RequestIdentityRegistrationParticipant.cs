using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Application;

public class RequestIdentityRegistrationParticipant(IBus bus, ISessionRepository sessionRepository, IMediator mediator)
    : IParticipant<RequestIdentityRegistration>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task HandleAsync(RequestIdentityRegistration message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (message is { TenantId: not null, IdentityId: not null })
        {
            var session = await _sessionRepository.FindAsync(new SessionSpecification().WithTenantId(message.TenantId.Value).WithIdentityId(message.IdentityId.Value), cancellationToken);

            if (session != null && session.HasPermission(AccessPermissions.Identities.Register))
            {
                message.Allowed(session.IdentityName, session.HasPermission(AccessPermissions.Identities.Activate));
            }
        }

        if (!message.IsAllowed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message.RegisterIdentityMessage.Password))
        {
            var generatePassword = new GeneratePassword();

            await _mediator.SendAsync(generatePassword, cancellationToken);

            message.RegisterIdentityMessage.Password = generatePassword.GeneratedPassword;
        }

        var generateHash = new GenerateHash { Value = message.RegisterIdentityMessage.Password };

        await _mediator.SendAsync(generateHash, cancellationToken);

        message.RegisterIdentityMessage.Password = string.Empty;
        message.RegisterIdentityMessage.PasswordHash = generateHash.Hash;
        message.RegisterIdentityMessage.AuditIdentityName = message.RegisteredBy;
        message.RegisterIdentityMessage.Activated = message.RegisterIdentityMessage.Activated && message.IsActivationAllowed;
        message.RegisterIdentityMessage.System = message.RegisterIdentityMessage.System;

        await _bus.SendAsync(message.RegisterIdentityMessage, cancellationToken);
    }
}