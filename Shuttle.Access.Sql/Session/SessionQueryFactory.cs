using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
	public class SessionQueryFactory : ISessionQueryFactory
	{
		private const string SelectedColumns = @"
	Token, 
	IdentityId, 
	IdentityName, 
	DateRegistered, 
	ExpiryDate 
";

		public IQuery Get(Guid token)
		{
			return new Query(@"
select 
	Token, 
	IdentityId, 
	IdentityName, 
	DateRegistered, 
	ExpiryDate 
from 
	[dbo].[Session] 
where 
	Token = @Token
")
				.AddParameter(Columns.Token, token);
		}

        public IQuery Get(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));
            
            return new Query(@"
select 
	Token, 
	IdentityId, 
	IdentityName, 
	DateRegistered, 
	ExpiryDate 
from 
	[dbo].[Session] 
where 
	IdentityName = @IdentityName
")
                .AddParameter(Columns.IdentityName, identityName);
        }

		public IQuery GetPermissions(Guid token)
		{
			return new Query(@"
select 
	PermissionName 
from 
	[dbo].[SessionPermission] 
where 
	Token = @Token
")
				.AddParameter(Columns.Token, token);
		}

		public IQuery Remove(string identityName)
		{
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

			return new Query(@"
delete 
from 
	[dbo].[Session] 
where 
	IdentityName = @IdentityName
")
				.AddParameter(Columns.IdentityName, identityName);
		}

		public IQuery Add(Session session)
		{
            Guard.AgainstNull(session, nameof(session));
            
			return new Query(@"
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
				.AddParameter(Columns.Token, session.Token)
				.AddParameter(Columns.IdentityName, session.IdentityName)
				.AddParameter(Columns.IdentityId, session.IdentityId)
				.AddParameter(Columns.DateRegistered, session.DateRegistered)
				.AddParameter(Columns.ExpiryDate, session.ExpiryDate);
		}

		public IQuery AddPermission(Guid token, string permission)
		{
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));
            
			return new Query(@"
insert into [dbo].[SessionPermission]
(
	Token,
	PermissionName
)
values
(
	@Token,
	@PermissionName
)
")
				.AddParameter(Columns.Token, token)
				.AddParameter(Columns.PermissionName, permission);
		}

		public IQuery Remove(Guid token)
		{
			return new Query(@"
delete 
from 
	[dbo].[Session] 
here 
	Token = @Token
")
				.AddParameter(Columns.Token, token);
		}

	    public IQuery Contains(Guid token)
	    {
            return new Query(@"
if exists 
(
	select 
		null 
	from 
		[dbo].[Session] 
where 
	Token = @Token
) 
	select 1 else select 0
")
                .AddParameter(Columns.Token, token);
        }

	    public IQuery Contains(Guid token, string permission)
	    {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            return new Query(@"
if exists 
(
	select 
		null 
	from 
		[dbo].[SessionPermission] 
	where 
		Token = @Token 
	and 
		PermissionName = @PermissionName
) 
	select 1 else select 0
")
                .AddParameter(Columns.Token, token)
                .AddParameter(Columns.PermissionName, permission);
        }

        public IQuery Renew(Session session)
        {
            return new Query(@"
update
    [dbo].[Session]
set
    Token = @Token,
    ExpiryDate = @ExpiryDate
where
    IdentityName = @IdentityName
")
                .AddParameter(Columns.Token, session.Token)
                .AddParameter(Columns.ExpiryDate, session.ExpiryDate)
                .AddParameter(Columns.IdentityName, session.IdentityName);
        }

        public IQuery Search(DataAccess.Query.Session.Specification specification)
        {
			return Specification(specification, true);
		}

        private IQuery Specification(DataAccess.Query.Session.Specification specification, bool columns)
        {
			Guard.AgainstNull(specification, nameof(specification));

			return new Query($@"
select distinct
{(columns ? SelectedColumns : "count (*)")}
from
	[dbo].[Session] 
where
(
	1 = 1
)
{(!specification.Permissions.Any() ? string.Empty : $@"
and
    Token in (select distinct Token from [dbo].[SessionPermission] where PermissionName in ({string.Join(",", specification.Permissions.Select(item => $"'{item}'"))}))
")}
");
        }
	}
}