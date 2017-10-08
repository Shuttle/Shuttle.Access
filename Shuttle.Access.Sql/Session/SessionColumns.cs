using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionColumns
	{
		public static readonly MappedColumn<Guid> Token = new MappedColumn<Guid>("Token", DbType.Guid);
		public static readonly MappedColumn<string> Username = new MappedColumn<string>("Username", DbType.String, 65);
		public static readonly MappedColumn<DateTime> DateRegistered = new MappedColumn<DateTime>("DateRegistered", DbType.DateTime);
	}
}