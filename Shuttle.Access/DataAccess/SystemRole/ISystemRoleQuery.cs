using System;
using System.Collections.Generic;
using System.Data;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemRoleQuery
    {
        IEnumerable<string> Permissions(string roleName);
        IEnumerable<DataRow> Search(Query.Role.Specification specification);
        Query.Role Get(Guid id);
        IEnumerable<string> Permissions(Guid id);
    }
}