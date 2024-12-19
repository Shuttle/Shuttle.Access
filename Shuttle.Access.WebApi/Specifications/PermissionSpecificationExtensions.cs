using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi.Specifications
{
    public static class PermissionSpecificationExtensions
    {
        public static DataAccess.Query.Permission.Specification Create(this Messages.v1.Permission.Specification specification)
        {
            Guard.AgainstNull(specification);

            return new DataAccess.Query.Permission.Specification()
                .AddIds(specification.Ids);
        }
    }
}