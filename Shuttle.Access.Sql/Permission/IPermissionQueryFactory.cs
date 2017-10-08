using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public interface IPermissionQueryFactory
    {
        IQuery Available();
    }
}