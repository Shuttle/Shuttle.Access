using System;
using System.Collections.Generic;
using Shuttle.Access.DataAccess.Query;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemUserQuery
    {
        int Count(Query.User.Specification specification);
        IEnumerable<Query.User> Search(Query.User.Specification specification);
        UserExtended GetExtended(Guid id);
        IEnumerable<Guid> Roles(Guid id);
        int AdministratorCount();
    }
}