using Shuttle.Core.Data;

namespace Shuttle.Access.Sql.Permission
{
    public interface IPermissionQueryFactory
    {
        IQuery Available();
    }
}