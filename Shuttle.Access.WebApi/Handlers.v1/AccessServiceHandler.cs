using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.WebApi.Handlers.v1;

public class AccessServiceHandler :
    IMessageHandler<IdentityRoleSet>,
    IMessageHandler<RolePermissionSet>,
    IMessageHandler<PermissionStatusSet>
{
    private readonly IAccessService _accessService;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IIdentityQuery _identityQuery;
    private readonly IPermissionQuery _permissionQuery;
    private readonly IProjectionRepository _projectionRepository;
    private readonly ISessionQuery _sessionQuery;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionService _sessionService;

    public AccessServiceHandler(IAccessService accessService, ISessionService sessionService,
        IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository,
        IIdentityQuery identityQuery, IProjectionRepository projectionRepository, IPermissionQuery permissionQuery,
        ISessionQuery sessionQuery)
    {
        Guard.AgainstNull(accessService, nameof(accessService));
        Guard.AgainstNull(sessionService, nameof(sessionService));
        Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
        Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
        Guard.AgainstNull(identityQuery, nameof(identityQuery));
        Guard.AgainstNull(projectionRepository, nameof(projectionRepository));
        Guard.AgainstNull(permissionQuery, nameof(permissionQuery));
        Guard.AgainstNull(sessionQuery, nameof(sessionQuery));

        _accessService = accessService;
        _sessionService = sessionService;
        _databaseContextFactory = databaseContextFactory;
        _sessionRepository = sessionRepository;
        _identityQuery = identityQuery;
        _projectionRepository = projectionRepository;
        _permissionQuery = permissionQuery;
        _sessionQuery = sessionQuery;
    }

    public void ProcessMessage(IHandlerContext<IdentityRoleSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        using (_databaseContextFactory.Create())
        {
            if (_projectionRepository.GetSequenceNumber(ProjectionNames.Identity) < context.Message.SequenceNumber)
            {
                context.Send(context.Message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            Refresh(new DataAccess.Query.Session.Specification().AddPermissions(
                _permissionQuery.Search(
                        new DataAccess.Query.Permission.Specification().AddRoleId(context.Message.RoleId))
                    .Select(item => item.Name)));
        }
    }

    public void ProcessMessage(IHandlerContext<PermissionStatusSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        using (_databaseContextFactory.Create())
        {
            if (_projectionRepository.GetSequenceNumber(ProjectionNames.Permission) < context.Message.SequenceNumber)
            {
                context.Send(context.Message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            Refresh(new DataAccess.Query.Session.Specification().AddPermission(context.Message.Name));
        }
    }

    public void ProcessMessage(IHandlerContext<RolePermissionSet> context)
    {
        Guard.AgainstNull(context, nameof(context));

        using (_databaseContextFactory.Create())
        {
            if (_projectionRepository.GetSequenceNumber(ProjectionNames.Role) < context.Message.SequenceNumber)
            {
                context.Send(context.Message, c => c.Defer(DateTime.UtcNow.AddSeconds(5)).Local());

                return;
            }

            Refresh(new DataAccess.Query.Session.Specification().AddPermissions(_permissionQuery
                .Search(new DataAccess.Query.Permission.Specification().AddId(context.Message.PermissionId))
                .Select(item => item.Name)));
        }
    }

    private void Refresh(DataAccess.Query.Session.Specification specification)
    {
        foreach (var session in _sessionQuery.Search(specification))
        {
            _accessService.Flush(session.Token);
            _sessionService.Refresh(session.Token);
        }
    }
}