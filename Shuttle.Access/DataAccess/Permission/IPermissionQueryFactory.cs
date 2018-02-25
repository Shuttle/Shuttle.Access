using Shuttle.Core.Data;

namespace Shuttle.Access
{
    public interface IPermissionQueryFactory
    {
        IQuery Available();
    }
}