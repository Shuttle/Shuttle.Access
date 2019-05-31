using System;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionQueryFactory : ISessionQueryFactory
	{
		public IQuery Get(Guid token)
		{
			return RawQuery.Create("select Token, Username, DateRegistered from [dbo].[Session] where Token = @Token")
				.AddParameterValue(SessionColumns.Token, token);
		}

		public IQuery GetPermissions(Guid token)
		{
			return RawQuery.Create("select Permission from [dbo].[SessionPermission] where Token = @Token")
				.AddParameterValue(SessionColumns.Token, token);
		}

		public IQuery Remove(string username)
		{
			return RawQuery.Create("delete from [dbo].[Session] where Username = @Username")
				.AddParameterValue(SessionColumns.Username, username);
		}

		public IQuery Add(Session session)
		{
			return RawQuery.Create(@"
insert into [dbo].[Session] 
(
	Token, 
	Username, 
	DateRegistered
)
values
(
	@Token, 
	@Username, 
	@DateRegistered
)
")
				.AddParameterValue(SessionColumns.Token, session.Token)
				.AddParameterValue(SessionColumns.Username, session.Username)
				.AddParameterValue(SessionColumns.DateRegistered, session.DateRegistered);
		}

		public IQuery AddPermission(Guid token, string permission)
		{
			return RawQuery.Create(@"
insert into [dbo].[SessionPermission]
(
	Token,
	Permission
)
values
(
	@Token,
	@Permission
)
")
				.AddParameterValue(SessionPermissionColumns.Token, token)
				.AddParameterValue(SessionPermissionColumns.Permission, permission);
		}

		public IQuery Remove(Guid token)
		{
			return RawQuery.Create("delete from [dbo].[Session] where Token = @Token")
				.AddParameterValue(SessionColumns.Token, token);
		}

	    public IQuery Contains(Guid token)
	    {
            return RawQuery.Create("if exists (select null from [dbo].[Session] where Token = @Token) select 1 else select 0")
                .AddParameterValue(SessionColumns.Token, token);
        }

	    public IQuery Contains(Guid token, string permission)
	    {
            return RawQuery.Create("if exists (select null from [dbo].[SessionPermission] where Token = @Token and Permission = @Permission) select 1 else select 0")
                .AddParameterValue(SessionPermissionColumns.Token, token)
                .AddParameterValue(SessionPermissionColumns.Permission, permission);
        }

	    public IQuery Renewed(Session session)
	    {
	        return RawQuery.Create("update [dbo].[Session] set Token = @Token where Username = @Username")
	            .AddParameterValue(SessionColumns.Token, session.Token)
	            .AddParameterValue(SessionColumns.Username, session.Username);
	    }
	}
}