CREATE PROCEDURE [dbo].[RegisterRolePermission]
	@RoleName varchar(130),
	@Permission varchar(130)
AS
	declare @Id uniqueidentifier

	select @Id = Id from Role where RoleName = @RoleName

	if (@Id is null)
		return 0;

	if exists (select null from RolePermission where RoleId = @Id and Permission = @Permission)
		return;

	insert into RolePermission
	(
		RoleId,
		Permission
	)
	values
	(
		@id,
		@Permission
	)