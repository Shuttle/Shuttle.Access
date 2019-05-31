using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQueryFactory
    {
        IQuery Available();
    }
}