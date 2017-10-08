using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserColumns
    {
        public static readonly MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
        public static readonly MappedColumn<string> Username = new MappedColumn<string>("Username", DbType.String, 65);

        public static readonly MappedColumn<DateTime> DateRegistered =
            new MappedColumn<DateTime>("DateRegistered", DbType.DateTime);

        public static readonly MappedColumn<string> RegisteredBy =
            new MappedColumn<string>("RegisteredBy", DbType.String, 65);
    }
}