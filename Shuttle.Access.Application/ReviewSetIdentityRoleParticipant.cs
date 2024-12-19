﻿using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ReviewSetIdentityRoleParticipant : IParticipant<RequestMessage<SetIdentityRole>>
{
    private readonly IIdentityQuery _identityQuery;
    private readonly IRoleQuery _roleQuery;

    public ReviewSetIdentityRoleParticipant(IRoleQuery roleQuery, IIdentityQuery identityQuery)
    {
        Guard.AgainstNull(roleQuery);
        Guard.AgainstNull(identityQuery);

        _roleQuery = roleQuery;
        _identityQuery = identityQuery;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestMessage<SetIdentityRole>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;
        var roles = (await _roleQuery.SearchAsync(new DataAccess.Query.Role.Specification().AddName("Administrator"))).ToList();

        if (roles.Count != 1)
        {
            return;
        }

        var role = roles[0];

        if (request.RoleId.Equals(role.Id)
            &&
            !request.Active
            &&
            await _identityQuery.AdministratorCountAsync() == 1)
        {
            context.Message.Failed("last-administrator");
        }
    }
}