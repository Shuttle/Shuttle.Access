using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemRolePermissionColumns
    {
        public static readonly MappedColumn<Guid> RoleId = new MappedColumn<Guid>("RoleId", DbType.Guid);

        public static readonly MappedColumn<string> Permission =
            new MappedColumn<string>("Permission", DbType.String, 130);
    }
}