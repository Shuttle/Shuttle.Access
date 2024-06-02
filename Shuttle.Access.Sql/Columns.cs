using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class Columns
    {
        public static readonly Column<Guid> Id = new Column<Guid>("Id", DbType.Guid);
        public static readonly Column<string> RoleName = new Column<string>("RoleName", DbType.String);
        public static readonly Column<Guid> PermissionId = new Column<Guid>("PermissionId", DbType.Guid);
        public static readonly Column<Guid> RoleId = new Column<Guid>("RoleId", DbType.Guid);
        public static readonly Column<string> PermissionName = new Column<string>("PermissionName", DbType.String);
        public static readonly Column<string> IdentityName = new Column<string>("IdentityName", DbType.String);
        public static readonly Column<string> Name = new Column<string>("Name", DbType.String);
        public static readonly Column<string> NameMatch = new Column<string>("NameMatch", DbType.String);
        public static readonly Column<DateTime> DateActivated = new Column<DateTime>("DateActivated", DbType.DateTime2);
        public static readonly Column<DateTime> DateRegistered = new Column<DateTime>("DateRegistered", DbType.DateTime2);
        public static readonly Column<DateTime> ExpiryDate = new Column<DateTime>("ExpiryDate", DbType.DateTime2);
        public static readonly Column<string> RegisteredBy = new Column<string>("RegisteredBy", DbType.String);
        public static readonly Column<string> GeneratedPassword = new Column<string>("GeneratedPassword", DbType.String);
        public static readonly Column<Guid> IdentityId = new Column<Guid>("IdentityId", DbType.Guid);
        public static readonly Column<int> Status = new Column<int>("Status", DbType.Int32);
        public static readonly Column<Guid> Token = new Column<Guid>("Token", DbType.Guid);
    }
}