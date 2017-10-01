using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SystemRoleColumns
	{
		public static readonly MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
		public static readonly MappedColumn<string> RoleName = new MappedColumn<string>("RoleName", DbType.String, 130);
	}
}