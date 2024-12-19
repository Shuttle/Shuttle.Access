﻿using System;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface IRoleQueryFactory
{
    IQuery Count(Query.Role.Specification specification);
    IQuery Get(Guid id);
    IQuery NameSet(Guid id, NameSet domainEvent);
    IQuery PermissionAdded(Guid id, PermissionAdded domainEvent);
    IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent);
    IQuery Permissions(Query.Role.Specification specification);
    IQuery Registered(Guid id, Registered domainEvent);
    IQuery Removed(Guid id);
    IQuery Search(Query.Role.Specification specification);
}