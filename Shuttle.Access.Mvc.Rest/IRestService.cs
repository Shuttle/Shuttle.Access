using System;
using System.Collections.Generic;

namespace Shuttle.Access.Mvc.Rest
{
    public interface IRestService
    {
        IEnumerable<string> GetPermissions(Guid token);
    }
}