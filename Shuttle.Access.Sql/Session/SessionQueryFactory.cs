using System;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionQueryFactory : ISessionQueryFactory
	{
		public IQuery Get(Guid token)
		{
			return RawQuery.Create("select Token, IdentityId, IdentityName, DateRegistered, ExpiryDate from [dbo].[Session] where Token = @Token")
				.AddParameterValue(Columns.Token, token);
		}

		public IQuery GetPermissions(Guid token)
		{
			return RawQuery.Create("select Permission from [dbo].[SessionPermission] where Token = @Token")
				.AddParameterValue(Columns.Token, token);
		}

		public IQuery Remove(string identityName)
		{
			return RawQuery.Create("delete from [dbo].[Session] where IdentityName = @IdentityName")
				.AddParameterValue(Columns.IdentityName, identityName);
		}

		public IQuery Add(Session session)
		{
			return RawQuery.Create(@"
insert into [dbo].[Session] 
(
	Token, 
	IdentityId, 
	IdentityName, 
	DateRegistered,
    ExpiryDate
)
values
(
	@Token, 
	@IdentityId, 
	@IdentityName, 
	@DateRegistered,
    @ExpiryDate
)
")
				.AddParameterValue(Columns.Token, session.Token)
				.AddParameterValue(Columns.IdentityName, session.IdentityName)
				.AddParameterValue(Columns.IdentityId, session.IdentityId)
				.AddParameterValue(Columns.DateRegistered, session.DateRegistered)
				.AddParameterValue(Columns.ExpiryDate, session.ExpiryDate);
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
				.AddParameterValue(Columns.Token, token)
				.AddParameterValue(Columns.Permission, permission);
		}

		public IQuery Remove(Guid token)
		{
			return RawQuery.Create("delete from [dbo].[Session] where Token = @Token")
				.AddParameterValue(Columns.Token, token);
		}

	    public IQuery Contains(Guid token)
	    {
            return RawQuery.Create("if exists (select null from [dbo].[Session] where Token = @Token) select 1 else select 0")
                .AddParameterValue(Columns.Token, token);
        }

	    public IQuery Contains(Guid token, string permission)
	    {
            return RawQuery.Create("if exists (select null from [dbo].[SessionPermission] where Token = @Token and Permission = @Permission) select 1 else select 0")
                .AddParameterValue(Columns.Token, token)
                .AddParameterValue(Columns.Permission, permission);
        }

        public IQuery Renew(Session session)
        {
            return RawQuery.Create(@"
update
    [dbo].[Session]
set
    Token = @Token,
    ExpiryDate = @ExpiryDate
where
    IdentityName = @IdentityName
")
                .AddParameterValue(Columns.Token, session.Token)
                .AddParameterValue(Columns.ExpiryDate, session.ExpiryDate)
                .AddParameterValue(Columns.IdentityName, session.IdentityName);
        }
    }
}