using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class Columns
{
    public static readonly Column<DateTimeOffset> DateActivated = new("DateActivated", DbType.DateTimeOffset);
    public static readonly Column<DateTimeOffset> DateRegistered = new("DateRegistered", DbType.DateTimeOffset);
    public static readonly Column<Guid> ExchangeToken = new("ExchangeToken", DbType.Guid);
    public static readonly Column<DateTimeOffset> ExpiryDate = new("ExpiryDate", DbType.DateTimeOffset);
    public static readonly Column<string> GeneratedPassword = new("GeneratedPassword", DbType.String);
    public static readonly Column<Guid> Id = new("Id", DbType.Guid);
    public static readonly Column<Guid> IdentityId = new("IdentityId", DbType.Guid);
    public static readonly Column<string> IdentityName = new("IdentityName", DbType.String);
    public static readonly Column<string> IdentityNameMatch = new("IdentityNameMatch", DbType.String);
    public static readonly Column<string> Name = new("Name", DbType.String);
    public static readonly Column<string> NameMatch = new("NameMatch", DbType.String);
    public static readonly Column<Guid> PermissionId = new("PermissionId", DbType.Guid);
    public static readonly Column<string> PermissionName = new("PermissionName", DbType.String);
    public static readonly Column<Guid> RoleId = new("RoleId", DbType.Guid);
    public static readonly Column<string> RoleName = new("RoleName", DbType.String);
    public static readonly Column<string> RegisteredBy = new("RegisteredBy", DbType.String);
    public static readonly Column<Guid> SessionToken = new("SessionToken", DbType.Guid);
    public static readonly Column<int> Status = new("Status", DbType.Int32);
    public static readonly Column<byte[]> Token = new("Token", DbType.Binary);
}