using System.Transactions;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class TenantHandler(ITenantQuery tenantQuery, IRoleQuery roleQuery, IIdentityQuery identityQuery, IMediator mediator) :
    IMessageHandler<RegisterTenant>,
    IMessageHandler<RemoveTenant>
{
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);
    private readonly ITenantQuery _tenantQuery = Guard.AgainstNull(tenantQuery);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task HandleAsync(RemoveTenant message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(message, cancellationToken);
    }

    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = (await _identityQuery.SearchAsync(new IdentitySpecification().WithName(message.AdministratorIdentityName), cancellationToken)).FirstOrDefault();

        if (identity == null)
        {
            return;
        }

        message.Id ??= Guid.NewGuid();

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            await mediator.SendAsync(message, cancellationToken);
        }

        SqlServer.Models.Tenant? tenant;

        var timeout = DateTime.UtcNow.AddSeconds(15);
        do
        {
            tenant = (await _tenantQuery.SearchAsync(new TenantSpecification().AddId(message.Id.Value), cancellationToken)).FirstOrDefault();

            if (tenant == null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (tenant == null && DateTime.UtcNow < timeout);

        if (tenant == null)
        {
            throw new ApplicationException($"Timed out waiting for tenant '{message.Name}' to be registered.");
        }

        var auditInformation = new AuditInformation(message.Id.Value, "system");

        var registerRoleMessage = new Application.RegisterRole(Guid.NewGuid(), "Access Administrator", message.Id.Value, auditInformation);

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            await _mediator.SendAsync(registerRoleMessage, cancellationToken);
        }

        SqlServer.Models.Role? role;

        timeout = DateTime.UtcNow.AddSeconds(15);
        do
        {
            role= (await _roleQuery.SearchAsync(new RoleSpecification().AddName("Access Administrator").WithTenantId(message.Id.Value), cancellationToken)).FirstOrDefault();

            if (role== null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (role== null && DateTime.UtcNow < timeout);

        if (role == null)
        {
            throw new ApplicationException("Timed out waiting for role 'Access Administrator' to be registered.");
        }
        
        var registerAdministratorMessage = new SetIdentityTenantStatus
        {
            TenantId = message.Id.Value,
            IdentityId = identity.Id,
            Active = true,
            AuditIdentityName =auditInformation.IdentityName,
            AuditTenantId = auditInformation.TenantId
        };

        await _mediator.SendAsync(registerAdministratorMessage, cancellationToken);

        var setIdentityRoleMessage = new SetIdentityRoleStatus
        {
            IdentityId = identity.Id,
            RoleId = role.Id,
            Active = true,
            AuditIdentityName = auditInformation.IdentityName,
            AuditTenantId = auditInformation.TenantId
        };

        await _mediator.SendAsync(setIdentityRoleMessage, cancellationToken);
    }
}