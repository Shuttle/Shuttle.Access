using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionMapper : IDataRowMapper<Session>
{
    public MappedRow<Session> Map(DataRow row)
    {
        return new(row, new(
            Columns.Token.Value(row),
            Columns.IdentityId.Value(row),
            Columns.IdentityName.Value(row),
            Columns.DateRegistered.Value(row),
            Columns.ExpiryDate.Value(row)));
    }
}