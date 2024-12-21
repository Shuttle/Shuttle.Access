using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class Columns
{
    public static readonly Column<Guid> Id = new("Id", DbType.Guid);
    public static readonly Column<string> RoleName = new("RoleName", DbType.String);
    public static readonly Column<Guid> PermissionId = new("PermissionId", DbType.Guid);
    public static readonly Column<Guid> RoleId = new("RoleId", DbType.Guid);
    public static readonly Column<string> PermissionName = new("PermissionName", DbType.String);
    public static readonly Column<string> IdentityName = new("IdentityName", DbType.String);
    public static readonly Column<string> Name = new("Name", DbType.String);
    public static readonly Column<string> NameMatch = new("NameMatch", DbType.String);
    public static readonly Column<DateTime> DateActivated = new("DateActivated", DbType.DateTime2);
    public static readonly Column<DateTime> DateRegistered = new("DateRegistered", DbType.DateTime2);
    public static readonly Column<DateTime> ExpiryDate = new("ExpiryDate", DbType.DateTime2);
    public static readonly Column<string> RegisteredBy = new("RegisteredBy", DbType.String);
    public static readonly Column<string> GeneratedPassword = new("GeneratedPassword", DbType.String);
    public static readonly Column<Guid> IdentityId = new("IdentityId", DbType.Guid);
    public static readonly Column<int> Status = new("Status", DbType.Int32);
    public static readonly Column<Guid> Token = new("Token", DbType.Guid);
}