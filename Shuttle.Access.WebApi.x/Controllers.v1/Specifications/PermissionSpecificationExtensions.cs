using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi.Controllers.v1;

public static class PermissionSpecificationExtensions
{
    public static DataAccess.Query.Permission.Specification Create(this PermissionSpecification specification)
    {
        Guard.AgainstNull(specification, nameof(specification));

        return new DataAccess.Query.Permission.Specification()
            .AddIds(specification.Ids);
    }
}