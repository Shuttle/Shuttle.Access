using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionPermissionColumns
	{
		public static readonly MappedColumn<Guid> Token = new MappedColumn<Guid>("Token", DbType.Guid);
		public static readonly MappedColumn<string> Permission = new MappedColumn<string>("Permission", DbType.String, 130);
	}
}