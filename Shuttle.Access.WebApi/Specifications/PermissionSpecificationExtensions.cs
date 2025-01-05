using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi.Specifications;

public static class PermissionSpecificationExtensions
{
    public static DataAccess.Permission.Specification Create(this Messages.v1.Permission.Specification specification)
    {
        Guard.AgainstNull(specification);

        return new DataAccess.Permission.Specification()
            .AddIds(specification.Ids);
    }
}