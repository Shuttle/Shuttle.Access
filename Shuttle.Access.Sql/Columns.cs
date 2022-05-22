using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class Columns
    {
        public static readonly MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
        public static readonly MappedColumn<string> RoleName = new MappedColumn<string>("RoleName", DbType.String);
        public static readonly MappedColumn<Guid> PermissionId = new MappedColumn<Guid>("PermissionId", DbType.Guid);
        public static readonly MappedColumn<Guid> RoleId = new MappedColumn<Guid>("RoleId", DbType.Guid);
        public static readonly MappedColumn<string> Permission = new MappedColumn<string>("Permission", DbType.String);
        public static readonly MappedColumn<string> IdentityName = new MappedColumn<string>("IdentityName", DbType.String);
        public static readonly MappedColumn<string> Name = new MappedColumn<string>("Name", DbType.String);
        public static readonly MappedColumn<string> NameMatch = new MappedColumn<string>("NameMatch", DbType.String);
        public static readonly MappedColumn<DateTime> DateActivated = new MappedColumn<DateTime>("DateActivated", DbType.DateTime2);
        public static readonly MappedColumn<DateTime> DateRegistered = new MappedColumn<DateTime>("DateRegistered", DbType.DateTime2);
        public static readonly MappedColumn<DateTime> ExpiryDate = new MappedColumn<DateTime>("ExpiryDate", DbType.DateTime2);
        public static readonly MappedColumn<string> RegisteredBy = new MappedColumn<string>("RegisteredBy", DbType.String);
        public static readonly MappedColumn<string> GeneratedPassword = new MappedColumn<string>("GeneratedPassword", DbType.String);
        public static readonly MappedColumn<Guid> IdentityId = new MappedColumn<Guid>("IdentityId", DbType.Guid);
        public static readonly MappedColumn<int> Status = new MappedColumn<int>("Status", DbType.Int32);
        public static readonly MappedColumn<Guid> Token = new MappedColumn<Guid>("Token", DbType.Guid);
    }
}