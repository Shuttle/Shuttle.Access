using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserRoleColumns
    {
        public static readonly MappedColumn<Guid> UserId = new MappedColumn<Guid>("UserId", DbType.Guid);
        public static readonly MappedColumn<Guid> RoleId = new MappedColumn<Guid>("RoleId", DbType.Guid);
    }
}