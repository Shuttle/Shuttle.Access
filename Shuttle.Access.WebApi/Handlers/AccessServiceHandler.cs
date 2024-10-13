using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.WebApi.Handlers;

public class AccessServiceHandler :
    IAsyncMessageHandler<IdentityRoleSet>,
    IAsyncMessageHandler<RolePermissionSet>,
    IAsyncMessageHandler<PermissionStatusSet>
{
    private readonly IMediator _mediator;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IIdentityQuery _identityQuery;
    private readonly IPermissionQuery _permissionQuery;
    private readonly IProjectionRepository _projectionRepository;
    private readonly ISessionQuery _sessionQuery;

    public AccessServiceHandler(IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, IProjectionRepository projectionRepository, IPermissionQuery permissionQuery, ISessionQuery sessionQuery, IMediator mediator)
    {
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _identityQuery = Guard.AgainstNull(identityQuery);
        _projectionRepository = Guard.AgainstNull(projectionRepository);
        _permissionQuery = Guard.AgainstNull(permissionQuery);
        _sessionQuery = Guard.AgainstNull(sessionQuery);
        _mediator = Guard.AgainstNull(mediator);
    }

    public async Task ProcessMessageAsync(IHandlerContext<IdentityRoleSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message;

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            if (await _projectionRepository.GetSequenceNumberAsync(ProjectionNames.Identity) < message.SequenceNumber)
            {
                await context.SendAsync(message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            if (message.Active)
            {
                await RefreshAsync(new DataAccess.Query.Identity.Specification().WithRoleId(message.RoleId));
            }
            else
            {
                await RefreshAsync(new DataAccess.Query.Session.Specification().AddPermissions(
                    (await _permissionQuery.SearchAsync(new DataAccess.Query.Permission.Specification().AddRoleId(message.RoleId)))
                    .Select(item => item.Name)));
            }
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<PermissionStatusSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message;

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            if (await _projectionRepository.GetSequenceNumberAsync(ProjectionNames.Permission) < message.SequenceNumber)
            {
                await context.SendAsync(message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            if (message.Status == (int)PermissionStatus.Removed)
            {
                await RefreshAsync(new DataAccess.Query.Session.Specification().AddPermission(message.Name));
            }
            else
            {
                await RefreshAsync(new DataAccess.Query.Identity.Specification().WithPermissionId(message.Id));
            }
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<RolePermissionSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message;

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            if (await _projectionRepository.GetSequenceNumberAsync(ProjectionNames.Role) < message.SequenceNumber)
            {
                await context.SendAsync(message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            if (message.Active)
            {
                await RefreshAsync(new DataAccess.Query.Identity.Specification().WithRoleId(message.RoleId));
            }
            else
            { 
                await RefreshAsync(new DataAccess.Query.Session.Specification().AddPermissions(
                    (await _permissionQuery.SearchAsync(new DataAccess.Query.Permission.Specification().AddId(message.PermissionId)))
                    .Select(item => item.Name)));
            }
        }
    }

    private async Task RefreshAsync(DataAccess.Query.Session.Specification specification)
    {
        foreach (var session in await _sessionQuery.SearchAsync(specification))
        {
            await _mediator.SendAsync(new RefreshSession(session.Token));
        }
    }

    private async Task RefreshAsync(DataAccess.Query.Identity.Specification specification)
    {
        foreach (var identity in await _identityQuery.SearchAsync(specification))
        {
            await _mediator.SendAsync(new RefreshSession(identity.Name));
        }
    }
}