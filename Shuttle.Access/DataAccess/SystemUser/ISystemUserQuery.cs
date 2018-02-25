using System;
using System.Collections.Generic;
using System.Data;

namespace Shuttle.Access
{
    public interface ISystemUserQuery
    {
        int Count();
        IEnumerable<DataRow> Search();
        Query.User Get(Guid id);
        IEnumerable<string> Roles(Guid id);
        int AdministratorCount();
    }
}