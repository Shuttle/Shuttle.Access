using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionMapper : IDataRowMapper<Session>
	{
		public MappedRow<Session> Map(DataRow row)
		{
			return new MappedRow<Session>(row, new Session(
				Columns.Token.MapFrom(row), 
                Columns.UserId.MapFrom(row), 
				Columns.Username.MapFrom(row),
				Columns.DateRegistered.MapFrom(row),
                Columns.ExpiryDate.MapFrom(row)));
		}
	}
}