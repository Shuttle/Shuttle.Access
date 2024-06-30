using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using PermissionSpecification = Shuttle.Access.DataAccess.PermissionSpecification;

namespace Shuttle.Access.WebApi.Specifications
{
    public static class PermissionSpecificationExtensions
    {
        public static PermissionSpecification Create(this PermissionSpecification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return new PermissionSpecification()
                .AddIds(specification.Ids);
        }
    }
}