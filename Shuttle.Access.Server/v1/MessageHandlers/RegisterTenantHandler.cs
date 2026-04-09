using System.Transactions;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterTenantHandler(ITenantQuery tenantQuery, IRoleQuery roleQuery, IIdentityQuery identityQuery, IMediator mediator) :  IMessageHandler<RegisterTenant>
{
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);
    private readonly ITenantQuery _tenantQuery = Guard.AgainstNull(tenantQuery);

    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = (await _identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.AdministratorIdentityName), cancellationToken)).FirstOrDefault();

        if (identity == null)
        {
            return;
        }

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            await mediator.SendAsync(message, cancellationToken);
        }

        Query.Tenant? tenant;

        var timeout = DateTime.UtcNow.AddSeconds(15);
        do
        {
            tenant = (await _tenantQuery.SearchAsync(new Query.Tenant.Specification().AddId(message.Id), cancellationToken)).FirstOrDefault();

            if (tenant == null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (tenant == null && DateTime.UtcNow < timeout);

        if (tenant == null)
        {
            throw new ApplicationException($"Timed out waiting for tenant '{message.Name}' to be registered.");
        }

        var auditInformation = new AuditInformation(message.Id, "system");

        var registerRoleMessage = new Application.RegisterRole(Guid.NewGuid(), message.Id, "Access Administrator", message.AuditTenantId, message.AuditIdentityName);

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            await _mediator.SendAsync(registerRoleMessage, cancellationToken);
        }

        Query.Role? role;

        timeout = DateTime.UtcNow.AddSeconds(15);
        do
        {
            role = (await _roleQuery.SearchAsync(new Query.Role.Specification().AddName("Access Administrator").WithTenantId(message.Id), cancellationToken)).FirstOrDefault();

            if (role == null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (role == null && DateTime.UtcNow < timeout);

        if (role == null)
        {
            throw new ApplicationException("Timed out waiting for role 'Access Administrator' to be registered.");
        }

        var registerAdministratorMessage = new SetIdentityTenantStatus
        {
            TenantId = message.Id,
            IdentityId = identity.Id,
            Active = true,
            AuditIdentityName = auditInformation.AuditIdentityName,
            AuditTenantId = auditInformation.AuditTenantId
        };

        await _mediator.SendAsync(registerAdministratorMessage, cancellationToken);

        var setIdentityRoleMessage = new SetIdentityRoleStatus
        {
            IdentityId = identity.Id,
            RoleId = role.Id,
            Active = true,
            AuditIdentityName = auditInformation.AuditIdentityName,
            AuditTenantId = auditInformation.AuditTenantId
        };

        await _mediator.SendAsync(setIdentityRoleMessage, cancellationToken);
    }
}