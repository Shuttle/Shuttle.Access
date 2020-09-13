using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemUserQuery
    {
        int Count(Query.User.Specification specification);
        IEnumerable<Query.User> Search(Query.User.Specification specification);
        IEnumerable<Guid> RoleIds(Query.User.Specification specification);
        int AdministratorCount();
        Guid Id(string username);
        IEnumerable<string> Permissions(Guid userId);
    }
}