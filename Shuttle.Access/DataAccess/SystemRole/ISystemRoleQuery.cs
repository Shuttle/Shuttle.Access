using System;
using System.Collections.Generic;
using System.Data;

namespace Shuttle.Access
{
    public interface ISystemRoleQuery
    {
        IEnumerable<string> Permissions(string roleName);
        IEnumerable<DataRow> Search();
        Query.Role Get(Guid id);
        IEnumerable<string> Permissions(Guid id);
    }
}