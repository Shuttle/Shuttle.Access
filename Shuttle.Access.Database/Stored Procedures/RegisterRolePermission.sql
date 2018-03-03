CREATE PROCEDURE [dbo].[RegisterRolePermission]
	@RoleName varchar(130),
	@Permission varchar(130)
AS
	declare @Id uniqueidentifier

	select @Id = Id from SystemRole where RoleName = @RoleName

	if (@Id is null)
		return 0;

	if exists (select null from SystemRolePermission where RoleId = @Id and Permission = @Permission)
		return;

	insert into SystemRolePermission
	(
		RoleId,
		Permission
	)
	values
	(
		@id,
		@Permission
	)