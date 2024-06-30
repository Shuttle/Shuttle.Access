using System;

namespace Shuttle.Access.DataAccess
{
    public class IdentitySpecification
    {
        public Guid? Id { get; private set; }
        public string Name { get; private set; }
        public Guid? PermissionId { get; private set; }
        public Guid? RoleId { get; private set; }
        public string RoleName { get; private set; }
        public bool RolesIncluded { get; private set; }
        public DateTime? StartDateRegistered { get; private set; }

        public IdentitySpecification IncludeRoles()
        {
            RolesIncluded = true;

            return this;
        }

        public IdentitySpecification WithIdentityId(Guid id)
        {
            Id = id;

            return this;
        }

        public IdentitySpecification WithName(string name)
        {
            Name = name;

            return this;
        }

        public IdentitySpecification WithPermissionId(Guid permissionId)
        {
            PermissionId = permissionId;

            return this;
        }

        public IdentitySpecification WithRoleId(Guid roleId)
        {
            RoleId = roleId;

            return this;
        }

        public IdentitySpecification WithRoleName(string roleName)
        {
            RoleName = roleName;

            return this;
        }

        public IdentitySpecification WithStartDateRegistered(DateTime date)
        {
            StartDateRegistered = date;

            return this;
        }
    }
}