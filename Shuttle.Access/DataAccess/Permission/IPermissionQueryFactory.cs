using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQueryFactory
    {
        IQuery Available();
        IQuery Register(string permission);
        IQuery Remove(string permission);
        IQuery Count();
    }
}