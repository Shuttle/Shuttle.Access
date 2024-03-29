﻿using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface IIdentityQuery
    {
        int Count(Query.Identity.Specification specification);
        IEnumerable<Query.Identity> Search(Query.Identity.Specification specification);
        IEnumerable<Guid> RoleIds(Query.Identity.Specification specification);
        int AdministratorCount();
        Guid Id(string identityName);
        IEnumerable<string> Permissions(Guid id);
    }
}