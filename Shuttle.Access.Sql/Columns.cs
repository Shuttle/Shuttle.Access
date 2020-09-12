using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class Columns
    {
        public static readonly MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
        public static readonly MappedColumn<string> RoleName = new MappedColumn<string>("RoleName", DbType.String);
        public static readonly MappedColumn<string> RoleNameMatch = new MappedColumn<string>("RoleNameMatch", DbType.String);
        public static readonly MappedColumn<Guid> RoleId = new MappedColumn<Guid>("RoleId", DbType.Guid);
        public static readonly MappedColumn<string> Permission = new MappedColumn<string>("Permission", DbType.String);
        public static readonly MappedColumn<string> Username = new MappedColumn<string>("Username", DbType.String);
        public static readonly MappedColumn<DateTime> DateRegistered = new MappedColumn<DateTime>("DateRegistered", DbType.DateTime);
        public static readonly MappedColumn<DateTime> ExpiryDate = new MappedColumn<DateTime>("ExpiryDate", DbType.DateTime);
        public static readonly MappedColumn<string> RegisteredBy = new MappedColumn<string>("RegisteredBy", DbType.String);
        public static readonly MappedColumn<string> GeneratedPassword = new MappedColumn<string>("GeneratedPassword", DbType.String);
        public static readonly MappedColumn<Guid> UserId = new MappedColumn<Guid>("UserId", DbType.Guid);
        public static readonly MappedColumn<Guid> Token = new MappedColumn<Guid>("Token", DbType.Guid);
    }
}